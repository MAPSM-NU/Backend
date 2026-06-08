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

        public Task SendPasswordResetEmail(string toEmail, string otpCode)
        {
            string body = $@"
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                        body {{ background-color: #e9eff5; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; line-height: 1.5; padding: 20px; }}
                        .container {{ max-width: 500px; margin: 0 auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #91c48e 0%, #587756 100%); padding: 24px; text-align: center; color: white; }}
                        .header h1 {{ font-size: 22px; font-weight: 600; margin: 0; }}
                        .header p {{ font-size: 13px; opacity: 0.9; margin: 4px 0 0 0; }}
                        .body {{ padding: 24px; }}
                        .greeting {{ font-size: 16px; font-weight: 500; color: #000; margin-bottom: 12px; }}
                        .message {{ color: #333; font-size: 14px; margin-bottom: 16px; line-height: 1.6; }}
                        .otp-box {{ background: #f8fafc; border: 1px solid #e2edf7; border-radius: 12px; padding: 16px; text-align: center; margin: 16px 0; }}
                        .otp-label {{ font-size: 11px; text-transform: uppercase; letter-spacing: 1px; font-weight: 600; color: #3b82f6; margin-bottom: 8px; }}
                        .otp-code {{ font-size: 40px; font-weight: 800; letter-spacing: 8px; font-family: 'Courier New', monospace; color: #0f172a; }}
                        .notice {{ background: #fffbeb; border-left: 4px solid #f59e0b; padding: 12px 14px; border-radius: 8px; font-size: 13px; color: #92400e; margin: 12px 0; }}
                        .security {{ background: #f0f9ff; border-left: 4px solid #3b82f6; padding: 12px 14px; border-radius: 8px; font-size: 13px; color: #075985; margin: 12px 0; }}
                        .footer {{ border-top: 1px solid #e9eef3; padding-top: 16px; text-align: center; color: #6c757d; font-size: 11px; line-height: 1.5; }}
                        .footer a {{ color: #3b82f6; text-decoration: none; }}
                        @media(max-width: 480px) {{ .otp-code {{ font-size: 32px; letter-spacing: 6px; }} .header h1 {{ font-size: 18px; }} }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Password Reset</h1>
                            <p>Secure account recovery</p>
                        </div>
                        <div class=""body"">
                            <div class=""greeting"">Hello,</div>
                            <div class=""message"">
                                We received a request to reset your password. Use the code below to complete the process. <strong>Never share this code</strong> with anyone.
                            </div>

                            <div class=""otp-box"">
                                <div class=""otp-label"">Verification Code</div>
                                <div class=""otp-code"">{otpCode}</div>
                            </div>

                            <div class=""notice"">
                                ⏳ <strong>Expires in 15 minutes</strong> from when this email was sent.
                            </div>

                            <div class=""security"">
                                🛡️ <strong>Didn't request this?</strong> Ignore this email.
                            </div>

                            <div class=""footer"">
                                <p>This is an automated message. Please do not reply.</p>
                                <p style=""margin-top: 8px;"">&copy; 2025 Fitpack. All rights reserved.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
            return SendEmailAsync(toEmail, "Reset Your Fitpack Password", body, true);
        }
    }
}
