using Identity.Infrastructure.Credentials;

namespace Identity.Interfaces.Credentials
{
    public interface IRsaSigningKeyProvider
    {
        RsaSigningKey GetCurrentSigningKey();
        string? GetPublicKeyPem(string keyId);
    }
}
