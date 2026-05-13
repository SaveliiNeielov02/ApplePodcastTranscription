namespace ApplePodcastTranscription.Models.PodcastDto
{
    public class AudioPodcastDto : PodcastDtoBase
    {
        // The key used to store the audio file in the storage service, can be the same as AudioFilePath or a different value depending on the implementation of IPodcastFileManager
        public required string StorageKey { get; set; } 
    }
}
