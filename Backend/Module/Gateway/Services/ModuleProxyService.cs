using System.Net;
using Microsoft.Extensions.Primitives;

namespace Gateway.Services
{
    public sealed class ModuleProxyService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "TE",
            "Trailer", "Transfer-Encoding", "Upgrade", "Host", "Content-Length",
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

            // Module controllers expose their endpoints under /api, so preserve the
            // incoming path when forwarding (for example /api/contact -> /api/contact).
            var path = context.Request.Path.Value ?? string.Empty;

            var targetUri = new UriBuilder(moduleUrl)
            {
                Path = $"{new Uri(moduleUrl).AbsolutePath.TrimEnd('/')}{path}",
                Query = context.Request.QueryString.Value?.TrimStart('?') ?? string.Empty,
            }.Uri;

            using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
            if (context.Request.ContentLength is > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
                request.Content = new StreamContent(context.Request.Body);

            foreach (var header in context.Request.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key)) continue;
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            using var response = await httpClientFactory.CreateClient("ModuleProxy")
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
            {
                if (!HopByHopHeaders.Contains(header.Key))
                    context.Response.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }
            foreach (var header in response.Content.Headers)
            {
                if (!HopByHopHeaders.Contains(header.Key))
                    context.Response.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }

            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
    }
}
