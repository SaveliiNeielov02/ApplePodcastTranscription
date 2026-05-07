using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Services.Abstract;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Services
{
    public class WhisperWaveAudioResampler : WhisperAudioResampler
    {
        public override float[] ResampleBytes(byte[] audioContent) 
        {
            using var memoryStream = new MemoryStream(audioContent);
            using var reader = new WaveFileReader(memoryStream);

            var sampleProvider = reader.ToSampleProvider();

            var monoProvider = sampleProvider.WaveFormat.Channels == 1
                ? sampleProvider
                : new StereoToMonoSampleProvider(sampleProvider);

            var resampler = new WdlResamplingSampleProvider(monoProvider, 16000);

            var audioData = new List<float>();
            float[] buffer = new float[16000]; 
            int read;
            while ((read = resampler.Read(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new float[read];
                Array.Copy(buffer, chunk, read);
                audioData.AddRange(chunk);
            }

            return audioData.ToArray();
        }
    }
}
