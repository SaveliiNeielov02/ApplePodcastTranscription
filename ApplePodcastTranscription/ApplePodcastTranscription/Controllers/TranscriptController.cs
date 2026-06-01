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

