using ApplePodcastTranscription.Models.PodcastDto;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IApplePodcastDownloader
    {
        public Task<AssetPodcastDto> GetPodcastData(string podcastId);
        public Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(AssetPodcastDto podcastData, string fileName, CancellationToken cancellationToken);
    }
}
