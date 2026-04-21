using Gym_App.Application.Authorization;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Gym_App.Application.Hubs
{
    [Authorize(Policy = "NormalUsage")]
    public class ChatHub : Hub
    {
        private readonly IMessageService messageRegistry;
        private readonly ISessionService chatRegistry;
        private readonly ICurrentUser currentUser;
        private readonly Dictionary<string, List<Guid>> rooms = new();
        public ChatHub(IMessageService message, ISessionService chat,ICurrentUser user)
        {
            messageRegistry = message;
            chatRegistry = chat;
            currentUser = user;
        }
        public async Task<OutputResponse<OutputMessage>> JoinRoom(RoomRequest room, int page=1, int pageSize = 5)
        {
            // Parse room ID
            if (!Guid.TryParse(room.Room, out var chatId)) 
            {
                return new OutputResponse<OutputMessage>(0,"Invalid Id", null,null);
            }

            // Get or load session users into rooms dictionary
            if (!rooms.ContainsKey(room.Room))
            {
                var userIds = await chatRegistry.GetSessionUsersIDs(chatId);
                if (userIds == null || !userIds.Any())
                    return new OutputResponse<OutputMessage>(0,"No session found with given Id",null,null);
                
                rooms[room.Room] = userIds;
            }

            if (currentUser.User == null) return new OutputResponse<OutputMessage> (0, "Invalid Data", null, null);
            
            var currentUserId = currentUser.UserID;

            // Check if user is in the session
            if (!rooms[room.Room].Contains((Guid)currentUserId!))
            {
                return new OutputResponse<OutputMessage>(0, "User is not a member of this session", null, null);
            }

            var messages = await chatRegistry.GetSessionMessages(chatId, "", "", page, "", "", "", pageSize);
            List<OutputMessage> result = new List<OutputMessage>();
            foreach (var message in messages.Data.Items)
            {
                result.Add(new OutputMessage(
                     message.Content,
                     currentUser.Email ?? string.Empty,
                     room.Room,
                     message.Timestamp
                ));
            }
            // User is authorized, join the room
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Room);
            return new OutputResponse<OutputMessage>(2,"Successfully joined room",null,result);
        }
        public async Task GetMessages(RoomRequest room, int page = 1, int PageSize = 5)
        {
            throw new NotImplementedException();
        }
        public async Task SendMessage(InputMessage message)
        {
            var username = currentUser.Name;
            var userMessage = new UserMessage(
                new(Context.UserIdentifier, username),
                message.Message,
                message.Room,
                DateTimeOffset.Now
            );

            if (!rooms.ContainsKey(message.Room)) return;

            Guid chatId;
            Guid.TryParse(message.Room, out chatId);
            var dto = new MessageCreationDTO
            {
                Content = message.Message,
                IsRead = false
            };
            await messageRegistry.AddMessage(chatId,dto);

            await Clients.GroupExcept(message.Room, new[] { Context.ConnectionId })
                .SendAsync("send_message", userMessage.Output);
            return;
        }
        public Task LeaveGroup(RoomRequest room)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Room);
        }
    }
}
