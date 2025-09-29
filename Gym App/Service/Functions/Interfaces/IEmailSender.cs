namespace Gym_App.Service.Functions.Interfaces
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string toEmail);
    }
}
