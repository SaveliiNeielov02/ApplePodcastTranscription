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

        public async Task<IEnumerable<TranscriptSession>> GetAllSessionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<TranscriptSession?> GetSessionAsync(Guid sessionGuid)
        {
            return await _context.TranscriptSessions.FirstOrDefaultAsync(s => s.Guid == sessionGuid);
        }

        public async Task UpdateSessionErrorAsync(TranscriptSession entity, SessionError error)
        {
            entity.TranscriptionError = error;

            await _context.SaveChangesAsync();  
        }

        public async Task UpdateSessionStatusAsync(TranscriptSession entity, SessionStatus status)
        {
            entity.TranscriptionStatus = status;

            await _context.SaveChangesAsync();
        }
    }
}
