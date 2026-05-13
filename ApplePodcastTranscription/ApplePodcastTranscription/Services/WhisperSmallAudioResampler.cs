using ApplePodcastTranscription.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Services
{
    public class WhisperSmallAudioResampler : IAudioResampler
    {
        // 16kHz mono 16-bit PCM is the required format for Whisper small model 
        public Stream ResampleToWaveStream(Stream inputStream, string outputWaveFilePath)
        {
            using var reader = new StreamMediaFoundationReader(inputStream);
            var sampleProvider = reader.ToSampleProvider();

            var resampler = new WdlResamplingSampleProvider(
                reader.WaveFormat.Channels == 1 ? sampleProvider : new StereoToMonoSampleProvider(sampleProvider),
                16000);

            var pcmProvider = resampler.ToWaveProvider16();

            WaveFileWriter.CreateWaveFile(outputWaveFilePath, pcmProvider);

            return new FileStream(outputWaveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
        }
    }
}
