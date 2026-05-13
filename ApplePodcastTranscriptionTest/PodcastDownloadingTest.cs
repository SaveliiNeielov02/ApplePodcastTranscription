using ApplePodcastTranscription.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.ObjectModel;
using System.Text;
using ApplePodcastTranscription.Services;

namespace ApplePodcastTranscriptionTest
{
    public class PodcastDownloadingTest
    {
        [Fact]
        public async Task DownloadPodcastEpisodeAsync_ShouldReturnAudioContent()
        {
            // Arrange
            var podcastId = "1000764234023";
            var buffer = new byte[512];

            // Mock the HttpClientFactory
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient());

            // Mock file IO
            var podcastFileIo = new LocalPodcastFileManager();

            // Mock the configuration
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();

            var applePodcastDownloader = new DirectApplePodcastDownloader(
                httpClientFactory.Object,
               configuration, podcastFileIo);

            // Act
            var result = await applePodcastDownloader.DownloadPodcastEpisodeAsync(podcastId, podcastId, CancellationToken.None);

            var storageKey = result.StorageKey;
            var audioContent = podcastFileIo.ReadAsFileStream(storageKey);
            var bytesRead = await audioContent.ReadAsync(buffer, 0, buffer.Length);

            // Assert
            Assert.True(bytesRead > 0);
        }
    }
}
