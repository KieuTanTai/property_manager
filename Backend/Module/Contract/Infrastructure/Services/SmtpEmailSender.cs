using Contract.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Contract.Infrastructure.Services
{
    public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            var host = _configuration["Email:SmtpHost"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];
            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình Email__SmtpHost, Email__Username, Email__Password và Email__FromAddress cho Contract API.");
            }

            if (!int.TryParse(_configuration["Email:SmtpPort"], out var port))
            {
                port = 587;
            }

            var securityValue = _configuration["Email:SecureSocketOptions"];
            var security = Enum.TryParse<SecureSocketOptions>(securityValue, true, out var configuredSecurity)
                ? configuredSecurity
                : SecureSocketOptions.StartTls;

            var message = new MimeMessage();
            var fromName = _configuration["Email:FromName"];
            message.From.Add(new MailboxAddress(string.IsNullOrWhiteSpace(fromName) ? "ManagerSpace" : fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, security, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}