using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserServise _user;
        public UserController(IUserServise user)
        {
            _user = user;
        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            var result = await _user.UpdateUser(user);
            if (result == 0) return BadRequest(new { Message = "User not found" });
            else if (result == 1) return BadRequest(new { Message = "Name is not valid" });
            else return Ok(new { Message = "User Updated Successfully" });
        }
        [HttpPut("ChangeUserType")]
        public async Task<IActionResult> ChangeUserType([FromBody] UserTypeDTO user)
        {
            var result = await _user.ChangeUserType(user);
            if (result == 4) return Ok(new { Message = "User Type Updated Successfully" });
            else if(result == 3) return BadRequest(new { Message = "The user is already that type" });
            else if (result == 2) return BadRequest(new { Message = "User not found" });
            else if (result == 1) return BadRequest(new { Message = "User Type is not valid" });
            else return BadRequest(new { Message = "User not found" });

        }
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] Guid UserID)
        {
            var result = await _user.DeleteUser(UserID);
            if (result) return Ok(new { Message = "User Deleted Successfully" });
            return BadRequest(new { Message = "Failed to Delete User"});
        }
        [HttpPost("GetUserByID")]
        public async Task<IActionResult> GetUserByID([FromBody] Guid UserID)
        {
            var result = await _user.GetUserByID(UserID);
            if (result == null) return BadRequest(new { message = "User not found" });
            return Ok(result);
        }
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _user.GetAllUsers();
            return Ok(users);
        }
    }
}
