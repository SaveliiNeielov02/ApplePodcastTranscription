using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.PodcastDto;
using ApplePodcastTranscription.Services.Database;

namespace ApplePodcastTranscription.Services.Session
{
    public class SessionWorker
    {
        private const int DownloadTimeoutInMinutes = 2;

        private readonly ISessionRepository _sessionRepository;
        private readonly IPodcastRecordRepository _podcastRecordRepository;

        private readonly ITranscriptQueue _transcriptQueue;
        private readonly IApplePodcastDownloader _podcastDownloader;
        private readonly ILogger _logger;

        public SessionWorker(
            ISessionRepository sessionRepository,
            IPodcastRecordRepository podcastRecordRepository,
            ITranscriptQueue transcriptQueue,
            IApplePodcastDownloader podcastDownloader,
            ILogger logger)
        {
            _sessionRepository = sessionRepository;
            _podcastRecordRepository = podcastRecordRepository;
            _transcriptQueue = transcriptQueue;
            _podcastDownloader = podcastDownloader;
            _logger = logger;
        }
        public async Task StartTransriptPipeline(string externalPodcastId) 
        {
            try
            {
                _logger.LogInformation("Starting to process podcast session for Id: {PodcastId}", externalPodcastId);

                var session = await InitializeNewSession(externalPodcastId);
                var audioPodcastDto = await DownloadPodcastAsync(session);

                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.InQueue);

                // New task creating to prevent long-running SessionWorker and to dispose scoped services
                _ = Task.Run(async () => await _transcriptQueue.EnqueueSessionAsync(session.Guid, audioPodcastDto.StorageKey));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Unhandled out-pipeline exception for podcast Id: {PodcastId}", externalPodcastId);
            }

        }
        private async Task<TranscriptSession> InitializeNewSession(string externalPodcastId)
        {
            try
            {
                var podcastRecord = new PodcastRecord
                {
                    ExternalId = externalPodcastId
                };
                var session = new TranscriptSession
                {
                    Guid = Guid.NewGuid(),
                    PodcastRecordId = externalPodcastId,
                    PodcastRecord = podcastRecord,
                    TranscriptionStatus = SessionStatus.Pending,
                    TranscriptionError = SessionError.None
                };

                await _sessionRepository.AddSessionAsync(session);
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
                using var downloadCts = new CancellationTokenSource(TimeSpan.FromMinutes(DownloadTimeoutInMinutes));
                var podcastDto = await _podcastDownloader.DownloadPodcastEpisodeAsync(externalId, session.Guid.ToString(), downloadCts.Token);

                _logger.LogInformation("Downloaded podcast episode for Id: {PodcastId}", externalId);

                await _podcastRecordRepository.UpdatePodcastRecordAsync(session.PodcastRecord, podcastDto);
                return podcastDto;
            }
            catch (DownloadTimeoutException ex)
            {
                _logger.LogError(ex, "Download timed out for podcast episode Id: {PodcastId}", externalId);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.DownloadTimeout);
                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Failed);
                throw;
            }
            catch (DownloadException ex)
            {
                _logger.LogError(ex, "Failed to download podcast episode for Id: {PodcastId}", externalId);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.DownloadFailed);
                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Failed);
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while downloading podcast episode for Id: {PodcastId}", externalId);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.UnknownError);
                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Failed);
                throw;
            }
        }
    }
}
