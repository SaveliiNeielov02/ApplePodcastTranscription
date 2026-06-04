using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.Notification;
using MediatR;
using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Services.Background
{
    public class TranscriptBackgroundWorker : BackgroundService
    {
        private readonly ITranscriptQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TranscriptBackgroundWorker> _logger;
        private readonly SemaphoreSlim _queueSemaphore;

        private const int QueueTimeoutInMinutes = 60;
        private const int TranscriptTimeoutInMinutes = 120;
        private readonly bool _isNeedToDeleteAudioFileAfterTranscription = true;

        public TranscriptBackgroundWorker(
            ITranscriptQueue queue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TranscriptBackgroundWorker> logger)
        {
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _queueSemaphore = new SemaphoreSlim(1, 1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background transcription worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var item = await _queue.DequeueAsync(stoppingToken);
                    _ = ProcessAsync(item.SessionGuid, item.StorageKey, stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading from transcript queue.");
                }
            }
        }

        private async Task ProcessAsync(Guid sessionGuid, string storageKey, CancellationToken appStoppingToken)
        {
            using var queueCts = new CancellationTokenSource(TimeSpan.FromMinutes(QueueTimeoutInMinutes));
            using var transcriptCts = new CancellationTokenSource(TimeSpan.FromMinutes(TranscriptTimeoutInMinutes));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken, transcriptCts.Token);

            bool isLockAcquired = false;
            IPodcastFileManager? podcastFileManager = null;

            try
            {
                await _queueSemaphore.WaitAsync(queueCts.Token);
                isLockAcquired = true;

                using var scope = _serviceScopeFactory.CreateScope();

                var transcriber = scope.ServiceProvider.GetRequiredService<ITranscriber>();
                var podcastRepository = scope.ServiceProvider.GetRequiredService<IPodcastRecordRepository>();
                var podcastSessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                podcastFileManager = scope.ServiceProvider.GetRequiredService<IPodcastFileManager>();

                var session = await podcastSessionRepository.GetSessionAsync(sessionGuid)
                    ?? throw new InvalidOperationException($"Session with GUID {sessionGuid} not found.");

                _logger.LogInformation("Starting transcription for session {SessionGuid}", sessionGuid);

                await podcastSessionRepository.UpdateSessionStatusAsync(session, SessionStatus.InProgress);
                await mediator.Publish(new UpdateSession(session.Guid), CancellationToken.None);

                await using var inputStream = podcastFileManager.ReadAsFileStream(storageKey);

                var transcription = await transcriber.TranscribeStreamAsync(sessionGuid, inputStream, linkedCts.Token);

                await podcastRepository.AddPodcastTranscriptionAsync(session.PodcastRecord, transcription, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await podcastSessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Completed);
                await mediator.Publish(new UpdateSession(session.Guid), CancellationToken.None);
            }
            catch (TranscribingException ex)
            {
                _logger.LogError(ex, "Failed to transcribe podcast episode for Guid: {Guid}", sessionGuid);
                await UpdateErrorStatusAsync(sessionGuid, SessionStatus.Failed, SessionError.TranscriptionFailed);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Transcription timed out for podcast Guid: {Guid}", sessionGuid);
                await UpdateErrorStatusAsync(sessionGuid, SessionStatus.Failed, SessionError.TranscriptionTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing podcast transcription for Guid: {Guid}", sessionGuid);
                await UpdateErrorStatusAsync(sessionGuid, SessionStatus.Failed, SessionError.UnknownError);
            }
            finally
            {
                if (isLockAcquired)
                {
                    _logger.LogInformation("Finished processing transcription for session {SessionGuid}", sessionGuid);
                    _queueSemaphore.Release();
                }

                if (_isNeedToDeleteAudioFileAfterTranscription && podcastFileManager != null)
                {
                    DeleteAudioFile(podcastFileManager, storageKey);
                }
            }
        }

        private void DeleteAudioFile(IPodcastFileManager fileManager, string storageKey)
        {
            try
            {
                fileManager.DeleteFile(storageKey);
                _logger.LogInformation("Deleted audio file at path: {FilePath}", storageKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the audio file at path: {FilePath}", storageKey);
            }
        }

        private async Task UpdateErrorStatusAsync(Guid sessionGuid, SessionStatus status, SessionError error)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var sessionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var session = await sessionRepo.GetSessionAsync(sessionGuid);
                if (session != null)
                {
                    if (error != SessionError.None)
                    {
                        await sessionRepo.UpdateSessionErrorAsync(session, error);
                    }
                    await sessionRepo.UpdateSessionStatusAsync(session, status);
                    await mediator.Publish(new UpdateSession(sessionGuid), CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save error state for session {SessionGuid}", sessionGuid);
            }
        }
    }
}
