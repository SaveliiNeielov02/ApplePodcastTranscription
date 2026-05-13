using NAudio.Wave;
using System.Collections.ObjectModel;

namespace ApplePodcastTranscription.Interfaces
{
    public interface ITranscriber
    {
        public Task<string> TranscribeStreamAsync(Guid sessionGuid, Stream audioStream, CancellationToken ct);
    }
}
