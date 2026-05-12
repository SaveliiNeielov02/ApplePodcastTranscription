using System.Collections.ObjectModel;

namespace ApplePodcastTranscription.Interfaces
{
    public interface ITranscriber
    {
        public Task<string> TranscribeBytesAsync(byte[] audioContent, CancellationToken ct);
        public Task<string> TranscribeFileAsync(string filePath, CancellationToken ct);
    }
}
