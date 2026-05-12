using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;

namespace ApplePodcastTranscription.Services
{
    public class SessionWorker
    {
        private const int QueueTimeoutInMinutes = 24 * 60;
        private const int DownloadTimeoutInMinutes = 2;
        private const int TranscriptTimeoutInMinutes = 15;


        private readonly ISessionRepository _sessionRepository;
        private readonly IPodcastRecordRepository _podcastRecordRepository;

        private readonly IServiceProvider _serviceProvider;
        private readonly TranscriptionQueue _transcriptionQueue;
        private readonly ApplePodcastDownloader _podcastDownloader;
        private readonly ILogger _logger;

        public SessionWorker(
            ISessionRepository sessionRepository,
            IPodcastRecordRepository podcastRecordRepository,
            IServiceProvider serviceProvider,
            TranscriptionQueue transcriptionQueue,
            ApplePodcastDownloader podcastDownloader,
            ILogger logger)
        {
            _sessionRepository = sessionRepository;
            _podcastRecordRepository = podcastRecordRepository;
            _transcriptionQueue = transcriptionQueue;
            _podcastDownloader = podcastDownloader;
            _serviceProvider = serviceProvider;
            _logger = logger;

        }

        public async Task ProcessPodcastSessionAsync(string podcastUrl)
        {
            _logger.LogInformation("Starting to process podcast session for URL: {PodcastUrl}", podcastUrl);

            var podcastExternalId = GetPodcastExternalId(podcastUrl);
            var podcastRecord = new PodcastRecord
            {
                ExternalId = podcastExternalId
            };
            var session = new TranscriptSession
            {
                Guid = Guid.NewGuid(),
                PodcastRecordId = podcastExternalId,
                PodcastRecord = podcastRecord,
                TranscriptionStatus = SessionStatus.Pending,
                TranscriptionError = SessionError.None
            };

            await _sessionRepository.AddSessionAsync(session);

            try
            {
                using var downloadCts = new CancellationTokenSource(TimeSpan.FromMinutes(DownloadTimeoutInMinutes));
                var podcastDto = await _podcastDownloader.DownloadPodcastEpisodeAsync(podcastExternalId, downloadCts.Token);
                
                _logger.LogInformation("Downloaded podcast episode for URL: {PodcastUrl}", podcastUrl);

                await _podcastRecordRepository.UpdatePodcastRecordAsync(podcastRecord, podcastDto);

                using var queueCts = new CancellationTokenSource(TimeSpan.FromMinutes(QueueTimeoutInMinutes));
                using var transcriptCts = new CancellationTokenSource(TimeSpan.FromMinutes(TranscriptTimeoutInMinutes));

                await _transcriptionQueue.ProcessTranscriptionAsync(
                    _serviceProvider,
                    session,
                    queueCts.Token,
                    transcriptCts.Token,
                    podcastDto.AudioContent);
            }
            catch (DownloadTimeoutException ex)
            {
                _logger.LogError(ex, "Download timed out for podcast episode URL: {PodcastUrl}", podcastUrl);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.DownloadFailed);
                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Timeout);
            }
            catch (DownloadException ex) 
            {
                _logger.LogError(ex, "Failed to download podcast episode for URL: {PodcastUrl}", podcastUrl);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.DownloadFailed);
            }
            catch (TranscribingException ex)
            {
                _logger.LogError(ex, "Failed to transcribe podcast episode for URL: {PodcastUrl}", podcastUrl);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.TranscriptionFailed);
            }
            catch (OperationCanceledException ex) // Only catch Queue and Transcript timeout
            {
                _logger.LogError(ex, "Transcription timed out for podcast episode URL: {PodcastUrl}", podcastUrl);
                await _sessionRepository.UpdateSessionStatusAsync(session, SessionStatus.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing podcast session for URL: {PodcastUrl}", podcastUrl);
                await _sessionRepository.UpdateSessionErrorAsync(session, SessionError.UnknownError);
            }
        }
        public string GetPodcastExternalId(string podcastUrl) 
        {
            return podcastUrl.Split("=")[^1];
        }
    }
}
