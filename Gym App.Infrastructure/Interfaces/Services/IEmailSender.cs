using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IEmailSender
    {
        public Task IntroductionEmail(string toEmail, CancellationToken cancellationToken = default);
        public Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
    }
}
