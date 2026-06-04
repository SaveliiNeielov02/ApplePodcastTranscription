namespace ApplePodcastTranscription.Models
{
    public class TranscriptQueueItem
    {
        public required Guid SessionGuid { get; set; }
        public required string StorageKey { get; set; }
    }
}
