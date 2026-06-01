using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [ApiController]
    [Route("api/v1/email-verification")]
    public class EmailVerification : Controller
    {
        private readonly IEmailSender _emailSender;
        public EmailVerification(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] string email)
        {
            return Ok("Verification code sent to your email.");
        }
    }
}
