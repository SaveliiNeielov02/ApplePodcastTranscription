using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.Exception;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.ObjectModel;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;


namespace ApplePodcastTranscription.Services.Transcript
{
    public class WhisperSmallTranscriber : ITranscriber, IDisposable
    {
        private readonly WhisperFactory _factory;
        private readonly WhisperProcessor _processor;

        private readonly string _outputWaveFileFolder = Path.Combine(Directory.GetCurrentDirectory(), "ResampledAudio");
        private readonly WhisperSmallAudioResampler _audioResampler;
        private readonly ILogger _logger;

        private readonly string _modelPath = Path.Combine("Resources", "STTModels", "WhisperSmall", "ggml-small.bin");

        public WhisperSmallTranscriber(ILogger logger, WhisperSmallAudioResampler audioResampler) 
        {
            if (!File.Exists(_modelPath)) 
            {
                throw new FileNotFoundException($"Model file not found at path: {_modelPath}");
            }
            if (!Directory.Exists(_outputWaveFileFolder)) 
            {
                Directory.CreateDirectory(_outputWaveFileFolder);
            }
            _logger = logger;
            _audioResampler = audioResampler;

            LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);

            _factory = WhisperFactory.FromPath(_modelPath, new WhisperFactoryOptions { UseGpu = true });
            _processor = _factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();
        }
        public async Task<string> TranscribeStreamAsync(Guid sessionGuid, Stream audioStream, CancellationToken ct)
        {
            var tempFilePath = Path.Combine(_outputWaveFileFolder, $"{sessionGuid}.wave");

            try
            {
                await using var resampledAudioStream = _audioResampler.ResampleToWaveStream(audioStream, tempFilePath);
                var fullTextBuilder = new StringBuilder();

                await foreach (var segment in _processor.ProcessAsync(resampledAudioStream, ct))
                {
                    _logger.LogInformation("[{SessionGuid}] Transcribed segment: [{Start} - {End}] {Text}", sessionGuid.ToString(),
                        segment.Start, segment.End, segment.Text);

                    fullTextBuilder.Append(segment.Text).Append(' ');
                }

                return fullTextBuilder.ToString().Trim();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                throw new TranscribingException("An error occurred while transcribing the audio file", ex);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
        }

    }
}
