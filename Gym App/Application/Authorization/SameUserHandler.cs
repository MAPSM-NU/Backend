using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gym_App.Application.Authorization
{
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserRequirement requirement, Guid resource)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            // Admin bypass
            if (requirement.AllowAdmins && context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Try common claim names for user id
            var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? context.User.FindFirst("sub")
                        ?? context.User.FindFirst("userId")
                        ?? context.User.FindFirst("id");

            if (claim != null && Guid.TryParse(claim.Value, out var tokenUserID) && tokenUserID == resource)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
