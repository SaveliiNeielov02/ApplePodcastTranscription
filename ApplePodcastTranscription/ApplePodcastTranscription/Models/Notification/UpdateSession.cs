using MediatR;

namespace ApplePodcastTranscription.Models.Notification
{
    public record UpdateSession(Guid SessionGuid) : INotification;
}
