namespace Gym_App.Domain.DTOs
{
    public class UserUpdateDTO
    {
        public Guid UserID { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public DateTime DOB { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
    }
}
