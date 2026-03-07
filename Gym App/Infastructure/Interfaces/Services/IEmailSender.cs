namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string toEmail);
    }
}
