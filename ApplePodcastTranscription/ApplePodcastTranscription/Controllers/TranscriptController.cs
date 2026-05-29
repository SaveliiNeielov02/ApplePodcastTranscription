using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Models.DbTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranscriptController : ControllerBase
    {
        private readonly ISessionRepository _sessionRepository;
        public TranscriptController(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }
        [HttpGet("sessions")]
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
        public async Task<IActionResult> GetTranscript(Guid sessionGuid)
        {
            var session = await _sessionRepository.GetSessionAsync(sessionGuid);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            return Ok(new TranscriptResponse
            {
                SessionGuid = session.Guid,
                Text = session.PodcastRecord.TranscriptionText,
                TranscribedAtInUnixTimeSeconds = session.PodcastRecord.TranscribedAtInUnixTimeSeconds
            });
        }
    }
}

