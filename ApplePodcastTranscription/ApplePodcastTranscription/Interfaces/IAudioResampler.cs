using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IAudioResampler
    {
        public string CreateResampledFile(Stream inputStream);
        public TimeSpan GetAudioDuration(string resampledFilePath);
        public Stream ReadResampledFileAsStream(string resampledFilePath);
    }
}
