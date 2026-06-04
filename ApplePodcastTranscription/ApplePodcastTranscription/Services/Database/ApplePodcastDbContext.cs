using ApplePodcastTranscription.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace ApplePodcastTranscription.Services.Database
{
    public class ApplePodcastDbContext : DbContext
    {
        public DbSet<TranscriptSession> TranscriptSessions { get; set; }
        public DbSet<PodcastRecord> PodcastsRecords { get; set; }

        public ApplePodcastDbContext(DbContextOptions<ApplePodcastDbContext> options) : base(options)
        { }
        public ApplePodcastDbContext()
        { }
    }
}
