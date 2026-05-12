using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using System.Runtime.CompilerServices;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IPodcastRecordRepository
    {
        public Task AddPodcastTranscriptionAsync(PodcastRecord entity, string podcastTranscription, long transcribedAtInUnixTimeSeconds);
        public Task UpdatePodcastRecordAsync(PodcastRecord entity, PodcastDtoBase podcastDtoBase);
    }
}
