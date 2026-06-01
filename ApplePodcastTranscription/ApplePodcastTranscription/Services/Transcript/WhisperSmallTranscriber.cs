using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Services.Hub;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using ApplePodcastTranscription.Models.Notification;
using Whisper.net;

namespace ApplePodcastTranscription.Services.Transcript
{
    public class WhisperSmallTranscriber : ITranscriber, IDisposable
    {
        private readonly WhisperFactory _factory;
        private readonly WhisperProcessor _processor;

        private readonly string _outputWaveFileFolder = Path.Combine(Directory.GetCurrentDirectory(), "ResampledAudio");
        private readonly IAudioResampler _audioResampler;
        private readonly ILogger<WhisperSmallTranscriber> _logger;
        private readonly IMediator _mediator;

        private readonly string _modelPath = Path.Combine("Resources", "STTModels", "WhisperSmall", "ggml-small.bin");

        public WhisperSmallTranscriber(ILogger<WhisperSmallTranscriber> logger,
            IMediator mediator,
            IConfiguration configuration,
            IAudioResampler audioResampler)
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
            _mediator = mediator;

            var useGpu = configuration.GetValue<bool?>("UseGpuForTranscription") ?? true;
            _factory = WhisperFactory.FromPath(_modelPath, new WhisperFactoryOptions { UseGpu = useGpu });
            _processor = _factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();
        }
        public async Task<string> TranscribeStreamAsync(Guid sessionGuid, Stream audioStream, CancellationToken ct)
        {
            var resampledFilePath = _audioResampler.CreateResampledFile(audioStream);

            try
            {
                var audioDuration = _audioResampler.GetAudioDuration(resampledFilePath);
                await using var resampledAudioStream = _audioResampler.ReadResampledFileAsStream(resampledFilePath);
                var fullTextBuilder = new StringBuilder();

                await foreach (var segment in _processor.ProcessAsync(resampledAudioStream, ct))
                {
                    _logger.LogInformation("[{SessionGuid}] Transcribed segment: [{Start} - {End}] {Text}", sessionGuid.ToString(),
                        segment.Start, segment.End, segment.Text);

                    fullTextBuilder.Append(segment.Text).Append(' ');

                    await _mediator.Publish(
                        new TranscriptProgressNotification(sessionGuid.ToString(), (int)(segment.End / audioDuration * 100)),
                        CancellationToken.None);
                }

                return fullTextBuilder.ToString().Trim();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                throw new TranscribingException("An error occurred while transcribing the audio file", ex);
            }
            finally
            {
                if (File.Exists(resampledFilePath))
                {
                    File.Delete(resampledFilePath);
                }
            }
        }
        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
        }

    }
}
