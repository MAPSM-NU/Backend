using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Entities.Users;
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
    [ApiController]
    [Route("[controller]")]
    public class Authentication : Controller
    {
        private readonly IUserServise _userF;
        private readonly IEmailSender _emailSender;
        private readonly ITokenHandler _tokenHandler;
        public Authentication(IUserServise userF , IEmailSender emailSender,ITokenHandler tokenHandler)
        {
            _userF = userF;
            _emailSender = emailSender;
            _tokenHandler = tokenHandler;
        }
        [HttpPost]
        [Route("/Sign Up")]
        public async Task<IActionResult> NewUser([FromBody] UserDTO user)
        {
            var result = await _userF.SignUpUser(user);
            if (result.Status == 0) return BadRequest("Missing Credentials");
            else if (result.Status == 1) return BadRequest("Name is already in use");
            else if (result.Status == 2) return BadRequest("Email is already in use");
            else if (result.Status == 3) return BadRequest("Email is not valid");
            else if (result.Status == 4) return BadRequest("Password is not valid");
            else if (result.Status == 5)
            {
                
                return Ok(result);
            }

            else return BadRequest("Failed to Add User");
        }
        [HttpPost]
        [Route("/Sign In")]
        public async Task<IActionResult> LoginUser([FromBody] UserDTO user)
        {
            var result = await _userF.LoginUser(user);
            if(result.Status == 0)  return BadRequest("Email not found");
            else if (result.Status == 1) return BadRequest("Password is wrong");
            else if (result.Status == 2)
            {
                return Ok(result);
            }
            return BadRequest("Failed to Login");
        }
        [HttpPost]
        [Route("/Login by Token")]
        public async Task<IActionResult> LoginByToken([FromBody] string Refreshtoken)
        {
            var result = await _tokenHandler.ValidateAccessToken(Refreshtoken);
            if (result == null) return BadRequest("Invalid Token");
            else return Ok(result);
        }
        [HttpGet]
        [Route("/Get All Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userF.GetAllUsers();
            return Ok(users);
        }
        [HttpDelete]
        [Route("/Delete User")]
        public async Task<IActionResult> DeleteUser([FromBody] Guid UserID)
        {
            var result = await _userF.DeleteUser(UserID);
            if (result) return Ok("User Deleted Successfully");
            return BadRequest("Failed to Delete User");
        }
        [HttpGet]
        [Route("/Get Tokens")]
        public async Task<IActionResult> GetTokens()
        {
            var result = await _tokenHandler.GetAllRefreshTokens();
            return Ok(result);
        }
    }

}
