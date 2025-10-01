namespace Gym_App.Domain.Entities
{
    public class ResponseToken // used to transfer tokens inside the authentication controller and UserF class
    {
        public int Status { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
