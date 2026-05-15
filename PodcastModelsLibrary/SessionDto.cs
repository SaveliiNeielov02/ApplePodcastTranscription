namespace PodcastModelsLibrary
{
    public class SessionDto
    {
        public Guid Guid { get; set; }
        public string? TranscriptionStatus { get; set; }
        public string? PodcastTitle { get; set; }
        public string? PodcastArtist { get; set; }
        public string? IconUrl { get; set; }
        public long SessionCreatedAtInUnixTimeSeconds { get; set; }
        public long DownloadedAtInUnixTimeSeconds { get; set; }
        public long TranscribedAtInUnixTimeSeconds { get; set; }
    }
}
