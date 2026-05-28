using ApplePodcastTranscription.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Services.Transcript
{
    public class WhisperSmallAudioResampler : IAudioResampler
    {
        // 16kHz mono 16-bit PCM is the required format for Whisper small model 
        public string CreateResampledFile(Stream inputStream)
        {
            try
            {
                using var reader = new StreamMediaFoundationReader(inputStream);
                var sampleProvider = reader.ToSampleProvider();

                var resampler = new WdlResamplingSampleProvider(
                    reader.WaveFormat.Channels == 1 ? sampleProvider : new StereoToMonoSampleProvider(sampleProvider),
                    16000);

                var pcmProvider = resampler.ToWaveProvider16();

                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.wave");
                WaveFileWriter.CreateWaveFile(tempFilePath, pcmProvider);

                return tempFilePath;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Failed to resample the audio stream. Ensure the input stream is a valid audio format.", ex);
            }
        }

        public TimeSpan GetAudioDuration(string resampledFilePath)
        {
            if (!File.Exists(resampledFilePath))
            {
                throw new FileNotFoundException($"Resampled audio file not found at path: {resampledFilePath}");
            }

            WaveFileReader wf = new WaveFileReader(resampledFilePath);
            return wf.TotalTime;
        }

        public Stream ReadResampledFileAsStream(string resampledFilePath)
        {
            if (!File.Exists(resampledFilePath))
            {
                throw new FileNotFoundException($"Resampled audio file not found at path: {resampledFilePath}");
            }

            return new FileStream(resampledFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
        }
    }
}
