using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Services.Hub;
using Microsoft.AspNetCore.SignalR;
using System.Text;
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
        private readonly IHubContext<SessionHub> _hubContext;

        private readonly string _modelPath = Path.Combine("Resources", "STTModels", "WhisperSmall", "ggml-small.bin");

        public WhisperSmallTranscriber(ILogger<WhisperSmallTranscriber> logger,
            IHubContext<SessionHub> hubContext,
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
            _hubContext = hubContext;

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
                    await UpdateSessionState(sessionGuid.ToString(), segment.End, audioDuration);
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

        private async Task UpdateSessionState(string sessionGuid, TimeSpan processedSegment, TimeSpan totalDuration)
        {
            try
            {
                var transcriptState = (int)(processedSegment / totalDuration * 100);

                await _hubContext.Clients.Group(sessionGuid)
                    .SendAsync("StateChanged", sessionGuid, transcriptState, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send transcription state update for session {SessionGuid}", sessionGuid);
            }
        }

        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
        }

    }
}
