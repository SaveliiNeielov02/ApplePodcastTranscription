using ApplePodcastTranscription.Models.Notification;
using ApplePodcastTranscription.Services.Hub;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApplePodcastTranscription.Services.NotificationHandler
{
    public class TranscriptProgressHandler : INotificationHandler<TranscriptProgressNotification>
    {
        private readonly IHubContext<SessionHub> _hubContext;

        public TranscriptProgressHandler(IHubContext<SessionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(TranscriptProgressNotification request, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.Group(request.SessionGuid.ToString())
                .SendAsync("ProgressChanged", request.SessionGuid, request.Progress, CancellationToken.None);
        }
    }
}
