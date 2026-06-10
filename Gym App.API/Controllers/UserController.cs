using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Services;
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
        [HttpPut("change-pfp")]
        public async Task<IActionResult> ChangePfp([FromForm] Guid userId, IFormFile pfp)
        {
            var result = await _user.ChangePfp(userId, pfp);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete-pfp/{userId}")]
        public async Task<IActionResult> DeletePfp([FromRoute] Guid userId)
        {
            var result = await _user.DeletePfp(userId);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDTO user)
        {
            //Authentication
            
            if(user == null)return BadRequest(new { message = "Invalid Data" });
            
            var authResult = await _authenticationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
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
        public async Task<IActionResult> GetUserByID([FromRoute] Guid userID)
        {
            var result = await _user.GetUserByID(userID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-user-stats/{userID}")]
        public async Task<IActionResult> GetUserStatsByID([FromRoute] Guid userID)
        {
            throw new NotImplementedException();
            //var result = await _user.GetUserStatsByID(userID);
            //if (result.status == 0)
            //    return BadRequest(new { message = result.msg });
            //else if (result.status == 1)
            //    return Forbid();
            //else
            //    return Ok(result.Value);
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
