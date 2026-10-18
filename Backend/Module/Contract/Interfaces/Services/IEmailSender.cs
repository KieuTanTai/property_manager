namespace Contract.Interfaces.Services
{
    public interface IEmailSender
    {
        Task<string> SendAsync(
            string senderEmail,
            string subject,
            string body,
            CancellationToken cancellationToken = default);
    }
}
