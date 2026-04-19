using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace GymApp.Tests.ScheduleTests
{
    public class ScheduleGetTests : TestBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly Mock<ICachedAuthorizationService> _authorizationServiceMock;

        public ScheduleGetTests() : base("ScheduleCreationTestDatabase")
        {
            _authorizationServiceMock = new Mock<ICachedAuthorizationService>();
            _scheduleService = new ScheduleService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // GET SCHEDULE TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully retrieve schedule by ID
        /// Scenario: Valid schedule ID provided
        /// Expected: Schedule data is returned successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task GetScheduleById_WithValidId_ShouldSucceed()
        {
            // ARRANGE - Set up test data
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Test Schedule",
                Type = "Full Body",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // ACT - Execute the method being tested
            var result = await _scheduleService.GetScheduleById(schedule.Id);

            // ASSERT - Verify the result
            Assert.NotNull(result);
            Assert.Equal(2, result.status);  // Status 2 means Success
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Schedule", result.Value.Name);
            Assert.Equal("Full Body", result.Value.Type);
            Assert.Equal(schedule.Id, result.Value.ScheduleID);
            Assert.Equal(user.Id, result.Value.UserID);
        }

        /// <summary>
        /// Test Case 2: Get schedule with non-existent ID
        /// Scenario: Schedule ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task GetScheduleById_WithNonExistentId_ShouldFail()
        {
            // ARRANGE
            var nonExistentScheduleId = Guid.NewGuid();

            // ACT
            var result = await _scheduleService.GetScheduleById(nonExistentScheduleId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Schedule not found", result.msg);
            Assert.Null(result.Value);
        }

        /// <summary>
        /// Test Case 3: Get schedule workouts
        /// Scenario: Schedule has multiple workouts assigned
        /// Expected: All workout IDs should be returned successfully
        /// </summary>
        [Fact]
        public async Task GetScheduleWorkouts_WithAssignedWorkouts_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var workout1 = CreateTestWorkout(user, "Workout 1");
            var workout2 = CreateTestWorkout(user, "Workout 2");
            var workout3 = CreateTestWorkout(user, "Workout 3");
            await _unitOfWork.SaveChangesAsync();

            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule with Workouts",
                Type = "Full Body",
                User = user,
                CreatedAt = DateTime.UtcNow,
                Workouts = new List<Workout> { workout1, workout2, workout3 }
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetScheduleWorkouts(schedule.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.WorkoutsID);
            Assert.Equal(3, result.Value.WorkoutsID.Count);
            Assert.Contains(workout1.Id, result.Value.WorkoutsID);
            Assert.Contains(workout2.Id, result.Value.WorkoutsID);
            Assert.Contains(workout3.Id, result.Value.WorkoutsID);
        }

        /// <summary>
        /// Test Case 4: Get workouts for schedule with no workouts
        /// Scenario: Schedule exists but has no workouts assigned
        /// Expected: Should return empty list successfully
        /// </summary>
        [Fact]
        public async Task GetScheduleWorkouts_WithNoWorkouts_ShouldReturnEmptyList()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Empty Schedule",
                Type = "No Workouts",
                User = user,
                CreatedAt = DateTime.UtcNow,
                Workouts = new List<Workout>()
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetScheduleWorkouts(schedule.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.WorkoutsID);
            Assert.Empty(result.Value.WorkoutsID);
        }

        /// <summary>
        /// Test Case 5: Get workouts for non-existent schedule
        /// Scenario: Schedule ID doesn't exist
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task GetScheduleWorkouts_WithNonExistentSchedule_ShouldFail()
        {
            // ARRANGE
            var nonExistentScheduleId = Guid.NewGuid();

            // ACT
            var result = await _scheduleService.GetScheduleWorkouts(nonExistentScheduleId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Schedule not found", result.msg);
            Assert.Null(result.Value);
        }

        /// <summary>
        /// Test Case 6: Get all schedules with pagination
        /// Scenario: Multiple schedules exist in database
        /// Expected: Should return paginated results successfully
        /// </summary>
        [Fact]
        public async Task GetAllSchedules_WithPagination_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user1 = CreateTestUser(role, email: "user1@gmail.com");
            var user2 = CreateTestUser(role, email: "user2@gmail.com");

            var schedule1 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 1",
                Type = "Type 1",
                User = user1,
                CreatedAt = DateTime.UtcNow
            };
            var schedule2 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 2",
                Type = "Type 2",
                User = user1,
                CreatedAt = DateTime.UtcNow
            };
            var schedule3 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 3",
                Type = "Type 3",
                User = user2,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Schedules.Create(schedule1);
            _unitOfWork.Schedules.Create(schedule2);
            _unitOfWork.Schedules.Create(schedule3);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetAllSchedules(page: 1, pageSize: 10);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TotalCount >= 3);
        }

        /// <summary>
        /// Test Case 7: Get schedules for specific user
        /// Scenario: Retrieve schedules filtered by user ID with date range
        /// Expected: Should return user's schedules successfully
        /// </summary>
        [Fact]
        public async Task GetSchedulesByUser_WithDateRange_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var otherUser = CreateTestUser(role, email: "other@gmail.com");

            var now = DateTime.UtcNow;
            var schedule1 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "User Schedule 1",
                Type = "Type 1",
                User = user,
                CreatedAt = now
            };
            var schedule2 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Other User Schedule",
                Type = "Type 2",
                User = otherUser,
                CreatedAt = now
            };

            _unitOfWork.Schedules.Create(schedule1);
            _unitOfWork.Schedules.Create(schedule2);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetSchedulesByOfUser(
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "CreatedAt",
                OrderBy: "DESC",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            var userSchedules = result.Data.Items.Where(s => s.UserID == user.Id).ToList();
            Assert.NotEmpty(userSchedules);
        }

        /// <summary>
        /// Test Case 8: Get user ID from schedule
        /// Scenario: Retrieve user ID from a schedule
        /// Expected: Should return correct user ID
        /// </summary>
        [Fact]
        public async Task GetScheduleUserId_WithValidSchedule_ShouldReturnUserId()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Test Schedule",
                Type = "Test Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetScheduleUserID(schedule.Id);

            // ASSERT
            Assert.Equal(user.Id, result);
        }

        /// <summary>
        /// Test Case 9: Get multiple schedule details
        /// Scenario: Retrieve details for multiple schedules
        /// Expected: All schedule details should be correct
        /// </summary>
        [Fact]
        public async Task GetMultipleSchedules_ByIds_ShouldReturnAllCorrectly()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var schedule1 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 1",
                Type = "Type 1",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            var schedule2 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 2",
                Type = "Type 2",
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Schedules.Create(schedule1);
            _unitOfWork.Schedules.Create(schedule2);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result1 = await _scheduleService.GetScheduleById(schedule1.Id);
            var result2 = await _scheduleService.GetScheduleById(schedule2.Id);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);
            Assert.Equal("Schedule 1", result1.Value.Name);
            Assert.Equal("Schedule 2", result2.Value.Name);
            Assert.Equal("Type 1", result1.Value.Type);
            Assert.Equal("Type 2", result2.Value.Type);
        }

        /// <summary>
        /// Test Case 10: Get schedule with specific data verification
        /// Scenario: Verify all properties of retrieved schedule
        /// Expected: All properties should match created schedule
        /// </summary>
        [Fact]
        public async Task GetScheduleById_VerifyAllProperties_ShouldMatchCreatedData()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "verify@gmail.com");
            var createdAt = DateTime.UtcNow;

            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Verification Schedule",
                Type = "Full Body",
                User = user,
                CreatedAt = createdAt
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _scheduleService.GetScheduleById(schedule.Id);

            // ASSERT
            Assert.NotNull(result.Value);
            Assert.Equal(schedule.Name, result.Value.Name);
            Assert.Equal(schedule.Type, result.Value.Type);
            Assert.Equal(schedule.User.Id, result.Value.UserID);
            Assert.Equal(schedule.Id, result.Value.ScheduleID);
            Assert.Equal(createdAt, result.Value.CreatedAt);
        }
    }
}
