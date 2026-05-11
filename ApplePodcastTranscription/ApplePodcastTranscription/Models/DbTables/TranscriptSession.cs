using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplePodcastTranscription.Models.DbTables
{
    public class TranscriptSession
    {
        [Key]
        public required Guid Guid { get; set; }
        public required string PodcastRecordId { get; set; }
        public required PodcastRecord PodcastRecord { get; set; }
        public required SessionionStatus TranscriptionStatus { get; set; }
        public required SessionError TranscriptionError { get; set; }
    }
    public enum SessionionStatus
    {
        Pending,
        InQueue,
        InProgress,
        Completed,
        Failed
    }
    public enum SessionError
    {
        None,
        DownloadFailed,
        TranscriptionFailed,
        UnknownError
    }
}
