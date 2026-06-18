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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { FitnessGoalType.Endurance.ToString(), FitnessGoalType.StrengthTraining.ToString()},
                ExerciseRestrictions = new List<string> { ExerciseRestrictionsType.NoRunning.ToString(), ExerciseRestrictionsType.UpperBodyOnly.ToString() },
                Injuries = new List<string> { InjuryType.Back.ToString(), InjuryType.Knee.ToString() },
                MedicalConditions = new List<string> { MedicalConditionsType.HeartCondition.ToString(), MedicalConditionsType.Asthma.ToString() },
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { "Endurance", FitnessGoalType.StrengthTraining.ToString() },
                ExerciseRestrictions = new List<string> { ExerciseRestrictionsType.NoRunning.ToString(), "something something" },
                Injuries = new List<string> { InjuryType.Back.ToString(), InjuryType.Knee.ToString() },
                MedicalConditions = new List<string> { MedicalConditionsType.HeartCondition.ToString(), "something something" },
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            var onboardData = new OnboardDataCreationDTO
            {
                userId = user.Id,
                FitnessGoals = new List<string> { },
                ExerciseRestrictions = new List<string> {  },
                Injuries = new List<string> {  },
                MedicalConditions = new List<string> { },
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            var updateDto = createUpdateOnboardDto();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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
            await AddOnboardingData();
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
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

        private async Task AddOnboardingData()
        {
            var FitnessGoals = new List<FitnessGoals> {
                new FitnessGoals { Name = FitnessGoalType.Endurance.ToString(), Id = Guid.Parse("7b1f4c4d-5c0e-4f6d-8d8d-9f4c6b8a2e11") },
                new FitnessGoals { Name = FitnessGoalType.GeneralFitness.ToString(), Id = Guid.Parse("2e8a1f93-6c47-4d7f-9c21-4b5e7d3f8a22") },
                new FitnessGoals { Name = FitnessGoalType.StrengthTraining.ToString(), Id = Guid.Parse("91d3b6e7-8a54-4f12-b8c4-7e9f2d1a3b33") },
                new FitnessGoals { Name = FitnessGoalType.WeightLoss.ToString(), Id = Guid.Parse("4c7e2a11-9f85-4d3b-a1e7-5d8c6b2f4c44") },
                new FitnessGoals { Name = FitnessGoalType.MuscleGain.ToString(), Id = Guid.Parse("8f2d7c3a-1b64-4e9d-9a72-3c5e8f1b5d55") },
                new FitnessGoals { Name = FitnessGoalType.Flexibility.ToString(), Id = Guid.Parse("5a9c1e7f-3d82-4b6e-b4f8-2d7c9a6e1f66") }
            };
            var injuries = new List<Injury>
            {
                new Injury { Name = InjuryType.Back.ToString(), Id = Guid.Parse("a1f3c7d9-5b42-4e8a-9d71-2c6f8b3e7a11") },
                new Injury { Name = InjuryType.Knee.ToString(), Id = Guid.Parse("b2e4d8f1-6c53-4f9b-a182-3d7a9c4f8b22") },
                new Injury { Name = InjuryType.Shoulder.ToString(), Id = Guid.Parse("c3f5e9a2-7d64-4a1c-b293-4e8b1d5a9c33") },
                new Injury { Name = InjuryType.Ankle.ToString(), Id = Guid.Parse("d4a6f1b3-8e75-4b2d-c3a4-5f9c2e6b1d44") },
                new Injury { Name = InjuryType.Wrist.ToString(), Id = Guid.Parse("e5b7a2c4-9f86-4c3e-d4b5-6a1d3f7c2e55") },
                new Injury { Name = InjuryType.Hip.ToString(), Id = Guid.Parse("f6c8b3d5-1a97-4d4f-e5c6-7b2e4a8d3f66") }
            };
            var exerciseRestrictions = new List<ExerciseRestrictions>
            {
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.NoHeavyLifting.ToString(), Id = Guid.Parse("1d7a4c9e-2b58-4f6a-8c71-9e3d5b7a1c11") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.NoRunning.ToString(), Id = Guid.Parse("2e8b5d1f-3c69-4a7b-9d82-1f4e6c8b2d22") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.LowerBodyOnly.ToString(), Id = Guid.Parse("3f9c6e2a-4d7a-4b8c-a193-2a5f7d9c3e33") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.LowImpactOnly.ToString(), Id = Guid.Parse("4a1d7f3b-5e8b-4c9d-b2a4-3b6a8e1d4f44") },
                new ExerciseRestrictions { Name = ExerciseRestrictionsType.UpperBodyOnly.ToString(), Id = Guid.Parse("5b2e8a4c-6f9c-4d1e-c3b5-4c7b9f2e5a55") }
            };
            var Medical = new List<MedicalCondition>
            {
                new MedicalCondition { Name = MedicalConditionsType.HeartCondition.ToString(), Id = Guid.Parse("6e3f2a91-4d8b-47c6-a2e1-9f5d7b3c8a77") },
                new MedicalCondition { Name = MedicalConditionsType.HighBloodPressure.ToString(), Id = Guid.Parse("7f4a3b82-5e9c-48d7-b3f2-a6e8c4d9b188") },
                new MedicalCondition { Name = MedicalConditionsType.Arthritis.ToString(), Id = Guid.Parse("8a5b4c73-6f1d-49e8-c4a3-b7f9d5e1c299") },
                new MedicalCondition { Name = MedicalConditionsType.Asthma.ToString(), Id = Guid.Parse("9b6c5d64-7a2e-4af9-d5b4-c8a1e6f2d3aa") },
                new MedicalCondition { Name = MedicalConditionsType.Diabetes.ToString(), Id = Guid.Parse("ac7d8e91-3f42-4b6a-9c15-e7d2f8a4b3c1") }
            };
            foreach(var item in FitnessGoals)
            {
                await _unitOfWork.FitnessGoals.Create(item);
            }
            foreach(var item in injuries)
            {
                await _unitOfWork.Injuries.Create(item);
            }
            foreach(var item in exerciseRestrictions)
            {
                await _unitOfWork.ExerciseRestrictions.Create(item);
            }
            foreach(var item in Medical)
            {
                await _unitOfWork.MedicalConditions.Create(item);
            }
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
