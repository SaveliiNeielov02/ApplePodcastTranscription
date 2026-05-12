using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;

namespace ApplePodcastTranscription.Interfaces
{
    public interface ISessionRepository
    {
        public Task AddSessionAsync(TranscriptSession podcastDto);
        public Task<TranscriptSession?> GetSessionAsync(Guid sessionGuid);
        public Task<IEnumerable<TranscriptSession>> GetAllSessionsAsync();
        public Task UpdateSessionStatusAsync(TranscriptSession entity, SessionStatus status);
        public Task UpdateSessionErrorAsync(TranscriptSession entity, SessionError error);
    }
}
