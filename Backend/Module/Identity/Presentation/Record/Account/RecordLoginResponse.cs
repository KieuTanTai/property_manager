using Identity.Presentation.Record.Profile;
using Shared.Persistence.Record.Auth;

namespace Identity.Presentation.Record.Account
{
    public record RecordClaimResponse(string Type, string Value);

    public record RecordLoginResponse(
        RecordAuthResponse Account,
        RecordProfileResponse? Profile,
        List<RecordClaimResponse> Claims);
}