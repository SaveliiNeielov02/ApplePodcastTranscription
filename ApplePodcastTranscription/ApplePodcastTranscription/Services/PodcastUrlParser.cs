namespace ApplePodcastTranscription.Services
{
    public class PodcastUrlParser
    {
        public string GetPodcastExternalId(string podcastUrl)
        {
            return podcastUrl.Split("=")[^1];
        }
    }
}
