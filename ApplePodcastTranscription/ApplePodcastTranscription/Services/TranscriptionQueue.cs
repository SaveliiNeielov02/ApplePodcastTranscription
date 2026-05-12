using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Services.Database;

namespace ApplePodcastTranscription.Services
{
    public class TranscriptionQueue
    {
        private const int SemaphoreThreshold = 1; // Only one transcription at a time
        
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _queueSemaphore;
        public TranscriptionQueue(ILogger logger) 
        {
            _queueSemaphore = new SemaphoreSlim(1, SemaphoreThreshold);
            _logger = logger;
        }
        public async Task ProcessTranscriptionAsync(
            IServiceProvider services,
            TranscriptSession transcriptSession,
            CancellationToken queueCt,
            CancellationToken transcriptCt,
            byte[] audioContent) 
        {
            var transcriber = services.GetRequiredService<ITranscriber>();
            var podcastRepository = services.GetRequiredService<IPodcastRecordRepository>();
            var podcastSessionRepository = services.GetRequiredService<ISessionRepository>();

            await podcastSessionRepository!.UpdateSessionStatusAsync(transcriptSession, SessionStatus.InQueue);

            bool isLockAcquired = false;

            try
            {
                await _queueSemaphore.WaitAsync(queueCt); // Wait for the semaphore to be available

                isLockAcquired = true;

                _logger.LogInformation("Starting transcription for session {SessionGuid}", transcriptSession.Guid);

                await podcastSessionRepository!.UpdateSessionStatusAsync(transcriptSession, SessionStatus.InProgress);

                var transcription = await transcriber!.TranscribeBytesAsync(audioContent, transcriptCt);

                await podcastRepository!.AddPodcastTranscriptionAsync(transcriptSession.PodcastRecord, transcription, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                
                await podcastSessionRepository!.UpdateSessionStatusAsync(transcriptSession, SessionStatus.Completed);
            }
            finally
            {
                if (isLockAcquired)
                {
                    _logger.LogInformation("Finished processing transcription for session {SessionGuid}", transcriptSession.Guid);
                    _queueSemaphore.Release(); // Release the semaphore to allow the next transcription
                }
            }
        }
    }
}
