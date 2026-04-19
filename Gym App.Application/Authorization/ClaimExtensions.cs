using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gym_App.Application.Authorization
{
    public static class ClaimExtensions
    {
        public static Guid? GetUserIdFromClaims(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? user.FindFirst("sub")
                        ?? user.FindFirst("userId")
                        ?? user.FindFirst("id");

            return claim != null && Guid.TryParse(claim.Value, out var userId) 
                ? userId 
                : null;
        }

        public static string? GetEmailFromClaims(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst(ClaimTypes.Email)?.Value 
                ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        }

        public static string? GetNameFromClaims(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst(ClaimTypes.Name)?.Value 
                ?? user.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
        }

        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user?.IsInRole(role) == true;
        }

        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            return user?.Identity?.IsAuthenticated == true;
        }
    }
}
