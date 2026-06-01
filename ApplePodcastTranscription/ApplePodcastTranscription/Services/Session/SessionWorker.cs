using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.Notification;
using ApplePodcastTranscription.Models.PodcastDto;
using MediatR;
using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Services.Session
{
    public class SessionWorker
    {
        private const int DownloadTimeoutInMinutes = 2;

        private readonly ISessionRepository _sessionRepository;
        private readonly IPodcastRecordRepository _podcastRecordRepository;

        private readonly ITranscriptQueue _transcriptQueue;
        private readonly IApplePodcastDownloader _podcastDownloader;
        private readonly ILogger<SessionWorker> _logger;
        private readonly IMediator _mediator;

        public SessionWorker(
            ISessionRepository sessionRepository,
            IMediator mediator,
            IPodcastRecordRepository podcastRecordRepository,
            ITranscriptQueue transcriptQueue,
            IApplePodcastDownloader podcastDownloader,
            ILogger<SessionWorker> logger)
        {
            _sessionRepository = sessionRepository;
            _podcastRecordRepository = podcastRecordRepository;
            _transcriptQueue = transcriptQueue;
            _podcastDownloader = podcastDownloader;
            _logger = logger;
            _mediator = mediator;
        }
        public async Task StartTranscriptPipeline(Guid sessionGuid, string externalPodcastId)
        {
            try
            {
                _logger.LogInformation("Starting to process podcast session for Id: {PodcastId}", externalPodcastId);

                var session = await InitializeNewSession(sessionGuid, externalPodcastId);
                var audioPodcastDto = await DownloadPodcastAsync(session);

                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.InQueue);
                await _mediator.Publish(new UpdateSession(session.Guid));

                // New task creating to prevent long-running SessionWorker and to dispose scoped services
                _ = Task.Run(async () => await _transcriptQueue.EnqueueSessionAsync(session.Guid, audioPodcastDto.StorageKey));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline exception for podcast Id: {PodcastId}", externalPodcastId);
            }

        }
        private async Task<TranscriptSession> InitializeNewSession(Guid sessionGuid, string externalPodcastId)
        {
            try
            {
                var podcastRecordWithSameId = await _podcastRecordRepository.GetPodcastRecordByExternalId(externalPodcastId);
                var podcastRecordToAdd = podcastRecordWithSameId ?? new PodcastRecord { ExternalId = externalPodcastId };

                var session = new TranscriptSession
                {
                    Guid = sessionGuid,
                    PodcastRecordId = externalPodcastId,
                    PodcastRecord = podcastRecordToAdd,
                    TranscriptionStatus = SessionStatus.Pending,
                    TranscriptionError = SessionError.None,
                    CreatedAtInUnixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                
                await _sessionRepository.AddSessionAsync(session);
                await _mediator.Publish(new UpdateSession(session.Guid));

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while initializing podcast session for Id: {PodcastId}", externalPodcastId);
                throw;
            }
        }
        private async Task<AudioPodcastDto> DownloadPodcastAsync(TranscriptSession session)
        {
            var externalId = session.PodcastRecord.ExternalId;

            try
            {
                _logger.LogInformation("Retrieving podcast data for Id: {PodcastId}", externalId);

                var podcastData = await _podcastDownloader.GetPodcastData(externalId);

                await _podcastRecordRepository.UpdatePodcastRecordAsync(session.PodcastRecord, podcastData);
                await _mediator.Publish(new UpdateSession(session.Guid));

                _logger.LogInformation("Starting download for podcast episode Id: {PodcastId}", externalId);

                using var downloadCts = new CancellationTokenSource(TimeSpan.FromMinutes(DownloadTimeoutInMinutes));
                var podcastDto = await _podcastDownloader.DownloadPodcastEpisodeAsync(podcastData, session.Guid.ToString(), downloadCts.Token);

                _logger.LogInformation("Downloaded podcast episode for Id: {PodcastId}", externalId);

                return podcastDto;
            }
            catch (DownloadTimeoutException ex)
            {
                _logger.LogError(ex, "Download timed out for podcast episode Id: {PodcastId}", externalId);
                await HandleDownloadErrorAsync(session, SessionError.DownloadTimeout);
                throw;
            }
            catch (DownloadException ex)
            {
                _logger.LogError(ex, "Failed to download podcast episode for Id: {PodcastId}", externalId);
                await HandleDownloadErrorAsync(session, SessionError.DownloadFailed);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while downloading podcast episode for Id: {PodcastId}", externalId);
                await HandleDownloadErrorAsync(session, SessionError.UnknownError);
                throw;
            }
        }
        private async Task HandleDownloadErrorAsync(TranscriptSession session, SessionError errorType)
        {
            await _sessionRepository.UpdateSessionErrorAsync(session, errorType);
            await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Failed);
            await _mediator.Publish(new UpdateSession(session.Guid));
        }
    }
}
