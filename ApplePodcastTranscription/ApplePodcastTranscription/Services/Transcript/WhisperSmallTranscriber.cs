using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.Notification;
using ApplePodcastTranscription.Services.Hub;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace ApplePodcastTranscription.Services.Transcript
{
    public class WhisperSmallTranscriber : ITranscriber, IDisposable
    {
        private bool _isModelInitialized = false;
        private readonly bool _useGpu;
        private readonly GgmlType _ggmlType = GgmlType.Small;

        private WhisperFactory _factory;
        private WhisperProcessor _processor;

        private readonly IAudioResampler _audioResampler;
        private readonly ILogger<WhisperSmallTranscriber> _logger;
        private readonly IMediator _mediator;

        private readonly string _modelPath = Path.Combine("Resources", "STTModels", "WhisperSmall", "ggml-small.bin");

        public WhisperSmallTranscriber(ILogger<WhisperSmallTranscriber> logger,
            IMediator mediator,
            IConfiguration configuration,
            IAudioResampler audioResampler)
        {
            _logger = logger;
            _audioResampler = audioResampler;
            _mediator = mediator;

            _useGpu = configuration.GetValue<bool?>("UseGpuForTranscription") ?? true;
        }

        private async Task EnsureInitialized()
        {
            if(_isModelInitialized) return;

            _logger.LogInformation("Initializing Whisper model...");
            
            var modelDir = Path.GetDirectoryName(_modelPath);
            if (!Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir!);
            }

            if (!File.Exists(_modelPath))
            {
                _logger.LogInformation("No model found at {modelPath}. Downloading...", _modelPath);

                await DownloadModel(_modelPath, _ggmlType);
            }
            
            _factory = WhisperFactory.FromPath(_modelPath, new WhisperFactoryOptions { UseGpu = _useGpu });
            _processor = _factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();

            _isModelInitialized = true;
        }

        public async Task<string> TranscribeStreamAsync(Guid sessionGuid, Stream audioStream, CancellationToken ct)
        {
            await EnsureInitialized();
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
                        new TranscriptProgressNotification(sessionGuid, (int)(segment.End / audioDuration * 100)),
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
        private async Task DownloadModel(string fileName, GgmlType ggmlType)
        {
            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
            await using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter);
        }
        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
        }

    }
}
