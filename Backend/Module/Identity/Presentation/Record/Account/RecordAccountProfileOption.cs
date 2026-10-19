using Identity.Presentation.Record.Profile;

namespace Identity.Presentation.Record.Account
{
    public record RecordAccountProfileOption(
        Guid AccountId,
        string AccountEmail,
        RecordProfileResponse? UserProfile);
}
