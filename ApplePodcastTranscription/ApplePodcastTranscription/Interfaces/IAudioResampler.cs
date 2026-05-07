using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Interfaces
{
    public interface IAudioResampler
    {
        public abstract float[] ResampleBytes(byte[] audioContent);
        public float[] ResampleFile(string filePath);
    }
}
