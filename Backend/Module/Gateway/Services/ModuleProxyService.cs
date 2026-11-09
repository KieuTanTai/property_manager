using System.Net.WebSockets;
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

        private static string MapModulePath(string path, string moduleUrlKey)
        {
            if (!string.Equals(
                    moduleUrlKey,
                    "ModuleUrls:TicketAndNotification",
                    StringComparison.Ordinal))
            {
                return path;
            }

            var routeMappings = new (string GatewayPath, string ModulePath)[]
            {
                ("/api/notifications", "/api/Notification"),
                ("/api/notification-recipients", "/api/NotificationRecipient")
            };

            foreach (var (gatewayPath, modulePath) in routeMappings)
            {
                if (path.Equals(gatewayPath, StringComparison.OrdinalIgnoreCase))
                {
                    return modulePath;
                }

                if (path.StartsWith($"{gatewayPath}/", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{modulePath}{path[gatewayPath.Length..]}";
                }
            }

            return path;
        }

        public async Task ForwardWebSocketAsync(HttpContext context, string moduleUrlKey)
        {
            var moduleUrl = configuration[moduleUrlKey];
            if (string.IsNullOrWhiteSpace(moduleUrl))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Module đích chưa được cấu hình.", context.RequestAborted);
                return;
            }

            var targetUriBuilder = new UriBuilder(moduleUrl)
            {
                Scheme = new Uri(moduleUrl).Scheme switch
                {
                    "https" => "wss",
                    "http" => "ws",
                    var scheme => scheme
                },
                Path = $"{new Uri(moduleUrl).AbsolutePath.TrimEnd('/')}{context.Request.Path.Value}",
                Query = context.Request.QueryString.Value?.TrimStart('?') ?? string.Empty
            };

            using var upstream = new ClientWebSocket();
            if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                upstream.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            }

            foreach (var header in context.Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                    || header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)
                    || header.Key.Equals("Upgrade", StringComparison.OrdinalIgnoreCase)
                    || header.Key.StartsWith("Sec-WebSocket-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                upstream.Options.SetRequestHeader(header.Key, header.Value.ToString());
            }

            await upstream.ConnectAsync(targetUriBuilder.Uri, context.RequestAborted);
            using var downstream = await context.WebSockets.AcceptWebSocketAsync();
            using var relayCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            var downstreamToUpstream = RelayWebSocketAsync(downstream, upstream, relayCancellation.Token);
            var upstreamToDownstream = RelayWebSocketAsync(upstream, downstream, relayCancellation.Token);
            await Task.WhenAny(downstreamToUpstream, upstreamToDownstream);
            relayCancellation.Cancel();
            try
            {
                await Task.WhenAll(downstreamToUpstream, upstreamToDownstream);
            }
            catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested)
            {
            }
        }

        private static async Task RelayWebSocketAsync(
            WebSocket source,
            WebSocket destination,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];
            while (source.State == WebSocketState.Open)
            {
                var result = await source.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (destination.State == WebSocketState.Open)
                    {
                        await destination.CloseOutputAsync(
                            result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                            result.CloseStatusDescription,
                            cancellationToken);
                    }

                    return;
                }

                await destination.SendAsync(
                    buffer.AsMemory(0, result.Count),
                    result.MessageType,
                    result.EndOfMessage,
                    cancellationToken);
            }
        }
    }
}
