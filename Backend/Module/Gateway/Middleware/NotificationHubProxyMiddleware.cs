using Gateway.Services;

namespace Gateway.Middleware
{
    public sealed class NotificationHubProxyMiddleware(RequestDelegate next)
    {
        private const string HubPath = "/hubs/notifications";
        private const string ModuleUrlKey = "ModuleUrls:TicketAndNotification";

        public async Task InvokeAsync(HttpContext context, ModuleProxyService proxyService)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (!path.Equals(HubPath, StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith($"{HubPath}/", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                await proxyService.ForwardWebSocketAsync(context, ModuleUrlKey);
                return;
            }

            await proxyService.ForwardAsync(context, ModuleUrlKey);
        }
    }
}
