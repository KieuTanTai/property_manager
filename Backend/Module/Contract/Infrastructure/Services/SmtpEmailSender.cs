using Contract.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Contract.Infrastructure.Services.Options;
using Shared.Logging;

namespace Contract.Infrastructure.Services
{
    public sealed class SmtpEmailSender(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailSender> logger,
        ILogPool logPool) : IEmailSender
    {
        private const string Module = "Contract";
        private const string Layer = "Infrastructure/Services";
        private readonly EmailOptions _options = options.Value;

        public async Task<string> SendAsync(
            string senderEmail,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            var (host, port, username, password) = GetEmailConfiguration();
            var security = Enum.TryParse<SecureSocketOptions>(_options.SecureSocketOptions, true, out var configuredSecurity)
                ? configuredSecurity
                : SecureSocketOptions.StartTls;

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(senderEmail));
            message.To.Add(MailboxAddress.Parse(username));
            message.ReplyTo.Add(MailboxAddress.Parse(senderEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(host, port, security, cancellationToken);
                await client.AuthenticateAsync(username, password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
                logger.LogLayerInformation(logPool, Module, Layer, "Consultation email sent successfully.");
                return username;
            }
            catch (Exception exception)
            {
                logger.LogLayerError(logPool, Module, Layer, exception, "Failed to send consultation email.");
                throw;
            }
        }

        #region PRIVATE

        private Tuple<string, int, string, string> GetEmailConfiguration()
        {
            var host = _options.SmtpServer;
            var smtpPort = _options.SmtpPort;
            var smtpUsername = _options.SmtpUsername;
            var smtpPassword = _options.SmtpPassword;
            if (string.IsNullOrWhiteSpace(host)
                || smtpPort is < 1 or > 65535
                || string.IsNullOrWhiteSpace(smtpUsername)
                || string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new InvalidOperationException(
                    "Email SMTP configuration is incomplete. Configure EmailSettings:SmtpServer, EmailSettings:SmtpUsername, and EmailSettings:SmtpPassword for the Contract API.");
            }

            return new Tuple<string, int, string, string>(host, smtpPort, smtpUsername, smtpPassword);
        }

        #endregion
    }
}
