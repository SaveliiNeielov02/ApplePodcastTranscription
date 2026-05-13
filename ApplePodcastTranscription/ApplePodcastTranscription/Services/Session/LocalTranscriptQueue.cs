using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Services.Database;

namespace ApplePodcastTranscription.Services.Session
{
    public class LocalTranscriptQueue : ITranscriptQueue
    {
        private const int SemaphoreThreshold = 1; // Only one transcription at a time

        private const int QueueTimeoutInMinutes = 24 * 60; // 24 hours, effectively no timeout for waiting in the queue
        private const int TranscriptTimeoutInMinutes = 15; // 15 minutes for the transcription process itself

        private readonly bool _isNeedToDeleteAudioFileAfterTranscription;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _queueSemaphore;
        private readonly IPodcastFileManager _podcastFileManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public LocalTranscriptQueue(ILogger logger, IConfiguration configuration, IPodcastFileManager podcastFileManager, IServiceScopeFactory serviceScopeFactory) 
        {
            _queueSemaphore = new SemaphoreSlim(1, SemaphoreThreshold);
            _logger = logger;
            _isNeedToDeleteAudioFileAfterTranscription = configuration.GetValue<bool?>("DeleteAudioFileAfterTranscription") ?? true;
            _podcastFileManager = podcastFileManager;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task EnqueueSessionAsync(Guid sessionGuid, string storageKey) 
        {
            using var queueCts = new CancellationTokenSource(TimeSpan.FromMinutes(QueueTimeoutInMinutes));
            using var transcriptCts = new CancellationTokenSource(TimeSpan.FromMinutes(TranscriptTimeoutInMinutes));

            bool isLockAcquired = false;
            try
            {
                await _queueSemaphore.WaitAsync(queueCts.Token); // Wait for the semaphore to be available

                isLockAcquired = true;

                using var scope = _serviceScopeFactory.CreateScope();

                var transcriber = scope.ServiceProvider.GetRequiredService<ITranscriber>();
                var podcastRepository = scope.ServiceProvider.GetRequiredService<IPodcastRecordRepository>();
                var podcastSessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                var session = await podcastSessionRepository.GetSessionAsync(sessionGuid) 
                    ?? throw new InvalidOperationException($"Session with GUID {sessionGuid} not found.");

                _logger.LogInformation("Starting transcription for session {SessionGuid}", sessionGuid);

                await podcastSessionRepository.UpdateSessionStatusAsync(session, SessionStatus.InProgress);

                await using var inputStream = _podcastFileManager.ReadAsFileStream(storageKey);
                var transcription = await transcriber.TranscribeStreamAsync(sessionGuid, inputStream, transcriptCts.Token);

                await podcastRepository.AddPodcastTranscriptionAsync(session.PodcastRecord, transcription, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                
                await podcastSessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Completed);
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
                    _queueSemaphore.Release(); // Release the semaphore to allow the next transcription
                }

                if(_isNeedToDeleteAudioFileAfterTranscription)
                {
                    DeleteAudioFile(_podcastFileManager, storageKey);
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

                var session = await sessionRepo.GetSessionAsync(sessionGuid);
                if (session != null)
                {
                    if (error != SessionError.None) await sessionRepo.UpdateSessionErrorAsync(session, error);
                    await sessionRepo.UpdateSessionStatusAsync(session, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save error state for session {SessionGuid}", sessionGuid);
            }
        }
    }
}
