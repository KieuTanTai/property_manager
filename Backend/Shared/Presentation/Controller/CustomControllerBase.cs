using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Shared.Logging;

namespace Shared.Presentation.Controller
{
    public abstract class CustomControllerBase(
        ILogger logger,
        ILogPool logPool,
        string module,
        string layer)
        : ControllerBase, IAsyncActionFilter
    {
        private readonly ILogPool _logPool = logPool;

        private readonly ILogger _logger = logger;

        protected string Module { get; } = module;
        protected string Layer { get; } = layer;

        [NonAction]
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var actionName = context.ActionDescriptor.DisplayName ?? "Unknown action";

            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Request started for {actionName}.");

            try
            {
                var executedContext = await next();
                if (executedContext is { Exception: {} exception, ExceptionHandled: false })
                {
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        $"Request failed for {actionName}.");
                    return;
                }

                var statusCode = executedContext.Result is IStatusCodeActionResult { StatusCode: not null } statusResult
                    ? statusResult.StatusCode.Value
                    : context.HttpContext.Response.StatusCode;
                if (statusCode >= StatusCodes.Status400BadRequest)
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        $"Request completed for {actionName} with HTTP {statusCode}.");
                }
                else
                {
                    _logger.LogLayerInformation(_logPool, Module, Layer,
                        $"Request completed for {actionName} with HTTP {statusCode}.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    $"Request failed for {actionName}.");
                throw;
            }
            finally
            {
                await _logPool.FlushAsync(Module, Layer, CancellationToken.None);
            }
        }
    }
}