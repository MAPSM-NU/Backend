using Gym_App.Service.Functions.Interfaces;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Crypto.Macs;

namespace Gym_App.Service.Functions.The_Applied
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string toEmail)
        {
            var msg = new MimeKit.MimeMessage();
            var from = new MimeKit.MailboxAddress("Gym App", "Modymansour2004@gmail.com");
            var to = new MimeKit.MailboxAddress("", toEmail);
            msg.To.Add(to);
            msg.From.Add(from);
            msg.Subject = "Hello and welcome";
            msg.Body = new TextPart(TextFormat.Plain){
                Text = """
                Hello and welcome again
                """
            };
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate("Modymansour2004@gmail.com", "Mody_1152004");
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
