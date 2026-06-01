using MediatR;

namespace ApplePodcastTranscription.Models.Notification
{
    public record TranscriptProgressNotification(string SessionGuid, int Progress) : INotification;
}
