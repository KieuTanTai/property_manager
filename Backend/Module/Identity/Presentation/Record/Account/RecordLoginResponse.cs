using Identity.Presentation.Record.Profile;
using Shared.Persistence.Record.Auth;

namespace Identity.Presentation.Record.Account
{
    public record RecordLoginResponse(
        RecordAuthResponse Account,
        RecordProfileResponse? Profile,
        List<RecordClaimResponse> Claims);
}