namespace Gym_App.Domain
{
    public class SessionUsers
    {
        public Guid SessionID { get; set; }
        public Session? Session { get; set; }
        public Guid? UserID { get; set; }
        public User? User { get; set; }
    }
}
