using Shared.Logging;

namespace Gateway.Middleware
{
    public sealed class RequestDeadlineMiddleware(
        RequestDelegate next,
        ILogger<RequestDeadlineMiddleware> logger,
        ILogPool logPool)
    {
        private const string Module = "Gateway";
        private const string Layer = "Middleware";
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(60);

        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestDeadlineMiddleware> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public async Task InvokeAsync(HttpContext context)
        {
            var clientCancellation = context.RequestAborted;
            using var requestDeadline = CancellationTokenSource.CreateLinkedTokenSource(clientCancellation);
            requestDeadline.CancelAfter(RequestTimeout);
            context.RequestAborted = requestDeadline.Token;

            try
            {
                await _next(context);
            }
            catch (OperationCanceledException exception) when (
                requestDeadline.IsCancellationRequested && !clientCancellation.IsCancellationRequested)
            {
                var timeoutException = new TimeoutException(
                    $"Gateway request exceeded the {RequestTimeout.TotalSeconds:0}-second deadline.", exception);
                _logger.LogLayerError(_logPool, Module, Layer, timeoutException,
                    "Gateway request deadline expired.");
                throw timeoutException;
            }
            finally
            {
                context.RequestAborted = clientCancellation;
                await _logPool.FlushAsync(Module, Layer, clientCancellation);
            }
        }
    }
}
