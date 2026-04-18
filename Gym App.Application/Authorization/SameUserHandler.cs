using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gym_App.Application.Authorization
{
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserRequirement requirement, Guid resource)
        {
            // Check if user is authenticated
            if (!context.User.IsAuthenticated())
                return Task.CompletedTask;

            // Admin bypass
            if (requirement.AllowAdmins && context.User.HasRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user ID matches resource
            var userId = context.User.GetUserIdFromClaims();
            if (userId.HasValue && userId.Value == resource)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
