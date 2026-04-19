using System.Security.Claims;

namespace Gym_App.Application.Authorization
{
    public interface ICurrentUser
    {
        ClaimsPrincipal? User { get; }
        Guid? UserID { get; }
        string? Email { get; }
        string? Name { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
