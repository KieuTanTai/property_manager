namespace Contract.Infrastructure.Services.Options
{
    public sealed class EmailOptions
    {
        public const string SectionName = "EmailSettings";

        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SecureSocketOptions { get; set; } = "StartTls";
    }
}