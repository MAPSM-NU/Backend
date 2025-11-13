using Microsoft.AspNetCore.Authorization;

namespace Gym_App.Service.Authorization
{
    public class ListUserRequirement : IAuthorizationRequirement
    {
        public bool AllowAdmins { get;}
        public ListUserRequirement(bool allowAdmins = true) => AllowAdmins = allowAdmins;
    }
}
