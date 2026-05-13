using ApplePodcastTranscription.Services.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Transcript;

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
            
            var transcriber = new WhisperSmallTranscriber(loggerMock.Object, audioResampler);
            var audioFilePath = Path.Combine("DownloadedPodcasts", "test_small_sample.mp3");

            await using var audioStream = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read);
            // Act
            var transcribed = await transcriber.TranscribeStreamAsync(Guid.NewGuid(), audioStream, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrEmpty(transcribed));
        }    
    }   
}
