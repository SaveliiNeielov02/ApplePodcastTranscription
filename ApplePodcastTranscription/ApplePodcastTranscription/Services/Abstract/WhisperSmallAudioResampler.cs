using ApplePodcastTranscription.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ApplePodcastTranscription.Services.Abstract
{
    public abstract class WhisperSmallAudioResampler : IAudioResampler
    {
        // Need to be specified by concrete implementations, as the method of reading audio bytes can vary based on the format (e.g., WAV, MP3, etc.)
        public abstract float[] ResampleBytes(byte[] audioContent);

        // Universal method for resampling audio files to 16kHz mono, which is the required format for Whisper models
        public float[] ResampleFile(string filePath)
        {
            using var reader = new AudioFileReader(filePath);
            var resampler = new WdlResamplingSampleProvider(
                reader.WaveFormat.Channels == 1 ? reader : new StereoToMonoSampleProvider(reader),
                16000);

            var audioSamples = new List<float>();
            float[] buffer = new float[16000 * 5];
            int read;
            while ((read = resampler.Read(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new float[read];
                Array.Copy(buffer, chunk, read);
                audioSamples.AddRange(chunk);
            }

            return audioSamples.ToArray();
        }
    }
}
