using Gym_App.Application.Authorization;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.Interfaces.Services;
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
        private static readonly Dictionary<string, List<Guid>> rooms = new();
        private static readonly Dictionary<string, Dictionary<string, string>> roomUsers = new(); // room -> {connectionId -> userName}
        
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
                roomUsers[room.Room] = new Dictionary<string, string>();
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
            
            // Track user in room
            roomUsers[room.Room][Context.ConnectionId] = currentUser.Name ?? "Unknown";
            
            // User is authorized, join the room
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Room);
            
            // Notify others that user joined
            await Clients.Group(room.Room).SendAsync("user_joined", new { userName = currentUser.Name, room = room.Room });
            
            return new OutputResponse<OutputMessage>(2,"Successfully joined room",null,result);
        }
        public async Task<OutputResponse<OutputMessage>> GetMessages(RoomRequest room, int page = 1, int pageSize = 5)
        {
            if (!Guid.TryParse(room.Room, out var chatId))
            {
                return new OutputResponse<OutputMessage>(0, "Invalid Id", null, null);
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
            return new OutputResponse<OutputMessage>(2, "Messages Loaded successfully", null, result);
        }
        public async Task<OutputResponse<Object>> SendMessage(InputMessage message)
        {
            var username = currentUser.Name;
            var userMessage = new UserMessage(
                new(Context.UserIdentifier!, username!),
                message.Message,
                message.Room,
                DateTimeOffset.Now
            );

            if (!rooms.ContainsKey(message.Room)) return new OutputResponse<Object>(0, "Chat Id not found", null, null);

            Guid chatId;
            Guid.TryParse(message.Room, out chatId);
            var dto = new MessageCreationDTO
            {
                SessionID = chatId,
                Content = message.Message,
                IsRead = false
            };
            var result = await messageRegistry.AddMessage((Guid)currentUser.UserID!,dto);
            if (result.status == 0) return new OutputResponse<Object>(0, result.msg, null, null);

            // Send message to ALL clients in the group (including sender)
            await Clients.Group(message.Room)
                .SendAsync("send_message", userMessage.Output);
            return new OutputResponse<Object>(2, result.msg, null, null);
        }
        public Task LeaveGroup(RoomRequest room)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Room);
        }
        
        public Task<OutputResponse<string>> GetLiveChats(int page=1,int pageSize=5)
        {
            var chats = rooms.Keys.ToList();
            return Task.FromResult(new OutputResponse<string>(2, "Success", null, chats));
        }
    }
}
