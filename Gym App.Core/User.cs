using Gym_App.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class User : BaseEntity
    {
        [Column(TypeName = "nvarchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(100)")]
        public required string Email { get; set; }
        public bool isEmailConfirmed { get; set; } = false;
        [Column(TypeName = "varchar(100)")]
        public required string Password{ get; set; }
        [Column(TypeName = "nvarchar(1000)")]
        public string? Bio { get; set; }
        public DateTime? DOB { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? State { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? City { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Country { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string? PhoneNumber { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? ProfilePictureUrl { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string? subscriptionPlan { get; set; }
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? Certifications { get; set; }
        [Column(TypeName = "varchar(8)")]
        public string UserType { get; set; } = "User"; // Discriminator column
        public Guid RoleID { get; set; } // Foreign key property

        //Relationships
        public ICollection<PastInjuries>? PastInjuries  { get; set; } = new List<PastInjuries>();
        public ICollection<Challenges>? Challenges { get; set; } = new List<Challenges>();
        public ICollection<LiveFeedback>? LiveFeedbacks { get; set; } = new List<LiveFeedback>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Transaction>? Transactions { get; set; } = new List<Transaction>();
        public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();
        public ICollection<Message>? Messages { get; set; } = new List<Message>();
        public ICollection<Schedule>? Schedules { get; set; } = new List<Schedule>();
        public ICollection<Workout>? Workouts { get; set; } = new List<Workout>();
        public ICollection<RefreshTokens>? RefreshTokens { get; set; } = new List<RefreshTokens>();
        public ICollection<Session>? Sessions { get; set; } = new List<Session>();
        public ICollection<PasswordResetToken> passwordResetTokens { get; set; } = new List<PasswordResetToken>();
        public required Role Role { get; set; }
        public UserStats? UserStats { get; set; }
        public ICollection<UserStatsDaily>? UserStatsDaily { get; set; } = new List<UserStatsDaily>();
        public ICollection<UserStatsWeekly>? UserStatsWeekly { get; set; } = new List<UserStatsWeekly>();
        public ICollection<UserStatsMonthly>? UserStatsMonthly { get; set; } = new List<UserStatsMonthly>();
        public ICollection<FitnessGoals> FitnessGoals { get; set; } = new List<FitnessGoals>();
        public ICollection<Injury> Injuries { get; set; } = new List<Injury>();
        public ICollection<ExerciseRestrictions> ExerciseRestrictions { get; set; } = new List<ExerciseRestrictions>();
        public ICollection<MedicalCondition> MedicalConditions { get; set; } = new List<MedicalCondition>();

    }
}
