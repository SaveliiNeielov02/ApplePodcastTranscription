using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplePodcastTranscription.Models.DbTables
{
    public class PodcastRecord
    {
        [Key]
        public required string ExternalId { get; set; }
        public List<TranscriptSession> Sessions { get; set; } = new();
        public string? Title { get; set; }
        public string? ArtistName { get; set; }
        public string? IconUrl { get; set; }
        public string? TranscriptionText { get; set; }
        public long DownloadedAtInUnixTimeSeconds { get; set; }
        public long TranscribedAtInUnixTimeSeconds { get; set; }
    }
}
