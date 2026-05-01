

namespace Gym_App.Application.Hubs
{
    public record NotifChatMessage(
        string userId,
        string Message,
        DateTimeOffset SentAt);
    public record NotifSentMessage(
        string senderId,
        string Message,
        string recieverId,
        DateTimeOffset SentAt
        );
    public record User(string UserId, string UserName);
    public record RoomRequest(string Room);

    public record InputMessage(
        string Message,
        string Room
    );

    public record OutputMessage(
        string Message,
        string UserName,
        string Room,
        DateTimeOffset SentAt
    );
    public record OutputResponse<T>(
        int status,
        string msg,
        T? value,
        List<T>? Data
    );

    public record UserMessage(
        User User,
        string Message,
        string Room,
        DateTimeOffset SentAt
    )
    {
        public OutputMessage Output => new(Message, User.UserName, Room, SentAt);
    }
}
