using System.Collections.ObjectModel;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IAsyncTranscriber
    {
        public Task<string> TranscribeBytesAsync(byte[] audioContent);
        public Task<string> TranscribeFileAsync(string filePath);
    }
}
