using ApplePodcastTranscription.Models;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IApplePodcastDownloader
    {
        public Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(string externalPodcastId, string fileName, CancellationToken cancellationToken);
    }
}
