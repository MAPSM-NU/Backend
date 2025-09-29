using Gym_App.Domain.Entities;
using Gym_App.Domain.Entities.Sessions;
using Gym_App.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Gym_App
{
    public class DbBase : DbContext
    {
        public DbBase(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Trainee> Trainees { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<CoachTraineeSession> CoachTraineeSessions { get; set; }
        public DbSet<DoctorTraineeSession> DoctorTraineeSessions { get; set; }
        public DbSet<PastInjuries> PastInjuries { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Muscles> Muscles { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<LiveFeedback> LiveFeedbacks { get; set; }
        public DbSet<Challenges> Challenges { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RefreshTokens> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Trainee>("Trainee")
                .HasValue<Coach>("Coach")
                .HasValue<Doctor>("Doctor");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserID)
                .IsUnique();
            modelBuilder.Entity<Trainee>();
            modelBuilder.Entity<Coach>();
            modelBuilder.Entity<Doctor>();

            modelBuilder.Entity<PastInjuries>();

            modelBuilder.Entity<Session>()
                .HasDiscriminator<string>("SessionType")
                .HasValue<CoachTraineeSession>("CoachTraineeSession")
                .HasValue<DoctorTraineeSession>("DoctorTraineeSession");

            modelBuilder.Entity<CoachTraineeSession>();
            modelBuilder.Entity<DoctorTraineeSession>();
            modelBuilder.Entity<Workout>();
            modelBuilder.Entity<Exercise>();
            modelBuilder.Entity<Muscles>();
            modelBuilder.Entity<Schedule>();
            modelBuilder.Entity<Notification>();
            modelBuilder.Entity<Message>();
            modelBuilder.Entity<Feedback>();
            modelBuilder.Entity<LiveFeedback>();
            modelBuilder.Entity<Challenges>();
            modelBuilder.Entity<Transaction>();
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<RefreshTokens>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserID);
            modelBuilder.Entity<RefreshTokens>();
        }
    }
}
