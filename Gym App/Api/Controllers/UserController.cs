using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Route("api/v1/user")]
    public class UserController : Controller
    {
        private readonly IUserServise _user;
        private readonly IAuthorizationService _authenticationService;
        public UserController(IUserServise user,IAuthorizationService authorizationService)
        {
            _user = user;
            _authenticationService = authorizationService;
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            //Authentication
            
            if(user == null)return BadRequest(new { message = "Invalid Data" });
            
            var authResult = await _authenticationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();
            
            //Talking to Database
            
            var result = await _user.UpdateUser(user);
            
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [Authorize(Policy ="ElevatedPower")]
        [HttpPut("change-user-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] UserChangeTypeDTO user)
        {
            var result = await _user.ChangeUserType(user);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{userID}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userID)
        {
            //Authentication

            var authResult = await _authenticationService.AuthorizeAsync(User, userID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();

            //Talking to Database

            var result = await _user.DeleteUser(userID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get/{userID}")]
        public async Task<IActionResult> GetUserByID([FromQuery] Guid UserID)
        {
            var result = await _user.GetUserByID(UserID);
            if (result == null) 
                return BadRequest(new { message = "User not found" });
            return Ok(result);
        }
        [HttpGet("get-mini-users")]
        public async Task<IActionResult> GetUsers([FromQuery]string startDate, string endDate, string sortColumn, string OrderBy, string SearchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _user.GetMiniUsers(startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsersBasicInfo([FromQuery]string startDate, string endDate, string sortColumn, string OrderBy, string SearchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _user.GetUsers(startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, int pageSize = 10)
        {
            var result = await _user.GetAllUsers(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
