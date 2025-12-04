using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Route("[controller]")]
    public class Authentication : Controller
    {
        private readonly IUserServise _userServiceService;
        private readonly IEmailSender _emailSender;//Still have not made the email verification service
        private readonly ITokenHandler _tokenHandler;
        public Authentication(IUserServise userService , IEmailSender emailSender,ITokenHandler tokenHandler)
        {
            _userServiceService = userService;
            _emailSender = emailSender;
            _tokenHandler = tokenHandler;
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpPost("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] UserCreationDTO user)
        {
            var result = await _userServiceService.CreateAdmin(user);
            if (result.Status == 5)
                return Ok(result);
            else if (result.Status == 4)
                return BadRequest("Password is not valid");
            else if (result.Status == 3)
                return BadRequest("Email is not valid");
            else if (result.Status == 2)
                return BadRequest("Email is already in use");
            else if (result.Status == 1)
                return BadRequest("Name is already in use");
            else if (result.Status == 0)
                return BadRequest("Missing Credentials");
            else
                return BadRequest("Failed to Add Admin");
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> NewUser([FromBody] UserCreationDTO user)
        {
            var result = await _userServiceService.SignUpUser(user);
            if (result.Status == 5)
                return Ok(result);
            else if (result.Status == 4)
                return BadRequest("Password is not valid");
            else if (result.Status == 3)
                return BadRequest("Email is not valid");
            else if (result.Status == 2)
                return BadRequest("Email is already in use");
            else if (result.Status == 1)
                return BadRequest("Name is already in use");
            else if (result.Status == 0)
                return BadRequest("Missing Credentials");
            else
                return BadRequest("Failed to Add User");
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> LoginUser([FromBody] UserCreationDTO user)
        {
            var result = await _userServiceService.LoginUser(user);
            if(result.Status == 0) 
                return BadRequest("Email not found");
            else if (result.Status == 1) 
                return BadRequest("Password is wrong");
            else if (result.Status == 2)
                return Ok(result);
            else
                return BadRequest("Failed to Login");
        }
        [HttpPost("LoginbyToken")]
        public async Task<IActionResult> LoginByToken([FromQuery] string Refreshtoken)
        {
            var result = await _tokenHandler.ValidateAccessToken(Refreshtoken);
            if (result == null)
                return BadRequest("Invalid Token");
            else
                return Ok(result);
        }
        [HttpGet("GetTokens")]
        public async Task<IActionResult> GetTokens([FromQuery]int page,int pageSize)
        {
            var result = await _tokenHandler.GetAllRefreshTokens(page,pageSize);
            return Ok(result);
        }
    }

}
