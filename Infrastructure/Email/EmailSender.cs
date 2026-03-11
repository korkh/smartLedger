using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Email
{
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string userEmail, string emailSubject, string msg)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Smtp:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = emailSubject;

            var builder = new BodyBuilder { HtmlBody = msg };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Используем параметры из конфигурации
                await smtp.ConnectAsync(
                    _config["Smtp:Server"],
                    int.Parse(_config["Smtp:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
