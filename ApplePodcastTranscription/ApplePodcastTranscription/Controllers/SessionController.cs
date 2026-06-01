using ApplePodcastTranscription.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionRepository _sessionRepository;
        public SessionController(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSessions()
        {
            var sessions = (await _sessionRepository.GetAllSessionsAsync()).ToList();

            var response = sessions.Select(s => new SessionDto
            {
                Guid = s.Guid,
                TranscriptionStatus = s.TranscriptionStatus,
                PodcastTitle = s.PodcastRecord.Title,
                PodcastArtist = s.PodcastRecord.ArtistName,
                IconUrl = s.PodcastRecord.IconUrl,
                SessionCreatedAtInUnixTimeSeconds = s.CreatedAtInUnixTimeSeconds,
                DownloadedAtInUnixTimeSeconds = s.PodcastRecord.DownloadedAtInUnixTimeSeconds,
                TranscribedAtInUnixTimeSeconds = s.PodcastRecord.TranscribedAtInUnixTimeSeconds
            });

            return Ok(response);
        }
        [HttpGet("{sessionGuid}")]
        public async Task<IActionResult> GetSession(Guid sessionGuid)
        {
            var session = await _sessionRepository.GetSessionAsync(sessionGuid);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            var response = new SessionDto
            {
                Guid = session.Guid,
                TranscriptionStatus = session.TranscriptionStatus,
                PodcastTitle = session.PodcastRecord.Title,
                PodcastArtist = session.PodcastRecord.ArtistName,
                IconUrl = session.PodcastRecord.IconUrl,
                SessionCreatedAtInUnixTimeSeconds = session.CreatedAtInUnixTimeSeconds,
                DownloadedAtInUnixTimeSeconds = session.PodcastRecord.DownloadedAtInUnixTimeSeconds,
                TranscribedAtInUnixTimeSeconds = session.PodcastRecord.TranscribedAtInUnixTimeSeconds
            };

            return Ok(response);
        }
    }
}
