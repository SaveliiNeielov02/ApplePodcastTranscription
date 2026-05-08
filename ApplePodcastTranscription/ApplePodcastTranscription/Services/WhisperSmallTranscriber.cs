using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Services.Abstract;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.ObjectModel;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;


namespace ApplePodcastTranscription.Services
{
    public class WhisperSmallTranscriber : IAsyncTranscriber, IDisposable
    {
        private WhisperFactory _factory;
        private WhisperProcessor _processor;
        private readonly WhisperSmallAudioResampler _audioResampler;
        private readonly ILogger _logger;

        private readonly string _modelPath = Path.Combine("Resources", "STTModels", "WhisperSmall", "ggml-small.bin");

        public WhisperSmallTranscriber(ILogger logger, WhisperSmallAudioResampler audioResampler) 
        {
            if (!File.Exists(_modelPath)) 
            {
                throw new FileNotFoundException($"Model file not found at path: {_modelPath}");
            }

            _logger = logger;
            _audioResampler = audioResampler;
            _factory = WhisperFactory.FromPath(_modelPath, new WhisperFactoryOptions());
            _processor = _factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();
        }
        public async Task<string> TranscribeBytesAsync(byte[] audioContent)
        {
            throw new NotImplementedException();
        }

        public async Task<string> TranscribeFileAsync(string filePath)
        {
            var audioSamples = _audioResampler.ResampleFile(filePath);
            var fullText = new List<string>();

            await foreach (var segment in _processor.ProcessAsync(audioSamples.ToArray()))
            {
                _logger.LogInformation("[{FileName}] Transcribed segment: [{Start} - {End}] {Text}", filePath, segment.Start, segment.End, segment.Text);
                fullText.Add(segment.Text);
            }

            return string.Join(" ", fullText);
        }
        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
        }

    }
}
