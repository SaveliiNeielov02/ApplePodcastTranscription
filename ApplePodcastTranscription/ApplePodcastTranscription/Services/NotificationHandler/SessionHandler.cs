using ApplePodcastTranscription.Models.Notification;
using ApplePodcastTranscription.Services.Hub;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApplePodcastTranscription.Services.NotificationHandler
{
    public class SessionHandler : INotificationHandler<UpdateSession>
    {
        private readonly IHubContext<SessionHub> _hubContext;

        public SessionHandler(IHubContext<SessionHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task Handle(UpdateSession request, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.Group(request.SessionGuid.ToString())
                .SendAsync("SessionChanged", request.SessionGuid, CancellationToken.None);
        }
    }
}
