namespace Gateway.Services
{
    public interface IIdentitySigningKeyResolver
    {
        Task<string?> GetPublicKeyPemAsync(string keyId, CancellationToken cancellationToken);
    }
}
