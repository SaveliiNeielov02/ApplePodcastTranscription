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

        public async Task AddPodcastTranscriptionAsync(PodcastRecord entity, string podcastTranscription, long transcribedAtInUnixTimeSeconds)
        {
            entity.TranscriptionText = podcastTranscription;
            entity.TranscribedAtInUnixTimeSeconds = transcribedAtInUnixTimeSeconds;
            
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePodcastRecordAsync(PodcastRecord entity, PodcastDtoBase podcastDtoBase)
        {
            entity.ArtistName = podcastDtoBase.ArtistName;
            entity.IconUrl = podcastDtoBase.IconUrl;
            entity.Title = podcastDtoBase.Title;

            await _context.SaveChangesAsync();
        }
    }
}
