namespace Contract.Infrastructure.Services.Options
{
    public sealed class PremiseSyncOptions
    {
        public const string SectionName = "ModuleUrls";

        public string Premise { get; set; } = "https://localhost:7081";
        public string ApiKey { get; set; } = string.Empty;
    }
}