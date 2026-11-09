using System.Net;

namespace Gateway.Interfaces.Authorization
{
    public interface IIdentityTokenAuthorizationService
    {
        Task<HttpStatusCode?> AuthorizeAsync(string accessToken, CancellationToken cancellationToken);
    }
}
