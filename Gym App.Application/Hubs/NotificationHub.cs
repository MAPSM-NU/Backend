using Gym_App.Application.Authorization;
using Gym_App.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Hubs
{
    [Authorize(Policy = "NormalUsage")]
    public class NotificationHub : Hub
    {
        private readonly ICurrentUser currentUser;
        private readonly ILogger<NotificationHub> logger;
        public static Dictionary<string, List<string>> ConnectedUsers = new();
        public NotificationHub(ICurrentUser current,ILogger<NotificationHub> log)
        { 
            currentUser = current;
            logger = log;
        }

        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = currentUser.UserID.ToString(); // any desired user id

            lock (ConnectedUsers)
            {
                if (!ConnectedUsers.ContainsKey(userId))
                    ConnectedUsers[userId] = new();
                ConnectedUsers[userId].Add(connectionId);
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var userId = currentUser.UserID.ToString(); // any desired user id

            lock (ConnectedUsers)
            {
                if (ConnectedUsers.ContainsKey(userId))
                {
                    ConnectedUsers[userId].Remove(connectionId);
                    if (ConnectedUsers[userId].Count == 0)
                        ConnectedUsers.Remove(userId);
                }
            }
            logger.Log(logLevel:LogLevel.Error,exception!.Message);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task<OutputResponse<Object>> SendNotif(NotifMessage notif, CancellationToken stoppingToken)
        {
            if(!ConnectedUsers.ContainsKey(notif.userId)) return new OutputResponse<Object>(0,"user not online",null,null);

            await Clients.User(notif.userId).SendAsync("Notify", notif, stoppingToken);

            return new OutputResponse<Object>(2, "successful", null, null);

        }
    }
}
