namespace ApplePodcastTranscription.Models
{
    public class PodcastDtoBase
    {
        public required string ExternalId { get; set; }
        public string? Title { get; set; }
        public string? ArtistName { get; set; }
        public string? IconUrl { get; set; }
        public long DownloadedAtInUnixTimeSeconds { get; set; }
    }
}
