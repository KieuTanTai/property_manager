namespace Shared.ModelHelper
{
    public sealed class IdentityRulesModel
    {
        public int MaxPasswordLength { get; init; }
        public int MinPasswordLength { get; init; }
        public bool RequireUppercase { get; init; }
        public bool RequireLowercase { get; init; }
        public bool RequireDigit { get; init; }
        public bool RequiredSpecialCharacter { get; init; }
        public string RegexForEmail { get; init; } = "";
        public string RegexForPhoneNumber { get; init; } = "";
    }
}