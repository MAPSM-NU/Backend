using Gym_App.Core;
using Gym_App.Domain;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infastructure.Context
{
    public class DbBase : DbContext
    {
        public DbBase(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
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
        public DbSet<Role> Roles { get; set; }
        public DbSet<PersonalRecord> PersonalRecords { get; set; }
        public DbSet<ExerciseInstance> ExerciseInstances { get; set; }
        public DbSet<WorkoutSet> WorkoutSets { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PastInjuries>();
            //modelBuilder.Entity<User>()
            //    .Property(u => u.Id)
            //    .HasColumnName("Id");
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<Workout>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<Exercise>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Muscles>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Schedule>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);
            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Feedback>()
                .HasKey(f => f.Id);
            modelBuilder.Entity<Challenges>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<RefreshTokens>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<PasswordResetToken>()
                .HasKey(pt => pt.Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u=>u.RoleID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workout>()
                .HasOne(w => w.Feedback)
                .WithOne(f => f.Workout)
                .HasForeignKey<Feedback>(f => f.WorkoutID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workout>()
                .HasMany(w => w.ExerciseInstances)
                .WithOne(ei => ei.Workout)
                .HasForeignKey(ei => ei.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveFeedback>();
            
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u=>u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RefreshTokens>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RefreshTokens>();

            modelBuilder.Entity<WorkoutSet>();

            modelBuilder.Entity<ExerciseInstance>()
                .HasMany(ei=>ei.Sets)
                .WithOne(ws=>ws.ExerciseInstance)
                .HasForeignKey(ws=>ws.ExerciseInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonalRecord>();

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(pt => pt.User)
                .WithMany(u => u.passwordResetTokens)
                .HasForeignKey(pt => pt.userId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
