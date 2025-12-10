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
            if(result.Status == 0)
                return BadRequest(new { message = result.msg });
            return 
                Ok(result);
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> NewUser([FromBody] UserCreationDTO user)
        {
            var result = await _userServiceService.SignUpUser(user);
            if (result.Status == 0)
                return BadRequest(new { message = result.msg });
            return
                Ok(result);
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SigninUser([FromQuery]string email,string password)
        {
            var result = await _userServiceService.SigninUser(email,password);
            if (result.Status == 0)
                return BadRequest(new { message = result.msg });
            return
                Ok(result);
        }
        [HttpPost("LoginbyToken")]
        public async Task<IActionResult> LoginByToken([FromQuery] string Refreshtoken)
        {
            var result = await _tokenHandler.ValidateAccessToken(Refreshtoken);
            if (result == null)
                return BadRequest(new { message = "Invalid Token" });
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
