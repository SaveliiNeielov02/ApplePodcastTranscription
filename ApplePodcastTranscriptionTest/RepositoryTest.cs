using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
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
    public class RepositoryTest
    {
        [Fact]
        public async Task AddSession_ShouldBeAddedInDatabase()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDbContext<ApplePodcastDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            services.AddScoped<ISessionRepository, PodcastSessionRepository>();

            using var serviceProvider = services.BuildServiceProvider();
            var testPodcastObject = new PodcastRecord() { ExternalId = "test_podcast_external_id" };
            var testSessionObject = new TranscriptSession()
            {
                Guid = Guid.NewGuid(),
                PodcastRecordId = testPodcastObject.ExternalId,
                PodcastRecord = testPodcastObject,
                TranscriptionStatus = SessionStatus.Pending,
                TranscriptionError = SessionError.None
            };
            var repository = serviceProvider.GetRequiredService<ISessionRepository>();

            // Act
            await repository.AddSessionAsync(testSessionObject);

            // Assert
            var podcastRecord = await repository.GetSessionAsync(testSessionObject.Guid);
            Assert.NotNull(podcastRecord);
        }
    }
}
