using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using System.Runtime.CompilerServices;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IPodcastRecordRepository
    {
        public Task AddPodcastTranscriptionAsync(PodcastRecord entity, string podcastTranscription);
        public Task UpdateTranscribedAtInUnixTimeSecondsAsync(PodcastRecord entity, long transcribedAtInUnixTimeSeconds);
    }
}
