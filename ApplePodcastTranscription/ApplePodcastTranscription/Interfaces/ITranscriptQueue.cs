using ApplePodcastTranscription.Models;

namespace ApplePodcastTranscription.Interfaces
{
    public interface ITranscriptQueue
    {
        ValueTask EnqueueSessionAsync(Guid sessionGuid, string storageKey, CancellationToken ct = default);
        ValueTask<TranscriptQueueItem> DequeueAsync(CancellationToken ct);
    }
}
