using ApplePodcastTranscription.Models;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Session;
using Microsoft.AspNetCore.Mvc;

namespace ApplePodcastTranscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PodcastUrlParser _urlParser;
        public SessionController(PodcastUrlParser urlParser, IServiceScopeFactory serviceProvider) 
        {
            _urlParser = urlParser;
            _serviceScopeFactory = serviceProvider;
        }
        [HttpPost("initialize")]
        public IActionResult Initialize([FromBody] InitializeSession request)
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
