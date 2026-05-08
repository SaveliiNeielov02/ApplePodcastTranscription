using ApplePodcastTranscription.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplePodcastTranscription.Services.Database
{
    public class ApplePodcastDbContext : DbContext
    {
        public DbSet<PodcastRecord> PodcastsRecords { get; set; }
        public ApplePodcastDbContext(DbContextOptions<ApplePodcastDbContext> options) : base(options)
        {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine("Resources", "Database", "database.sqlite");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }
}
