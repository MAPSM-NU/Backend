namespace Gym_App.Infastructure.DTOs.UserDTOs
{
    public class UserCreationDTO
    {
        public string? Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? UserType { get; set; }
    }
}
