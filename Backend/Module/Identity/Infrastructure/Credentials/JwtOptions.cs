namespace Identity.Infrastructure.Credentials
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int ExpirationHours { get; init; } = 12;
        public int KeyRotationHours { get; init; } = 12;
        public int RetiredKeyGraceHours { get; init; } = 1;
        public int Version { get; init; } = 1;
    }
}
