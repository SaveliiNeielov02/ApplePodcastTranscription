using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Services.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplePodcastTranscriptionTest
{
    public class ApplePodcastDbTest
    {
        [Fact]
        public async Task AddPodcast_ShouldBeAddedInDatabase()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDbContext<ApplePodcastDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            services.AddScoped<IPodcastRecordRepository, PodcastRecordRepository>();

            using var serviceProvider = services.BuildServiceProvider();
            var testPodcastExternalId = "test_podcast_external_id";

            var repository = serviceProvider.GetRequiredService<IPodcastRecordRepository>();
            var dbContext = serviceProvider.GetRequiredService<ApplePodcastDbContext>();

            // Act
            await repository.AddPodcastRecordAsync(testPodcastExternalId);

            // Assert
            var podcastRecord = await dbContext.PodcastsRecords.FirstOrDefaultAsync(p => p.ExternalId == testPodcastExternalId);
            Assert.NotNull(podcastRecord);
        }
    }
}
