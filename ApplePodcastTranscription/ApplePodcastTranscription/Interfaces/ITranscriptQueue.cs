namespace ApplePodcastTranscription.Interfaces
{
    public interface ITranscriptQueue
    {
        public Task EnqueueSessionAsync(Guid sessionGuid, string storageKey);
    }
}
