namespace Shared.Persistence.Record.Auth
{
    public sealed record RecordJwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string SigningKey { get; init; } = string.Empty;
        public int ExpirationHours { get; init; } = 12;
        public int Version { get; init; } = 1;
    }
}