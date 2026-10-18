using Contract.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Contract.Infrastructure.Services
{
    public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> SendAsync(
            string senderEmail,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            var (host, port, username, password) = GetEmailConfiguration();
            var securityValue = _configuration["Email:SecureSocketOptions"];
            var security = Enum.TryParse<SecureSocketOptions>(securityValue, true, out var configuredSecurity)
                ? configuredSecurity
                : SecureSocketOptions.StartTls;

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(senderEmail));
            message.To.Add(MailboxAddress.Parse(username));
            message.ReplyTo.Add(MailboxAddress.Parse(senderEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, security, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return username;
        }

        #region PRIVATE

        private Tuple<string, int, string, string> GetEmailConfiguration()
        {
            var settings = _configuration.GetSection("EmailSettings");
            var host = settings["SmtpServer"];
            var port = settings["SmtpPort"];
            var smtpUsername = settings["SmtpUsername"];
            var smtpPassword = settings["SmtpPassword"];
            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(port)
                || string.IsNullOrWhiteSpace(smtpUsername)
                || string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new InvalidOperationException(
                    "Email SMTP configuration is incomplete. Configure EmailSettings:SmtpServer, EmailSettings:SmtpUsername, and EmailSettings:SmtpPassword for the Contract API.");
            }

            if (!int.TryParse(port, out var parsedPort))
            {
                throw new InvalidOperationException(
                    "EmailSettings:SmtpPort must be a valid port number.");
            }
            return new Tuple<string, int, string, string>(host, parsedPort, smtpUsername, smtpPassword);
        }

        #endregion
    }
}
