namespace Gym_App.Application.Interfaces
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string toEmail);
    }
}
