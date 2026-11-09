using Identity.Infrastructure.Credentials.Models;

namespace Identity.Interfaces.Credentials
{
    public interface IRsaSigningKeyProvider
    {
        RsaSigningKey GetCurrentSigningKey();
        string? GetPublicKeyPem(string keyId);
    }
}