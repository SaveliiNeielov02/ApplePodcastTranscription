using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Session;
using ApplePodcastTranscription.Services.Transcript;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class PodcastDownloadingTest
    {
        [Fact]
        public async Task DownloadPodcastEpisodeAsync_ShouldReturnAudioContent()
        {
            // Arrange
            var podcastId = "1000767595273";
            var buffer = new byte[512];

            // Mock the HttpClientFactory
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(h => h.CreateClient("ApplePodcast")).Returns(() =>
            {
                var httpHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                };
                var httpClient = new HttpClient(httpHandler);
                return httpClient;
            });

            // Mock file IO
            var podcastFileIo = new LocalPodcastFileManager();

            // Mock the logger
            var loggerMock = new Mock<ILogger<DirectApplePodcastDownloader>>();

            // Mock the configuration
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var applePodcastDownloader = new DirectApplePodcastDownloader(
                httpClientFactory.Object,
               configuration,
               loggerMock.Object,
                podcastFileIo);

            // Act
            var podcastData = await applePodcastDownloader.GetPodcastData(podcastId);
            var result = await applePodcastDownloader.DownloadPodcastEpisodeAsync(podcastData, podcastId, CancellationToken.None);

            var storageKey = result.StorageKey;
            var audioContent = podcastFileIo.ReadAsFileStream(storageKey);
            var bytesRead = await audioContent.ReadAsync(buffer, 0, buffer.Length);

            // Assert
            Assert.True(bytesRead > 0);
        }
    }
}
