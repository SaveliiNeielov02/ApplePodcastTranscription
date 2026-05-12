using ApplePodcastTranscription.Models;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IApplePodcastDownloader
    {
        public Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(string externalPodcastId, CancellationToken cancellationToken, bool isNeedToSaveLocally = false);
    }
}
