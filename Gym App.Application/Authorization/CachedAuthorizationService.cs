using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Gym_App.Application.Authorization
{
    public interface ICachedAuthorizationService
    {
        Task<bool> IsAuthorizedAsync(string policy, object resource);
        Task<bool> IsUserAsync(Guid userId);
        Task<bool> IsInRoleAsync(string role);
        Guid? GetCurrentUserId();
    }

    public class CachedAuthorizationService : ICachedAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUser _currentUser;
        private readonly IMemoryCache _cache;

        private const string CACHE_PREFIX = "auth_";
        private const int CACHE_DURATION_MINUTES = 5;

        public CachedAuthorizationService(
            IAuthorizationService authorizationService,
            ICurrentUser currentUser,
            IMemoryCache cache)
        {
            _authorizationService = authorizationService;
            _currentUser = currentUser;
            _cache = cache;
        }

        public async Task<bool> IsAuthorizedAsync(string policy, object resource)
        {
            if (!_currentUser.IsAuthenticated)
                return false;

            var cacheKey = GenerateCacheKey(_currentUser.UserID, policy, resource);

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            var authResult = await _authorizationService.AuthorizeAsync(
                _currentUser.User,
                resource,
                policy);

            var result = authResult.Succeeded;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }

        public async Task<bool> IsUserAsync(Guid userId)
        {
            return await IsAuthorizedAsync("SameUserPolicy", userId);
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            return _currentUser.IsInRole(role);
        }

        public Guid? GetCurrentUserId()
        {
            return _currentUser.UserID;
        }

        private static string GenerateCacheKey(Guid? userId, string policy, object resource)
        {
            return $"{CACHE_PREFIX}{userId}_{policy}_{resource}";
        }
    }
}
