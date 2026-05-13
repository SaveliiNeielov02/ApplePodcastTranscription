using ApplePodcastTranscription.Models.PodcastDto;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IApplePodcastDownloader
    {
        public Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(string externalPodcastId, string fileName, CancellationToken cancellationToken);
    }
}
