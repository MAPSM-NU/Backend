using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Gym_App.Application.Authorization
{
    public class ListUserHandler : AuthorizationHandler<ListUserRequirement,List<Guid>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ListUserRequirement requirement, List<Guid> resources)
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

            // Check if user ID is in the resource list
            var userId = context.User.GetUserIdFromClaims();
            if (userId.HasValue && resources.Contains(userId.Value))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
