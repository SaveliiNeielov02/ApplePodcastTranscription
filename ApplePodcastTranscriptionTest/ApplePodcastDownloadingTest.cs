using ApplePodcastTranscription.Models;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class ApplePodcastDownloadingTest
    {
        [Fact]
        public async Task DownloadPodcastEpisodeAsync_ShouldReturnFilePath()
        {
            // Arrange
            var podcastId = "1000764234023";

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
            await applePodcastDownloader.DownloadPodcastEpisodeAsync(podcastId);

            // Assert
            var expectedFilePath = Path.Combine(Constants.PathToDownloadedPodcasts, $"{podcastId}.mp3");
            Assert.True(File.Exists(expectedFilePath));
        }
    }
}
