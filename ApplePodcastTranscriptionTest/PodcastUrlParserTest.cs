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
    public class PodcastUrlParserTest
    {
        [Fact]
        public void GetPodcastExternalId_ShouldReturnCorrectId()
        {
            // Arrange
            var podcastUrlParser = new PodcastUrlParser();

            var podcastUrl = "https://podcasts.apple.com/de/podcast/insight-wie-viele-feiertage-kann-sich-deutschland-noch/id1352434240?i=1000762904550";
            var expectedExternalId = "1000762904550";
            
            // Act
            var result = podcastUrlParser.GetPodcastExternalId(podcastUrl);

            // Assert
            Assert.Equal(expectedExternalId, result);
        }
    }
}