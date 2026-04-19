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
    public class ScheduleUpdateTests : TestBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly Mock<ICachedAuthorizationService> _authorizationServiceMock;

        public ScheduleUpdateTests() : base("ScheduleCreationTestDatabase")
        {
            _authorizationServiceMock = new Mock<ICachedAuthorizationService>();
            _scheduleService = new ScheduleService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // UPDATE SCHEDULE TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully update an existing schedule
        /// Scenario: Valid schedule ID and update data provided
        /// Expected: Schedule is updated successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithValidData_ShouldSucceed()
        {
            // ARRANGE - Set up test data
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Original Schedule",
                Type = "Original Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Mock authorization to succeed
            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var updateDto = new ScheduleCreationAndEditDTO
            {
                Name = "Updated Schedule",
                Type = "Updated Type"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT - Execute the method being tested
            var result = await _scheduleService.UpdateSchedule(schedule.Id, updateDto);

            // ASSERT - Verify the result
            Assert.NotNull(result);
            Assert.Equal(2, result.status);  // Status 2 means Success
            Assert.Equal("Schedule updated successfully", result.msg);

            // Verify the schedule was actually updated in the database
            var updatedSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.NotNull(updatedSchedule);
            Assert.Equal("Updated Schedule", updatedSchedule.Name);
            Assert.Equal("Updated Type", updatedSchedule.Type);
        }

        /// <summary>
        /// Test Case 2: Update schedule with null input
        /// Scenario: Null update data provided
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithNullInput_ShouldFail()
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

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            ScheduleCreationAndEditDTO nullSchedule = null;

            // ACT
            var result = await _scheduleService.UpdateSchedule(schedule.Id, nullSchedule);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid schedule data", result.msg);
        }

        /// <summary>
        /// Test Case 3: Update non-existent schedule
        /// Scenario: Schedule ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithNonExistentSchedule_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var nonExistentScheduleId = Guid.NewGuid();
            var updateDto = new ScheduleCreationAndEditDTO
            {
                Name = "Updated Name",
                Type = "Updated Type"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.UpdateSchedule(nonExistentScheduleId, updateDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Schedule not found", result.msg);
        }

        /// <summary>
        /// Test Case 4: Update schedule with unauthorized user
        /// Scenario: User doesn't have permission to update another user's schedule
        /// Expected: Should fail with status 1
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithUnauthorizedUser_ShouldFail()
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
            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var updateDto = new ScheduleCreationAndEditDTO
            {
                Name = "Unauthorized Update",
                Type = "Unauthorized Type"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.UpdateSchedule(schedule.Id, updateDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }

        /// <summary>
        /// Test Case 5: Update schedule with only name changed
        /// Scenario: Only name is provided, type is null
        /// Expected: Only name should be updated
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithOnlyNameChanged_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Type = "Original Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var updateDto = new ScheduleCreationAndEditDTO
            {
                Name = "Updated Name",
                Type = null
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.UpdateSchedule(schedule.Id, updateDto);

            // ASSERT
            Assert.Equal(2, result.status);

            var updatedSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.Equal("Updated Name", updatedSchedule.Name);
            Assert.Equal("Original Type", updatedSchedule.Type);  // Type should remain unchanged
        }

        /// <summary>
        /// Test Case 6: Update schedule with only type changed
        /// Scenario: Only type is provided, name is null
        /// Expected: Only type should be updated
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_WithOnlyTypeChanged_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Type = "Original Type",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Schedules.Create(schedule);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var updateDto = new ScheduleCreationAndEditDTO
            {
                Name = null,
                Type = "Updated Type"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _scheduleService.UpdateSchedule(schedule.Id, updateDto);

            // ASSERT
            Assert.Equal(2, result.status);

            var updatedSchedule = await _unitOfWork.Schedules.GetScheduleById(schedule.Id);
            Assert.Equal("Original Name", updatedSchedule.Name);  // Name should remain unchanged
            Assert.Equal("Updated Type", updatedSchedule.Type);
        }

        /// <summary>
        /// Test Case 7: Update multiple schedules sequentially
        /// Scenario: Update multiple different schedules
        /// Expected: All updates should succeed
        /// </summary>
        [Fact]
        public async Task UpdateMultipleSchedules_Sequentially_ShouldSucceed()
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

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result1 = await _scheduleService.UpdateSchedule(schedule1.Id,
                new ScheduleCreationAndEditDTO { Name = "Updated 1", Type = "New Type 1" });
            var result2 = await _scheduleService.UpdateSchedule(schedule2.Id,
                new ScheduleCreationAndEditDTO { Name = "Updated 2", Type = "New Type 2" });

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);

            var updated1 = await _unitOfWork.Schedules.GetScheduleById(schedule1.Id);
            var updated2 = await _unitOfWork.Schedules.GetScheduleById(schedule2.Id);
            Assert.Equal("Updated 1", updated1.Name);
            Assert.Equal("Updated 2", updated2.Name);
        }
    }
}
