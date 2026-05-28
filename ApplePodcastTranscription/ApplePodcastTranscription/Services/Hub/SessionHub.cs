using Microsoft.AspNetCore.SignalR;

namespace ApplePodcastTranscription.Services.Hub
{
    public class SessionHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SubscribeToSession(Guid sessionGuid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionGuid.ToString());
        }
        public async Task UnsubscribeFromSession(Guid sessionGuid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionGuid.ToString());
        }
    }
}
