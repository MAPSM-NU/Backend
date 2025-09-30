namespace Gym_App.Domain.Entities
{
    public class Response
    {
        public int Status { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
