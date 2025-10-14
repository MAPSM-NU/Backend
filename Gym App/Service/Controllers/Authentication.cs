using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class Authentication : Controller
    {
        private readonly IUserServise _user;
        private readonly IEmailSender _emailSender;
        private readonly ITokenHandler _tokenHandler;
        public Authentication(IUserServise userF , IEmailSender emailSender,ITokenHandler tokenHandler)
        {
            _user = userF;
            _emailSender = emailSender;
            _tokenHandler = tokenHandler;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> NewUser([FromBody] UserDTO user)
        {
            var result = await _user.SignUpUser(user);
            if (result.Status == 5)return Ok(result);
            else if (result.Status == 4) return BadRequest("Password is not valid");
            else if (result.Status == 3) return BadRequest("Email is not valid");
            else if (result.Status == 2) return BadRequest("Email is already in use");
            else if (result.Status == 1) return BadRequest("Name is already in use");
            else if (result.Status == 0) return BadRequest("Missing Credentials");
            else return BadRequest("Failed to Add User");
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> LoginUser([FromBody] UserDTO user)
        {
            var result = await _user.LoginUser(user);
            if(result.Status == 0)  return BadRequest("Email not found");
            else if (result.Status == 1) return BadRequest("Password is wrong");
            else if (result.Status == 2) return Ok(result);
            else return BadRequest("Failed to Login");
        }
        [HttpPost("LoginbyToken")]
        public async Task<IActionResult> LoginByToken([FromBody] string Refreshtoken)
        {
            var result = await _tokenHandler.ValidateAccessToken(Refreshtoken);
            if (result == null) return BadRequest("Invalid Token");
            else return Ok(result);
        }
        [HttpGet("GetTokens")]
        public async Task<IActionResult> GetTokens()
        {
            var result = await _tokenHandler.GetAllRefreshTokens();
            return Ok(result);
        }
    }

}
