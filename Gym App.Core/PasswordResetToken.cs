
using Gym_App.Domain;

namespace Gym_App.Core
{
    public class PasswordResetToken : BaseEntity
    {
        public required string OTP { get; set; }
        public required DateTime Expiration { get; set; }
        public required string Email { get; set; }
        public required Guid userId { get; set; }
        public User User { get; set; } = null!;
        public bool isUsed { get; set; } = false;
    }
}
