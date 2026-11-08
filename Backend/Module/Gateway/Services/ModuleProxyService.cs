using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Gateway.Services
{
    public sealed class ModuleProxyService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "TE",
            "Trailer", "Transfer-Encoding", "Upgrade", "Host", "Content-Length"
        };

        public async Task ForwardAsync(HttpContext context, string moduleUrlKey)
        {
            var moduleUrl = configuration[moduleUrlKey];
            if (string.IsNullOrWhiteSpace(moduleUrl))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Module đích chưa được cấu hình.", context.RequestAborted);
                return;
            }

            var targetUri = BuildTargetUri(context, moduleUrl);
            using var request = CreateProxyRequest(context, targetUri);
            using var response = await SendRequestAsync(request, context.RequestAborted);
            await WriteResponseAsync(context, response, context.RequestAborted);
        }

        private static Uri BuildTargetUri(HttpContext context, string moduleUrl)
        {
            // Keep the incoming API path when forwarding (for example /api/contact -> /api/contact).
            var path = context.Request.Path.Value ?? string.Empty;
            return new UriBuilder(moduleUrl)
            {
                Path = $"{new Uri(moduleUrl).AbsolutePath.TrimEnd('/')}{path}",
                Query = context.Request.QueryString.Value?.TrimStart('?') ?? string.Empty
            }.Uri;
        }

        private HttpRequestMessage CreateProxyRequest(HttpContext context, Uri targetUri)
        {
            var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
            var httpRequestBodyDetectionFeature = context.Features.Get<IHttpRequestBodyDetectionFeature>();
            if (httpRequestBodyDetectionFeature?.CanHaveBody == true)
            {
                request.Content = new StreamContent(context.Request.Body);
            }

            CopyRequestHeaders(context.Request.Headers, request);
            return request;
        }

        private static void CopyRequestHeaders(IHeaderDictionary sourceHeaders, HttpRequestMessage request)
        {
            foreach (var header in sourceHeaders)
            {
                if (HopByHopHeaders.Contains(header.Key))
                {
                    continue;
                }

                if (!request.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]))
                {
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]);
                }
            }
        }

        private Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            httpClientFactory.CreateClient("ModuleProxy")
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        private static async Task WriteResponseAsync(HttpContext context, HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            context.Response.StatusCode = (int)response.StatusCode;
            CopyResponseHeaders(response.Headers, context.Response.Headers);
            CopyResponseHeaders(response.Content.Headers, context.Response.Headers);

            await response.Content.CopyToAsync(context.Response.Body, cancellationToken);
        }

        private static void CopyResponseHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> sourceHeaders,
            IHeaderDictionary destinationHeaders)
        {
            foreach (var header in sourceHeaders)
            {
                if (!HopByHopHeaders.Contains(header.Key))
                {
                    destinationHeaders[header.Key] = new StringValues([.. header.Value]);
                }
            }
        }
    }
}
