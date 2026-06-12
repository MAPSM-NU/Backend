using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Tests
{
    public abstract class TestBase
    {
        protected readonly DbBase _db;
        protected readonly IUnitOfWork _unitOfWork;

        protected TestBase(string databaseName)
        {
            var options = new DbContextOptionsBuilder<DbBase>()
                .UseInMemoryDatabase(databaseName: $"{databaseName}-{Guid.NewGuid()}")
                .Options;
            _db = new DbBase(options);
            _unitOfWork = new UnitOfWork(_db);
        }

        // Standard cleanup methods
        protected async Task CleanupAsync()
        {
            if (_db.Users.Any())
                _db.Users.RemoveRange(_db.Users);
            if (_db.Roles.Any())
                _db.Roles.RemoveRange(_db.Roles);
            await _unitOfWork.SaveChangesAsync();
        }

        // Factory methods for common test entities
        protected RefreshTokens CreateTestRefreshToken()
        {
            var refreshToken = new RefreshTokens
            {
                RefreshToken = "refresh-token",
                Expires = DateTime.Now.AddDays(7)
            };
            _unitOfWork.Tokens!.Create(refreshToken);
            return refreshToken;
        }
        protected Role CreateTestRole(string roleName = "User")
        {
            var role = new Role
            {
                RoleName = roleName,
                Id = Guid.NewGuid()
            };
            _unitOfWork.Roles.Create(role);
            return role;
        }
        protected RefreshTokens CreateTestRefreshToken(User user)
        {
            var refreshToken = new RefreshTokens
            {
                RefreshToken = "Test RefreshToken",
                Expires = DateTime.Now.AddDays(7),
                User = user,
                UserID = user.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _unitOfWork.Tokens!.Create(refreshToken);
            return refreshToken;
        }
        protected User CreateTestUser(Role role, string email = "test@gmail.com", string password = "Test_2004",
            string userType = "T", string name = "Test User", string phoneNumber = "testNum", string state = "TestState",
            string city = "TestCity", string country = "TestCountry", string bio = "TestBio", DateTime? dob = null, int heightCm = 0, int weightKg = 0,
            bool isEmailConfirmed = false)
        {
            var user = new User
            {
                Name = name,
                Email = email,
                UserType = userType,
                Password = new PasswordHasher<User>().HashPassword(null, password),
                Role = role,
                Id = Guid.NewGuid(),
                PhoneNumber = phoneNumber,
                State = state,
                City = city,
                Country = country,
                Bio = bio,
                DOB = dob,
                HeightCm = heightCm,
                WeightKg = weightKg,
                isEmailConfirmed = isEmailConfirmed
            };
            var userStats = new Gym_App.Core.UserStats
            {
                user = user,
                userId = user.Id,
                totalWorkoutsCompleted = 0,
                longestStreak = 0,
                workoutStreak = 0,
                totalExercisesCompleted = 0,
                totalHours = 0,
                totalWorkoutsMissed = 0,
                workoutCompletionRate = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            user.UserStats = userStats;
            _unitOfWork.Users.Create(user);
            return user;
        }
        protected Workout CreateTestWorkout(User user, string workoutName = "Test Workout", string description = "Test Description", Guid Id = default)
        {
            
            var workout = new Workout
            {
                Name = workoutName,
                Description = description,
                User = user,
                Id = Id == default ? Guid.NewGuid() : Id,
                Date = DateTime.Now,
                Day = DateTime.Now.DayOfWeek.ToString(),
                Type = "Strength",
                Difficulty = "Medium",
            };
            var exerciseList = new List<Exercise>();
            for(int i = 0; i < 3; i++)
            {
                exerciseList.Add(CreateTestExercise($"Test Exercise {i + 1}"));
            }
            var exercises = new List<ExerciseInstance>();
            for (int i = 0; i < 3; i++)
            {
                exercises.Add(CreateTestExerciseInstance(workout, exerciseList[i], $"Test Notes {i + 1}"));
                var set = CreateTestWorkoutSet(exercises[i], $"Test Notes {i + 1}");
            }
            workout.ExerciseInstances = exercises;
            _unitOfWork.Workouts.Create(workout);
            return workout;
        }
        protected ExerciseInstance CreateTestExerciseInstance(Workout workout, Exercise exercise,string notes = "Test Notes", int sets = 3, int reps = 10, int weightKg = 50)
        {
            var exerciseInstance = new ExerciseInstance
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                PlannedReps = reps,
                Notes = notes,
                PlannedWeight = weightKg,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _unitOfWork.ExerciseInstance.Create(exerciseInstance);
            return exerciseInstance;
        }
        protected WorkoutSet CreateTestWorkoutSet(ExerciseInstance exerciseInstance,string notes = "Test Notes", int setNumber = 1, int actualReps = 10, int actualWeight = 50)
        {
            var workoutSet = new WorkoutSet
            {
                ExerciseInstance = exerciseInstance,
                ExerciseInstanceId = exerciseInstance.Id,
                SetNumber = setNumber,
                ActualReps = actualReps,
                ActualWeight = actualWeight,
                Notes = notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _unitOfWork.WorkoutSet.Create(workoutSet);
            return workoutSet;
        }
        protected Exercise CreateTestExercise(string exerciseName = "Test Exercise",
            string description = "Test Description", DateTime createdAt = default, string category = "Strength", string difficulty = "Medium",
            string grip = "Overhand", Guid Id = default)
        {
            var exercise = new Exercise
            {
                Name = exerciseName,
                Description = description,
                VideoUrl = $"https://www.youtube.com/watch?v=example",
                UpdatedAt = DateTime.Now,
                Category = category,
                Id = Id == default ? Guid.NewGuid() : Id,
                Difficulty = difficulty,
                Grip = grip,
                CreatedAt = createdAt == default ? DateTime.Now : createdAt
            };
            _unitOfWork.Exercises.Create(exercise);
            return exercise;
        }
        protected Message CreateTestMessage(User sender, Session session, string content = "Test Message", DateTime sentAt = default)
        {
            var message = new Message
            {
                Sender = sender,
                Session = session,
                CreatedAt = sentAt == default ? DateTime.Now : sentAt,
                IsRead = false,
                Content = content,
                UpdatedAt = DateTime.Now,
            };
            _unitOfWork.Messages.Create(message);
            return message;
        }
        protected Session CreateTestSession(List<User> users, DateTime date = default)
        {
            var session = new Session
            {
                Users = users,
                SessionType = "Live Feedback",
                CreatedAt = DateTime.Now,
                UpdatedAt = date == default ? DateTime.Now : date
            };
            _unitOfWork.Sessions.Create(session);
            return session;
        }
        protected Exercise CreateTestExerciseWithId(Guid exerciseId, string exerciseName = "Test Exercise",
            string description = "Test Description", DateTime createdAt = default, string category = "Strength", string difficulty = "Medium",
            string grip = "Overhand")
        {
            var exercise = new Exercise
            {
                Name = exerciseName,
                Description = description,
                VideoUrl = $"https://www.youtube.com/watch?v=example",
                UpdatedAt = DateTime.Now,
                Category = category,
                Id = exerciseId,
                Difficulty = difficulty,
                Grip = grip,
                CreatedAt = createdAt == default ? DateTime.Now : createdAt
            };
            _unitOfWork.Exercises.Create(exercise);
            return exercise;
        }
        protected Feedback CreateTestFeedback(User user, Workout workout, string title = "Test Feedback",
            string type = "General", string feedbackText = "This is a test feedback",
            int? caloriesBurned = 100, int? durationMinutes = 30, Guid id = default)
        {
            var feedback = new Feedback
            {
                Id = id == default ? Guid.NewGuid() : id,
                CreatedAt = DateTime.UtcNow,
                Title = title,
                Type = type,
                FeedbackText = feedbackText,
                CaloriesBurned = caloriesBurned,
                DurationMinutes = durationMinutes,
                User = user,
                Workout = workout,
                WorkoutID = workout.Id
            };
            _unitOfWork.Feedbacks.Create(feedback);
            return feedback;
        }
    }
}
