using FluentEmail.Core;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IFluentEmail fluentEmail,
            ILogger<EmailSender> logger)
        {
            _fluentEmail = fluentEmail;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                _logger.LogInformation($"Sending email to: {toEmail}");

                var response = await _fluentEmail
                    .To(toEmail)
                    .Subject(subject)
                    .Body(body, true)  // true = isHtml
                    .SendAsync();

                if (response.Successful)
                {
                    _logger.LogInformation($"Email sent successfully to: {toEmail} with content {body}");
                }
                else
                {
                    _logger.LogError($"Email failed: {string.Join(", ", response.ErrorMessages)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email exception: {ex.Message}");
                throw;
            }
        }
        public Task IntroductionEmail(string toEmail)
        {
            return SendEmailAsync(toEmail, "Welcome to Gym App!", GetWelcomeEmailBody(), true);
        }
        private string GetWelcomeEmailBody()
        {
            return """
                <html>
                    <body style="font-family: Arial; line-height: 1.6; color: #333;">
                        <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                            <h1 style="color: #007bff;">Welcome to Fitpack!</h1>
                            <p>Hello,</p>
                            <p>Thank you for registering with our fitness application.</p>
                            <p>You can now:</p>
                            <ul>
                                <li>Track your workouts</li>
                                <li>Monitor your progress</li>
                                <li>Connect with other users</li>
                            </ul>
                            <p>
                                <a href="https://www.youtube.com/watch?v=M5ofvOP1Ybc" 
                                   style="display: inline-block; padding: 10px 20px; 
                                          background-color: #007bff; color: white; 
                                          text-decoration: none; border-radius: 5px;">
                                    Go to Dashboard
                                </a>
                            </p>
                            <p>Best regards,<br>The Fitpack Team</p>
                        </div>
                    </body>
                </html>
                """;
        }
    }
}
