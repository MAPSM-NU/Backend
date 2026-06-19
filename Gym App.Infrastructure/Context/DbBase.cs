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
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<UserStatsDaily> UserStatsDaily { get; set; }
        public DbSet<UserStatsWeekly> UserStatsWeekly { get; set; }
        public DbSet<UserStatsMonthly> UserStatsMonthly { get; set; }
        public DbSet<FitnessGoals> FitnessGoals { get; set; }
        public DbSet<Injury> Injuries { get; set; }
        public DbSet<ExerciseRestrictions> ExerciseRestrictions { get; set; }
        public DbSet<MedicalCondition> MedicalConditions { get; set; }

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

            modelBuilder.Entity<UserStats>()
                .HasOne(us => us.user)
                .WithOne(u => u.UserStats)
                .HasForeignKey<UserStats>(us => us.userId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserStatsDaily>()
                .HasOne(usd => usd.user)
                .WithMany(u => u.UserStatsDaily)
                .HasForeignKey(usd => usd.userId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserStatsDaily>().HasIndex(usd => new { usd.userId, usd.date }).IsUnique();

            modelBuilder.Entity<UserStatsWeekly>()
                .HasOne(usw => usw.user)
                .WithMany(u => u.UserStatsWeekly)
                .HasForeignKey(usw => usw.userId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserStatsWeekly>().HasIndex(usw => new { usw.userId, usw.weekNumber, usw.year }).IsUnique();

            modelBuilder.Entity<UserStatsMonthly>()
                .HasOne(usm => usm.user)
                .WithMany(u => u.UserStatsMonthly)
                .HasForeignKey(usm => usm.userId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserStatsMonthly>().HasIndex(usm => new {usm.userId, usm.monthName, usm.year}).IsUnique();

            modelBuilder.Entity<FitnessGoals>()
                .HasMany(fg => fg.Users)
                .WithMany(u => u.FitnessGoals);
            modelBuilder.Entity<FitnessGoals>().HasData(
                new FitnessGoals { Name = FitnessGoalType.Endurance.ToString(), Id = Guid.Parse("7b1f4c4d-5c0e-4f6d-8d8d-9f4c6b8a2e11") },
                new FitnessGoals { Name = FitnessGoalType.GeneralFitness.ToString(), Id = Guid.Parse("2e8a1f93-6c47-4d7f-9c21-4b5e7d3f8a22") },
                new FitnessGoals { Name = FitnessGoalType.StrengthTraining.ToString(), Id = Guid.Parse("91d3b6e7-8a54-4f12-b8c4-7e9f2d1a3b33") },
                new FitnessGoals { Name = FitnessGoalType.WeightLoss.ToString(), Id = Guid.Parse("4c7e2a11-9f85-4d3b-a1e7-5d8c6b2f4c44") },
                new FitnessGoals { Name = FitnessGoalType.MuscleGain.ToString(), Id = Guid.Parse("8f2d7c3a-1b64-4e9d-9a72-3c5e8f1b5d55") },
                new FitnessGoals { Name = FitnessGoalType.Flexibility.ToString(), Id = Guid.Parse("5a9c1e7f-3d82-4b6e-b4f8-2d7c9a6e1f66") }
            );

            modelBuilder.Entity<Injury>()
                .HasMany(i => i.Users)
                .WithMany(u => u.Injuries);
            modelBuilder.Entity<Injury>().HasData(
                new Injury { Name = InjuryType.Back.ToString(), Id = Guid.Parse("a1f3c7d9-5b42-4e8a-9d71-2c6f8b3e7a11") },
                new Injury { Name = InjuryType.Knee.ToString(), Id = Guid.Parse("b2e4d8f1-6c53-4f9b-a182-3d7a9c4f8b22") },
                new Injury { Name = InjuryType.Shoulder.ToString(), Id = Guid.Parse("c3f5e9a2-7d64-4a1c-b293-4e8b1d5a9c33") },
                new Injury { Name = InjuryType.Ankle.ToString(), Id = Guid.Parse("d4a6f1b3-8e75-4b2d-c3a4-5f9c2e6b1d44") },
                new Injury { Name = InjuryType.Wrist.ToString(), Id = Guid.Parse("e5b7a2c4-9f86-4c3e-d4b5-6a1d3f7c2e55") },
                new Injury { Name = InjuryType.Hip.ToString(), Id = Guid.Parse("f6c8b3d5-1a97-4d4f-e5c6-7b2e4a8d3f66") }
            );

            modelBuilder.Entity<ExerciseRestrictions>()
                .HasMany(er => er.Users)
                .WithMany(u => u.ExerciseRestrictions);
            modelBuilder.Entity<ExerciseRestrictions>().HasData(
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.NoHeavyLifting.ToString(), Id = Guid.Parse("1d7a4c9e-2b58-4f6a-8c71-9e3d5b7a1c11") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.NoRunning.ToString(), Id = Guid.Parse("2e8b5d1f-3c69-4a7b-9d82-1f4e6c8b2d22") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.LowerBodyOnly.ToString(), Id = Guid.Parse("3f9c6e2a-4d7a-4b8c-a193-2a5f7d9c3e33") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.LowImpactOnly.ToString(), Id = Guid.Parse("4a1d7f3b-5e8b-4c9d-b2a4-3b6a8e1d4f44") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.UpperBodyOnly.ToString(), Id = Guid.Parse("5b2e8a4c-6f9c-4d1e-c3b5-4c7b9f2e5a55") }
            );

            modelBuilder.Entity<MedicalCondition>()
                .HasMany(mc => mc.Users)
                .WithMany(u => u.MedicalConditions);
            modelBuilder.Entity<MedicalCondition>().HasData(
                new MedicalCondition { Name = MedicalConditionsType.HeartCondition.ToString(), Id = Guid.Parse("6e3f2a91-4d8b-47c6-a2e1-9f5d7b3c8a77") },
                new MedicalCondition { Name = MedicalConditionsType.HighBloodPressure.ToString(), Id = Guid.Parse("7f4a3b82-5e9c-48d7-b3f2-a6e8c4d9b188") },
                new MedicalCondition { Name = MedicalConditionsType.Arthritis.ToString(), Id = Guid.Parse("8a5b4c73-6f1d-49e8-c4a3-b7f9d5e1c299") },
                new MedicalCondition { Name = MedicalConditionsType.Asthma.ToString(), Id = Guid.Parse("9b6c5d64-7a2e-4af9-d5b4-c8a1e6f2d3aa") },
                new MedicalCondition { Name = MedicalConditionsType.Diabetes.ToString(), Id = Guid.Parse("ac7d8e91-3f42-4b6a-9c15-e7d2f8a4b3c1") }
            );
            
        }
    }
}
