namespace ApplePodcastTranscription.Models
{
    public class AudioPodcastDto : PodcastDtoBase
    {
        public required string AudioFilePath { get; set; }
    }
}
