using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class ApplePodcacstTranscriptionTest
    {
        [Fact]
        public async Task TranscriptPodcastEpisode_ShouldReturnText()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplePodcastTranscription.Services.WhisperTranscriber>>();
            var audioResampler = new ApplePodcastTranscription.Services.WhisperWaveAudioResampler();

            var transcriber = new ApplePodcastTranscription.Services.WhisperTranscriber(loggerMock.Object, audioResampler);
            var audioFilePath = Path.Combine("DownloadedPodcasts", "1000764234023.wave");

            // Act
            var transribed = await transcriber.TranscribeFileAsync(audioFilePath);

            // Assert
            Assert.False(string.IsNullOrEmpty(transribed));
        }    
    }   
}
