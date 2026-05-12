using ApplePodcastTranscription.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.ObjectModel;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class PodcastDownloadingTest
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
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<ApplePodcastTranscription.Services.DirectApplePodcastDownloader>>();

            // Mock the configuration
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var applePodcastDownloader = new ApplePodcastTranscription.Services.DirectApplePodcastDownloader(
                httpClientFactory.Object,
               configuration);

            // Act
            var result = await applePodcastDownloader.DownloadPodcastEpisodeAsync(podcastId, new(), saveLocally);
            var audioContent = result?.AudioContent;

            // Assert
            Assert.True(audioContent != null && audioContent.Length > 0);
        }
    }
}
