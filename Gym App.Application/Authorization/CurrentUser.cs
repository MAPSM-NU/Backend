using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gym_App.Application.Authorization
{

namespace Gym_App.Application.Authorization
    {
        public class CurrentUser : ICurrentUser
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public CurrentUser(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }
            public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User ?? null;
            public Guid? UserID => User!.GetUserIdFromClaims();
            public string? Email => User!.GetEmailFromClaims();
            public string? Name => User!.GetNameFromClaims();
            public bool IsAuthenticated => User!.IsAuthenticated();
            public bool IsInRole(string role) => User!.HasRole(role);
        }
    }

}
