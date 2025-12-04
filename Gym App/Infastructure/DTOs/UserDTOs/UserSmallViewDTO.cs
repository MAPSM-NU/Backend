namespace Gym_App.Infastructure.DTOs.UserDTOs
{
    public class UserSmallViewDTO
    {
        public Guid UserID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
