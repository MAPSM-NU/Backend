using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities.Users;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Authentication : Controller
    {
        private readonly IUserF _userF;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        public Authentication(IUserF userF , IConfiguration config, IEmailSender emailSender)
        {
            _userF = userF;
            _config = config;
            _emailSender = emailSender;
        }
        [HttpPost]
        [Route("/Sign Up")]
        public async Task<IActionResult> NewUser([FromBody] UserDTO user)
        {
            bool result = await _userF.AddUser(user);

            if (result)
            {
                var token = CreateToken(user);
                //await _emailSender.SendEmailAsync(user.Email);
                return Ok(token);
            }

            else return BadRequest("Failed to Add User");
        }
        [HttpPost]
        [Route("/Sign In")]
        public async Task<IActionResult> LoginUser([FromBody] UserDTO user)
        {
            bool result = await _userF.LoginUser(user);

            if (result)
            {
                var token = CreateToken(user);
                return Ok(token);
            }

            else return BadRequest("Email or Password is wrong");
        }
        [HttpGet]
        [Route("/Get All Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userF.GetAllUsers();
            return Ok(users);
        }
        private string CreateToken(UserDTO u)
        {
            var claims = new List<Claim>
            {
                new Claim("name", u.Name),
                new Claim("email", u.Email),
                new Claim("userId", u.UserID.ToString())
                //new Claim(ClaimTypes.Role,u.Role)
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("JwtSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var TokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("JwtSettings:Issuer"),
                audience: _config.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),//EXPIRATION DATE
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(TokenDescriptor);
        }
    }

}
