using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.Notification;
using ApplePodcastTranscription.Services.Transcript;
using MediatR;
using PodcastModelsLibrary;
using System.Threading.Channels;

namespace ApplePodcastTranscription.Services.Session
{
    public class LocalTranscriptQueue : ITranscriptQueue
    {
        private readonly Channel<TranscriptQueueItem> _queue;

        public LocalTranscriptQueue()
        {
            var options = new UnboundedChannelOptions { SingleReader = true };
            _queue = Channel.CreateUnbounded<TranscriptQueueItem>(options);
        }

        public async ValueTask EnqueueSessionAsync(Guid sessionGuid, string storageKey, CancellationToken ct = default)
        {
            await _queue.Writer.WriteAsync(new TranscriptQueueItem { SessionGuid = sessionGuid, StorageKey = storageKey }, ct);
        }

        public async ValueTask<TranscriptQueueItem> DequeueAsync(CancellationToken ct)
        {
            return await _queue.Reader.ReadAsync(ct);
        }
    }
}
