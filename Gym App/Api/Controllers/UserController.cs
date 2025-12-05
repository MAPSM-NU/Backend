using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserServise _user;
        private readonly IAuthorizationService _authenticationService;
        public UserController(IUserServise user,IAuthorizationService authorizationService)
        {
            _user = user;
            _authenticationService = authorizationService;
        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            //Authentication
            
            if(user == null)return BadRequest(new { message = "Invalid Data" });
            
            var authResult = await _authenticationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();
            
            //Talking to Database
            
            var result = await _user.UpdateUser(user);
            
            if(result == 3)
                return Ok(new { message = "User Updated Successfully" });
            else if (result == 2) 
                return BadRequest(new { message = "Name is not valid" });
            else if (result == 1)
                return BadRequest(new { message = "User not found" });
            else
                return BadRequest(new { message = "Faulty DTO" });
        }
        [Authorize(Policy ="ElevatedPower")]
        [HttpPut("ChangeUserType")]
        public async Task<IActionResult> ChangeUserType([FromBody] UserChangeTypeDTO user)
        {
            var result = await _user.ChangeUserType(user);
            
            if(result == 4)
                return Ok(new { message = "User Type Changed Successfully" });
            else if(result == 3)
                return BadRequest(new {message = "Same user type given"});
            else if (result == 2)
                return BadRequest(new { message = "User not found" });
            else if (result == 1)
                return BadRequest(new { message = "invalid UserType" });
            else
                return BadRequest(new { message = "Faulty DTO" });
        }
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid UserID)
        {
            //Authentication

            var authResult = await _authenticationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();

            //Talking to Database

            var result = await _user.DeleteUser(UserID);
            if(result == 2)
                return Ok(new { message = "User Deleted Successfully" });
            else if (result == 1)
                return BadRequest(new { message = "User not found" });
            else
                return BadRequest(new { message = "Faulty DTO" });
        }
        [HttpGet("GetUserByID")]
        public async Task<IActionResult> GetUserByID([FromQuery] Guid UserID)
        {
            var result = await _user.GetUserByID(UserID);
            if (result == null) 
                return BadRequest(new { message = "User not found" });
            return Ok(result);
        }
        [HttpGet("GetUsersByFilter")]
        public async Task<IActionResult> GetUsersByFilter([FromQuery]string startDate, string endDate, string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var users = await _user.GetUsersByFilter(startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            return Ok(users);
        }
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page, int pageSize)
        {
            var users = await _user.GetAllUsers(page,pageSize);
            return Ok(users);
        }
    }
}
