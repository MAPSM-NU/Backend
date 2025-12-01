using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Gym_App.Application.Authorization
{
    public class SameUserRequirement : IAuthorizationRequirement
    {
        public bool AllowAdmins { get; }
        public SameUserRequirement(bool allowAdmins = true) => AllowAdmins = allowAdmins;
    }
}
