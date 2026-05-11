using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace ApplePodcastTranscription.Services.Database
{
    public class PodcastRecordRepository : IPodcastRecordRepository
    {
        private readonly ApplePodcastDbContext _context;
        public PodcastRecordRepository(ApplePodcastDbContext context)
        {
            _context = context;
        }

        public Task AddPodcastTranscriptionAsync(PodcastRecord entity, string podcastTranscription)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscribedAtInUnixTimeSecondsAsync(PodcastRecord entity, long transcribedAtInUnixTimeSeconds)
        {
            throw new NotImplementedException();
        }
    }
}
