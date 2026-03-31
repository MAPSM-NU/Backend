using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface ISessionRepositry : IBaseRepositry<Session>
    {
        public Task<IEnumerable<Session>> GetUserSessions(Guid userId);
        public Task<IEnumerable<Session>> GetSessionsByUserId(Guid userId);
        public Task<Session> GetSessionWithUsers(Guid sessionId);
        public Task<IEnumerable<User>> GetSessionUsers(Guid sessionId);
        public Task<int> GetUserSessionCount(Guid userId);
        public Task<bool> AddUserToSession(Guid userId, Guid sessionId);
        public Task<bool> RemoveUserFromSession(Guid userId, Guid sessionId);

    }
}
