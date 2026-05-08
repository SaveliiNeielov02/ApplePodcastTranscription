using ApplePodcastTranscription.Models;
using System.Runtime.CompilerServices;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IPodcastRecordRepository
    {
        public Task AddPodcastRecordAsync(string externalId);
        public Task<PodcastRecord?> GetPodcastRecordAsync(string externalId);
        public Task<IEnumerable<PodcastRecord>> GetAllPodcastRecordsAsync();
        public Task UpdateTranscriptionStatusAsync(PodcastRecord entity, PodcastTranscriptionStatus status);
        public Task UpdateTranscriptionErrorAsync(PodcastRecord entity, PodcastTranscriptionError error);
        public Task UpdateTranscriptionTextAsync(PodcastRecord entity, string transcriptionText);
        public Task UpdateDownloadedAtAsync(PodcastRecord entity, long downloadedAtInUnixTimeSeconds);   
        public Task UpdateTranscribedAtAsync(PodcastRecord entity, long transcribedAtInUnixTimeSeconds);

    }
}
