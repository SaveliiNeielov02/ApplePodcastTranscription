using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class SessionWorkerTest
    {
        [Fact]
        public void GetPodcastExternalId_ShouldReturnCorrectId()
        {
            // Arrange
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var configuration = builder.Build();
            var transcriptionQueue = new TranscriptionQueue(Mock.Of<ILogger<TranscriptionQueue>>());
            var applePodcastDownloader = new ApplePodcastDownloader(
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<ILogger<ApplePodcastDownloader>>(),
                configuration);

            var sessionWorker = new SessionWorker(
                Mock.Of<ISessionRepository>(),
                Mock.Of<IPodcastRecordRepository>(),
                Mock.Of<IServiceProvider>(),
                transcriptionQueue,
                applePodcastDownloader,
                Mock.Of<ILogger>());

            var podcastUrl = "https://podcasts.apple.com/de/podcast/insight-wie-viele-feiertage-kann-sich-deutschland-noch/id1352434240?i=1000762904550";
            var expectedExternalId = "1000762904550";
            
            // Act
            var result = sessionWorker.GetPodcastExternalId(podcastUrl);

            // Assert
            Assert.Equal(expectedExternalId, result);
        }
    }
}