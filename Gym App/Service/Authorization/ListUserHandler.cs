using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Gym_App.Service.Authorization
{
    public class ListUserHandler : AuthorizationHandler<ListUserRequirement,List<Guid>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ListUserRequirement requirement, List<Guid> resources)
        {
            if(context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;
            
            if(requirement.AllowAdmins && context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? context.User.FindFirst("sub")
                        ?? context.User.FindFirst("userId")
                        ?? context.User.FindFirst("id");

            if (claim != null && Guid.TryParse(claim.Value, out var tokenUserID) && resources.Contains(tokenUserID))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
