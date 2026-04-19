using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.ScheduleTests
{
    public class ScheduleCreationTests : TestBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly Mock<ICachedAuthorizationService> _authorizationServiceMock;

        public ScheduleCreationTests() : base("ScheduleCreationTestDatabase")
        {
            _authorizationServiceMock = new Mock<ICachedAuthorizationService>();
            _scheduleService = new ScheduleService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // CREATE SCHEDULE TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully create a new schedule
        /// Scenario: Valid schedule data and user provided
        /// Expected: Schedule is created successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task CreateSchedule_WithValidData_ShouldSucceed()
        {
            // ARRANGE - Set up test data
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            // Mock authorization to succeed
            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var scheduleDto = new ScheduleCreationAndEditDTO
            {
                Name = "Monday Workout",
                Type = "Full Body"
            };

            // Create a claims principal for the user
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // ACT - Execute the method being tested
            var result = await _scheduleService.AddSchedule(user.Id, scheduleDto);

            // ASSERT - Verify the result
            Assert.NotNull(result);
            Assert.Equal(2, result.status);  // Status 2 means Success
            Assert.Equal("Schedule created successfully", result.msg);

            // Verify the schedule was actually saved to the database
            var savedSchedule = await _unitOfWork.Schedules.GetScheduleById(
                _unitOfWork.Schedules.GetAll().FirstOrDefault()?.Id ?? Guid.Empty);
            Assert.NotNull(savedSchedule);
            Assert.Equal("Monday Workout", savedSchedule.Name);
            Assert.Equal("Full Body", savedSchedule.Type);
        }

        /// <summary>
        /// Test Case 2: Create schedule with null input
        /// Scenario: Null schedule data provided
        /// Expected: Should fail with status 0 (Bad Request)
        /// </summary>
        [Fact]
        public async Task CreateSchedule_WithNullInput_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            ScheduleCreationAndEditDTO nullSchedule = null;

            // ACT
            var result = await _scheduleService.AddSchedule(user.Id, nullSchedule);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);  // Status 0 means error/bad request
            Assert.Equal("Invalid schedule data", result.msg);
        }

        /// <summary>
        /// Test Case 3: Create schedule with non-existent user
        /// Scenario: User ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task CreateSchedule_WithNonExistentUser_ShouldFail()
        {
            // ARRANGE
            var nonExistentUserId = Guid.NewGuid();

            var scheduleDto = new ScheduleCreationAndEditDTO
            {
                Name = "Test Schedule",
                Type = "Upper Body"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, nonExistentUserId.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.AddSchedule(nonExistentUserId, scheduleDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("User not found", result.msg);
        }

        /// <summary>
        /// Test Case 4: Create schedule with unauthorized user
        /// Scenario: User doesn't have permission to create schedule for another user
        /// Expected: Should fail with status 1 (Unauthorized)
        /// </summary>
        [Fact]
        public async Task CreateSchedule_WithUnauthorizedUser_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var unauthorizedUser = CreateTestUser(role, email: "other@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            // Mock authorization to fail
            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var scheduleDto = new ScheduleCreationAndEditDTO
            {
                Name = "Unauthorized Schedule",
                Type = "Lower Body"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.AddSchedule(user.Id, scheduleDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);  // Status 1 means Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }

        /// <summary>
        /// Test Case 5: Create schedule with empty name and type
        /// Scenario: Schedule name and type are empty/whitespace
        /// Expected: Should still create (fields are not validated as required in service)
        /// </summary>
        [Fact]
        public async Task CreateSchedule_WithEmptyNameAndType_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var scheduleDto = new ScheduleCreationAndEditDTO
            {
                Name = "  ",
                Type = "  "
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.AddSchedule(user.Id, scheduleDto);

            // ASSERT - Service creates schedule even with empty values
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Schedule created successfully", result.msg);
        }

        /// <summary>
        /// Test Case 6: Create multiple schedules for same user
        /// Scenario: User creates multiple schedules
        /// Expected: All schedules should be created successfully
        /// </summary>
        [Fact]
        public async Task CreateMultipleSchedules_ForSameUser_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var schedule1Dto = new ScheduleCreationAndEditDTO { Name = "Schedule 1", Type = "Type 1" };
            var schedule2Dto = new ScheduleCreationAndEditDTO { Name = "Schedule 2", Type = "Type 2" };

            // ACT
            var result1 = await _scheduleService.AddSchedule(user.Id, schedule1Dto);
            var result2 = await _scheduleService.AddSchedule(user.Id, schedule2Dto);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);

            var allSchedules = _unitOfWork.Schedules.GetAll().Where(s => s.User.Id == user.Id).ToList();
            Assert.Equal(2, allSchedules.Count);
        }
    }
}
