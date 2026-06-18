using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.User;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.UserTests
{
    public class OnboardingTests  : TestBase
    {
        private readonly IUserServise _userServiceMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IEmailSender> _EmailSender;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        public OnboardingTests() : base("UserTestDatabase")
        {
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _fileService = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _EmailSender = new Mock<IEmailSender>();
            _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object, _fileService.Object, _loggerMock.Object
                , _authorizationService.Object, _EmailSender.Object);
        }
        [Fact]
        public async Task OnboardingTheDataSuccessfully()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { FitnessGoalType.Endurance.ToString(), FitnessGoalType.StrengthTraining.ToString()},
                ExerciseRestrictions = new List<string> { ExerciseRestrictionsType.NoRunning.ToString(), ExerciseRestrictionsType.UpperBodyOnly.ToString() },
                Injuries = new List<string> { InjuryType.Back.ToString(), InjuryType.Knee.ToString() },
                MedicalConditions = new List<string> { MedicalConditionsType.HeartCondition.ToString(), MedicalConditionsType.Asthma.ToString() },
            };
            var result = await _userServiceMock.OnboardingData(onboardData);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(2, user.FitnessGoals.Count);
            Assert.Equal(2, user.ExerciseRestrictions.Count);
            Assert.Equal(2, user.Injuries.Count);
            Assert.Equal(2, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task OnBoardingTheDataButSomeValuesAreWrong()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { FitnessGoalType.Endurance.ToString(), FitnessGoalType.StrengthTraining.ToString() },
                ExerciseRestrictions = new List<string> { ExerciseRestrictionsType.NoRunning.ToString(), "something something" },
                Injuries = new List<string> { InjuryType.Back.ToString(), InjuryType.Knee.ToString() },
                MedicalConditions = new List<string> { MedicalConditionsType.HeartCondition.ToString(), "something something" },
            };
            var result = await _userServiceMock.OnboardingData(onboardData);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(2, user.FitnessGoals.Count);
            Assert.Equal(1, user.ExerciseRestrictions.Count);
            Assert.Equal(2, user.Injuries.Count);
            Assert.Equal(1, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task OnBoardingTheDataButNoValues()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { },
                ExerciseRestrictions = new List<string> {  },
                Injuries = new List<string> {  },
                MedicalConditions = new List<string> { },
            };
            var result = await _userServiceMock.OnboardingData(onboardData);
            Assert.Equal(0, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(0, user.ExerciseRestrictions.Count);
            Assert.Equal(0, user.Injuries.Count);
            Assert.Equal(0, user.MedicalConditions.Count);
        }
        //Updating the data
        [Fact]
        public async Task UpdatingOnBoardingDataSuccessfully()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto();
            var result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(1, user.FitnessGoals.Count);
            Assert.Equal(1, user.ExerciseRestrictions.Count);
            Assert.Equal(1, user.Injuries.Count);
            Assert.Equal(1, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task UpdatingOnboardRemoveAndCreatSomeData()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { FitnessGoalType.Endurance.ToString(), FitnessGoalType.StrengthTraining.ToString() },
                ExerciseRestrictions = new List<string> { ExerciseRestrictionsType.NoRunning.ToString(), ExerciseRestrictionsType.UpperBodyOnly.ToString() },
            };
            var result = await _userServiceMock.OnboardingData(onboardData);
            Assert.Equal(2, result.status);

            var updateDto = createUpdateOnboardDto("r","r");
            result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(1, user.FitnessGoals.Count);
            Assert.Equal(1, user.ExerciseRestrictions.Count);
            Assert.Equal(1, user.Injuries.Count);
            Assert.Equal(1, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task UpdatingOnboardButCouldntRemove()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto("r", "r");
            var result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(0, user.ExerciseRestrictions.Count);
            Assert.Equal(1, user.Injuries.Count);
            Assert.Equal(1, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task UpdatingOnboardButCouldntAdd()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto();
            updateDto.Data.ElementAt(1).name = "wrong name";
            var result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(2, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(1, user.ExerciseRestrictions.Count);
            Assert.Equal(1, user.Injuries.Count);
            Assert.Equal(1, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task UpdatingOnboardNothingChanged()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto("r","r","r","r");
            var result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(0, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(0, user.ExerciseRestrictions.Count);
            Assert.Equal(0, user.Injuries.Count);
            Assert.Equal(0, user.MedicalConditions.Count);

            updateDto = createUpdateOnboardDto();
            foreach (var item in updateDto.Data) item.name = "wrong name";
            result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(0, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(0, user.ExerciseRestrictions.Count);
            Assert.Equal(0, user.Injuries.Count);
            Assert.Equal(0, user.MedicalConditions.Count);
        }
        [Fact]
        public async Task UpdatingOnboardButWrongRequestTypes()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto();
            foreach (var item in updateDto.Data) item.requestType = "wrong request";
            var result = await _userServiceMock.UpdateOnboardData(updateDto);
            Assert.Equal(0, result.status);

            user = await _unitOfWork.Users.GetUserById(user.Id, false);
            Assert.Equal(0, user.FitnessGoals.Count);
            Assert.Equal(0, user.ExerciseRestrictions.Count);
            Assert.Equal(0, user.Injuries.Count);
            Assert.Equal(0, user.MedicalConditions.Count);
        }

        private UpdateOnboardDataList createUpdateOnboardDto(string r1 = "c", string r2 = "c", string r3 = "c", string r4 = "c")
        {
            var updateOnboardDataDto = new UpdateOnboardDataList
            {
                Data = new List<UpdateOnboardData>
                {
                    new UpdateOnboardData
                    {
                        name = FitnessGoalType.Endurance.ToString(),
                        type = OnboardDataType.FitnessGoals.ToString(),
                        requestType = r1
                    },
                    new UpdateOnboardData
                    {
                        name = ExerciseRestrictionsType.NoRunning.ToString(),
                        type = OnboardDataType.ExerciseRestrictions.ToString(),
                        requestType = r2
                    },
                    new UpdateOnboardData
                    {
                        name = InjuryType.Knee.ToString(),
                        type = OnboardDataType.i.ToString(),
                        requestType = r3
                    },
                    new UpdateOnboardData
                    {
                        name = MedicalConditionsType.HeartCondition.ToString(),
                        type = OnboardDataType.MeidcalCondition.ToString(),
                        requestType = r4
                    },
                }
            };
            return updateOnboardDataDto;
        }
    }
}
