using ApplePodcastTranscription.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.ObjectModel;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class ApplePodcastDownloadingTest
    {
        [Fact]
        public async Task DownloadPodcastEpisodeAsync_ShouldReturnAudioContent()
        {
            // Arrange
            var podcastId = "1000764234023";
            bool saveLocally = true;

            // Mock the HttpClientFactory
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient());

            // Mock the logger
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<ApplePodcastTranscription.Services.ApplePodcastDownloader>>();

            // Mock the configuration
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var applePodcastDownloader = new ApplePodcastTranscription.Services.ApplePodcastDownloader(
                httpClientFactory.Object,
                logger.Object,
               configuration);

            // Act
            var result = await applePodcastDownloader.DownloadPodcastEpisodeAsync(podcastId, saveLocally);

            // Assert
            Assert.True(result != null && result.Length > 0);
        }
    }
}
