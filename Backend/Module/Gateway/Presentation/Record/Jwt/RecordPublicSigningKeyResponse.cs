namespace Gateway.Presentation.Record.Jwt
{
    public sealed record RecordPublicSigningKeyResponse(string Kid, string PublicKeyPem);
}
