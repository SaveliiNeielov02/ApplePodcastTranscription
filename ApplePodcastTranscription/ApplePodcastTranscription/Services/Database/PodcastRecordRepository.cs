using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;

namespace ApplePodcastTranscription.Services.Database
{
    public class PodcastRecordRepository : IPodcastRecordRepository
    {
        private readonly ApplePodcastDbContext _context;
        public PodcastRecordRepository(ApplePodcastDbContext context)
        {
            _context = context;
        }

        public async Task AddPodcastRecordAsync(string externalId)
        {
            var newRecord = new PodcastRecord
            {
                Guid = Guid.NewGuid(),
                ExternalId = externalId,
                TranscriptionStatus = PodcastTranscriptionStatus.Obtained,
                TranscriptionError = PodcastTranscriptionError.None,
            };

            _context.PodcastsRecords.Add(newRecord);
            await _context.SaveChangesAsync();
        }

        public Task<IEnumerable<PodcastRecord>> GetAllPodcastRecordsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PodcastRecord> GetPodcastRecordAsync(string externalId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDownloadedAtAsync(PodcastRecord entity, long downloadedAtInUnixTimeSeconds)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscribedAtAsync(PodcastRecord entity, long transcribedAtInUnixTimeSeconds)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscriptionErrorAsync(PodcastRecord entity, PodcastTranscriptionError error)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscriptionStatusAsync(PodcastRecord entity, PodcastTranscriptionStatus status)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscriptionTextAsync(PodcastRecord entity, string transcriptionText)
        {
            throw new NotImplementedException();
        }
    }
}
