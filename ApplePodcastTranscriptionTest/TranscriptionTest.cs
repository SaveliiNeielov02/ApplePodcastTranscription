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
            var audioResampler = new WhisperSmallAudioResampler();
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var transcriber = new WhisperSmallTranscriber(loggerMock.Object, configuration, audioResampler);
            var audioFilePath = Path.Combine("DownloadedPodcasts", "1000767418728.wave");

            await using var audioStream = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read);
            // Act
            var transcribed = await transcriber.TranscribeStreamAsync(Guid.NewGuid(), audioStream, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrEmpty(transcribed));
        }
    }   
}
