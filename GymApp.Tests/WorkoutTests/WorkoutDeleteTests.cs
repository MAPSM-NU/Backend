using Gym_App.Application.Services;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.WorkoutTests
{
     public class WorkoutDeleteTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<IAuthorizationService> _authorizationService;
        public WorkoutDeleteTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        //Deleting workout with all its possibilities of failure
        [Fact]
        public async Task DeleteWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), workout.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout deleted successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task DeleteNonExistingWorkoutTest()
        {
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task DeleteWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), workout.Id);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task DeleteWorkoutWithInvalidIdTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), Guid.Empty);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid workout ID", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
