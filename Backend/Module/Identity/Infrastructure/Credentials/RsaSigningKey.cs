using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Credentials
{
    public sealed class RsaSigningKey : IDisposable
    {
        public RsaSigningKey(DateTimeOffset createdAt, TimeSpan signingLifetime, TimeSpan tokenLifetime, TimeSpan verificationGrace)
        {
            Rsa = RSA.Create(3072);
            KeyId = Guid.NewGuid().ToString("N");
            CreatedAt = createdAt;
            SigningEndsAt = createdAt.Add(signingLifetime);
            VerificationEndsAt = SigningEndsAt.Add(tokenLifetime).Add(verificationGrace);
            SecurityKey = new RsaSecurityKey(Rsa) { KeyId = KeyId };
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.RsaSha256);
        }

        public string KeyId { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset SigningEndsAt { get; }
        public DateTimeOffset VerificationEndsAt { get; }
        public RSA Rsa { get; }
        public RsaSecurityKey SecurityKey { get; }
        public SigningCredentials SigningCredentials { get; }

        public void Dispose() => Rsa.Dispose();
    }
}
