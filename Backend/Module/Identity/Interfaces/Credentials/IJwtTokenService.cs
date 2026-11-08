using Shared.Persistence.Record.Auth;

namespace Identity.Interfaces.Credentials
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(RecordAuthResponse account);
    }
}