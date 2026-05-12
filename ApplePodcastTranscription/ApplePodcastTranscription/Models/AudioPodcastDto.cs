namespace ApplePodcastTranscription.Models
{
    public class AudioPodcastDto : PodcastDtoBase
    {
        public required byte[] AudioContent { get; set; }
    }
}
