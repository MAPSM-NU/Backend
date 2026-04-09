using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.ScheduleTests
{
    public class ScheduleDeletionTests : TestBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public ScheduleDeletionTests() : base("ScheduleDeletionTestDatabase")
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _scheduleService = new ScheduleService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // DELETE SCHEDULE TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully delete an existing schedule
        /// Scenario: Valid schedule ID provided and user authorized
        /// Expected: Schedule is deleted successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_WithValidData_ShouldSucceed()
        {
            // ARRANGE - Set up test data
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule to Delete",
                Type = "Delete Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Mock authorization to succeed
            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT - Execute the method being tested
            var result = await _scheduleService.DeleteSchedule(principal, schedule.Id);

            // ASSERT - Verify the result
            Assert.NotNull(result);
            Assert.Equal(2, result.status);  // Status 2 means Success
            Assert.Equal("Schedule deleted successfully", result.msg);

            // Verify the schedule was actually deleted from the database
            var deletedSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.Null(deletedSchedule);
        }

        /// <summary>
        /// Test Case 2: Delete schedule with empty GUID
        /// Scenario: Empty GUID provided as schedule ID
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_WithEmptyGuid_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.DeleteSchedule(principal, Guid.Empty);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid schedule ID", result.msg);
        }

        /// <summary>
        /// Test Case 3: Delete non-existent schedule
        /// Scenario: Schedule ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_WithNonExistentSchedule_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var nonExistentScheduleId = Guid.NewGuid();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.DeleteSchedule(principal, nonExistentScheduleId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Schedule not found", result.msg);
        }

        /// <summary>
        /// Test Case 4: Delete schedule with unauthorized user
        /// Scenario: User doesn't have permission to delete another user's schedule
        /// Expected: Should fail with status 1
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_WithUnauthorizedUser_ShouldFail()
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

            var unauthorizedUser = CreateTestUser(role, email: "other@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            // Mock authorization to fail
            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.DeleteSchedule(principal, schedule.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);

            // Verify schedule still exists
            var stillExistingSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.NotNull(stillExistingSchedule);
        }

        /// <summary>
        /// Test Case 5: Delete schedule with workouts
        /// Scenario: Schedule contains assigned workouts
        /// Expected: Schedule should be deleted (workouts may have cascade delete or remain orphaned)
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_WithAssignedWorkouts_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule with Workouts",
                Type = "Full Body",
                User = user,
                CreatedAt = DateTime.UtcNow,
                Workouts = new List<Workout>()
            };

            var workout = CreateTestWorkout(user, "Test Workout with Schedule");
            schedule.Workouts.Add(workout);

            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.DeleteSchedule(principal, schedule.Id);

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.Equal("Schedule deleted successfully", result.msg);

            var deletedSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.Null(deletedSchedule);
        }

        /// <summary>
        /// Test Case 6: Delete multiple schedules sequentially
        /// Scenario: Delete multiple different schedules one after another
        /// Expected: All deletions should succeed
        /// </summary>
        [Fact]
        public async Task DeleteMultipleSchedules_Sequentially_ShouldSucceed()
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
            var schedule3 = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 3",
                Type = "Type 3",
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Schedules.Create(schedule1);
            _unitOfWork.Schedules.Create(schedule2);
            _unitOfWork.Schedules.Create(schedule3);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result1 = await _scheduleService.DeleteSchedule(principal, schedule1.Id);
            var result2 = await _scheduleService.DeleteSchedule(principal, schedule2.Id);
            var result3 = await _scheduleService.DeleteSchedule(principal, schedule3.Id);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);
            Assert.Equal(2, result3.status);

            var allSchedules = _unitOfWork.Schedules.GetAll().Where(s => s.User.Id == user.Id).ToList();
            Assert.Empty(allSchedules);
        }

        /// <summary>
        /// Test Case 7: Delete same schedule twice
        /// Scenario: Attempt to delete a schedule that was already deleted
        /// Expected: Second deletion should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_DeleteTwice_SecondShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Schedule to Delete Twice",
                Type = "Test Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT - Delete once
            var result1 = await _scheduleService.DeleteSchedule(principal, schedule.Id);

            // Try to delete again
            var result2 = await _scheduleService.DeleteSchedule(principal, schedule.Id);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal("Schedule deleted successfully", result1.msg);

            Assert.Equal(0, result2.status);
            Assert.Equal("Schedule not found", result2.msg);
        }
    }
}
