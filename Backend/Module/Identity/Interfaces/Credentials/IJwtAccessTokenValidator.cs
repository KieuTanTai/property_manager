namespace Identity.Interfaces.Credentials
{
    public interface IJwtAccessTokenValidator
    {
        bool IsValid(string accessToken);
    }
}
