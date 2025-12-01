using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.Context;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailVerification : Controller
    {
        private readonly DbBase _db;
        private readonly IEmailSender _emailSender;
        public EmailVerification(DbBase db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        [HttpPost("EmailVerification")]
        public async Task<IActionResult> VerifyEmail([FromBody] string email)
        {
            //var user = await _db.Users.FindAsync(email);
            //if (user == null) return BadRequest("Email not found");
            await _emailSender.SendEmailAsync(email);
            return Ok("Verification code sent to your email.");
        }
    }
}
