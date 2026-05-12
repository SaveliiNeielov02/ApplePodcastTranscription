using ApplePodcastTranscription.Services.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class TranscriptionTest
    {
        [Fact]
        public async Task TranscriptPodcastEpisode_ShouldReturnText()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplePodcastTranscription.Services.WhisperSmallTranscriber>>();
            var audioResampler = new ApplePodcastTranscription.Services.WhisperSmallWaveAudioResampler();

            var transcriber = new ApplePodcastTranscription.Services.WhisperSmallTranscriber(loggerMock.Object, audioResampler);
            var audioFilePath = Path.Combine("DownloadedPodcasts", "1000764234023.wave");

            // Act
            var transribed = await transcriber.TranscribeFileAsync(audioFilePath, new());

            // Assert
            Assert.False(string.IsNullOrEmpty(transribed));
        }    
    }   
}
