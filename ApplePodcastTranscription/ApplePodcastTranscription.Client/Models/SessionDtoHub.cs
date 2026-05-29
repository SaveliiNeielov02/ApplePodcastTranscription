using PodcastModelsLibrary;

namespace ApplePodcastTranscription.Client.Models
{
    public class SessionDtoHub : SessionDto
    {
        public int TranslationPercentage { get; set; } = 0;
    }
}
