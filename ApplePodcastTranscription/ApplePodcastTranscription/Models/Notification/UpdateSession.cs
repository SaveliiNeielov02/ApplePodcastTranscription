using MediatR;

namespace ApplePodcastTranscription.Models.Notification
{
    public record UpdateSession(string SessionGuid) : INotification;
}
