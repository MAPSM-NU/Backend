
namespace Gym_App.Infastructure.DTOs.UserDTOs
{
    public class UserViewDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DOB { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        public List<string> FitnessGoals {  get; set; } = new List<string>();
        public List<string> Injuries { get; set; } = new List<string>();
        public List<string> medicalConditions { get; set; } = new List<string>();
        public List<string> ExerciseRestrictions { get; set; } = new List<string>();
    }
}
