using MediatR;

namespace ApplePodcastTranscription.Models.Notification
{
    public record TranscriptProgressNotification(Guid SessionGuid, int Progress) : INotification;
}
