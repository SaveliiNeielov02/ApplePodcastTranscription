using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Session;
using Microsoft.AspNetCore.Mvc;

namespace ApplePodcastTranscription.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranscriptController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PodcastUrlParser _urlParser;
        public TranscriptController(PodcastUrlParser urlParser, IServiceScopeFactory serviceProvider) 
        {
            _urlParser = urlParser;
            _serviceScopeFactory = serviceProvider;
        }
        [HttpPost("initializeSession")]
        public IActionResult InitializeSession([FromBody] InitializeSessionRequest request)
        {
            try
            {
                string podcastExternalId = _urlParser.GetPodcastExternalId(request.PodcastUrl);
                Task.Run(async () => 
                {
                    // Creating new scope to avoid potential issues with scoped services in the background task
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sessionWorker = scope.ServiceProvider.GetRequiredService<SessionWorker>();
                    await sessionWorker.StartTransriptPipeline(podcastExternalId);
                });
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
