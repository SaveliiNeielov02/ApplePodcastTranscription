using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace ApplePodcastTranscription.Services.Database
{
    public class PodcastSessionRepository : ISessionRepository
    {
        private readonly ApplePodcastDbContext _context;
        public PodcastSessionRepository(ApplePodcastDbContext context) 
        {
            _context = context;
        }

        public async Task AddSessionAsync(TranscriptSession session)
        {
            _context.TranscriptSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        public Task<IEnumerable<TranscriptSession>> GetAllSessionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<TranscriptSession?> GetSessionAsync(Guid sessionGuid)
        {
            return await _context.TranscriptSessions.FirstOrDefaultAsync(s => s.Guid == sessionGuid);
        }

        public Task UpdateDownloadedAtAsync(TranscriptSession entity, long downloadedAtInUnixTimeSeconds)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSessionErrorAsync(TranscriptSession entity, SessionError error)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSessionStatusAsync(TranscriptSession entity, SessionionStatus status)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTranscribedAtAsync(TranscriptSession entity, long transcribedAtInUnixTimeSeconds)
        {
            throw new NotImplementedException();
        }
    }
}
