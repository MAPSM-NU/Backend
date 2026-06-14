using Gym_App.Application.Authorization;
using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.UserStats;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Services
{
    public class UserStatsService : IUserStatsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICachedAuthorizationService _authorizationService;
        private readonly ILogger<UserStatsService> _logger;
        public UserStatsService(IUnitOfWork unitOfWork, ICachedAuthorizationService authorizationService, ILogger<UserStatsService> logger)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _logger = logger;
        }
        public Task<SettersResponse> AddDailyStats(Workout workout)
        {
            throw new NotImplementedException();
        }

        public Task<SettersResponse> AddWeeklyStats(Workout workout)
        {
            throw new NotImplementedException();
        }
        public Task<SettersResponse> AddWeeklyStats(Workout workout, UserStatsDaily usd)
        {
            throw new NotImplementedException();
        }

        public Task<SettersResponse> AddMonthlyStats(Workout workout)
        {
            throw new NotImplementedException();
        }
        public Task<SettersResponse> AddMonthlyStats(Workout workout, UserStatsWeekly usw)
        {
            throw new NotImplementedException();
        }

        public Task<GettersResponse<UserStatsDailyDTO>> GetDailyStats(Guid userId, DateOnly date)
        {
            throw new NotImplementedException();
        }
        public Task<GettersResponse<UserStatsWeeklyDTO>> GetWeeklyStats(Guid userId, int weekNumber, int year)
        {
            throw new NotImplementedException();
        }

        public Task<GettersResponse<UserStatsMonthlyDTO>> GetMonthlyStats(Guid userId, string monthName, int year)
        {
            throw new NotImplementedException();
        }

        public Task<GettersResponse<UserStatsDTO>> GetOverallStats(Guid userId)
        {
            throw new NotImplementedException();
        }


    }
}
