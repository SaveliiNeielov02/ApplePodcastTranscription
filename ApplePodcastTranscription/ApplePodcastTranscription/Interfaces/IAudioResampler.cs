using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IAudioResampler
    {
        public Stream ResampleToWaveStream(Stream inputStream, string outputWaveFilePath);
    }
}
