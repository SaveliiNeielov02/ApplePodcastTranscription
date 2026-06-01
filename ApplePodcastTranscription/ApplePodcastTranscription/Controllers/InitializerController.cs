using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Session;
using Microsoft.AspNetCore.Mvc;
using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InitializerController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PodcastUrlParser _urlParser;
        public InitializerController(PodcastUrlParser urlParser, IServiceScopeFactory serviceProvider) 
        {
            _urlParser = urlParser;
            _serviceScopeFactory = serviceProvider;
        }

        [HttpPost("init")]
        public IActionResult Initialize([FromBody] InitializeSession request)
        {
            try
            {
                string podcastExternalId = _urlParser.GetPodcastExternalId(request.PodcastUrl);
                var newSessionGuid = Guid.NewGuid();

                Task.Run(async () => 
                {
                    // Creating new scope to avoid potential issues with scoped services in the background task
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sessionWorker = scope.ServiceProvider.GetRequiredService<SessionWorker>();
                    await sessionWorker.StartTranscriptPipeline(newSessionGuid, podcastExternalId);
                });

                return Ok(newSessionGuid);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
