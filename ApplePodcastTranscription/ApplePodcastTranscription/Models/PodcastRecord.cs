using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplePodcastTranscription.Models
{
    public class PodcastRecord
    {
        [Key]
        public required Guid Guid { get; set; }
        public required string ExternalId { get; set; }
        public required PodcastTranscriptionStatus TranscriptionStatus { get; set; }
        public required PodcastTranscriptionError TranscriptionError { get; set; }
        public string? TranscriptionText { get; set; }
        public long DownloadedAtInUnixTimeSeconds { get; set; }
        public long TranscribedAtInUnixTimeSeconds { get; set; }
    }
    public enum PodcastTranscriptionStatus
    {
        Obtained,
        InQueue,
        InProgress,
        Completed,
        Failed
    }
    public enum PodcastTranscriptionError
    {
        None,
        DownloadFailed,
        TranscriptionFailed,
        UnknownError
    }
}
