using Identity.Presentation.Record.Profile;
using Shared.Persistence.Record.Auth;

namespace Identity.Application.Record
{
    public record RecordLoginResponse(
        RecordAuthResponse Account,
        RecordProfileResponse? Profile,
        string AccessToken);
}