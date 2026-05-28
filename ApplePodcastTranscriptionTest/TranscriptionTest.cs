using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Database;
using ApplePodcastTranscription.Services.Transcript;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ApplePodcastTranscription.Services.Hub;
using Microsoft.AspNetCore.SignalR;
using Serilog.Core;
using Whisper.net.Logger;
using Xunit.Abstractions;

namespace ApplePodcastTranscriptionTest
{
    public class TranscriptionTest
    {
        [Fact]
        public async Task TranscriptPodcastEpisode_ShouldReturnText()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WhisperSmallTranscriber>>();
            var hubMock = new Mock<IHubContext<SessionHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            var sessionGuid = Guid.NewGuid();

            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(sessionGuid.ToString())).Returns(clientProxyMock.Object);

            var audioResampler = new WhisperSmallAudioResampler();
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var transcriber = new WhisperSmallTranscriber(loggerMock.Object, hubMock.Object, configuration, audioResampler);
            var audioFilePath = Path.Combine("DownloadedPodcasts", "test_small_sample.mp3");

            await using var audioStream = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read);
            // Act
            var transcribed = await transcriber.TranscribeStreamAsync(sessionGuid, audioStream, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrEmpty(transcribed));
        }
    }   
}
