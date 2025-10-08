using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserServise _user;
        public UserController(IUserServise user)
        {
            _user = user;
        }
        [HttpPut("/Update User")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            var result = await _user.UpdateUser(user);
            if (result == 0) return BadRequest(new { message = "User not found" });
            else if (result == 2) return BadRequest(new { message = "Name is not valid" });
            return Ok(new { message = "User Updated Successfully" });
        }
        [HttpPut("/Change User Type")]
        public async Task<IActionResult> ChangeUserType([FromBody] UserTypeDTO user)
        {
            var result = await _user.ChangeUserType(user);
            if (result == 0) return BadRequest(new { message = "Failed to Update User" });
            return Ok(new { message = "User Updated Successfully" });

        }
        [HttpGet("/Get All Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _user.GetAllUsers();
            return Ok(users);
        }
        [HttpDelete("/Delete User")]
        public async Task<IActionResult> DeleteUser([FromBody] Guid UserID)
        {
            var result = await _user.DeleteUser(UserID);
            if (result) return Ok("User Deleted Successfully");
            return BadRequest("Failed to Delete User");
        }
    }
}
