namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IEmailSender
    {
        public Task IntroductionEmail(string toEmail);
        public Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }
}
