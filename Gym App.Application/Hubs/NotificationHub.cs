using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Gym_App.Application.Hubs
{
    [Authorize(Policy = "NormalUsage")]
    public class NotificationHub : Hub
    {
        public NotificationHub() { }
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync(
                "Successfully connected");
        }
        public Task SendNotif(NotifMessage notif)
        {
            throw new NotImplementedException();
        }
    }
}
