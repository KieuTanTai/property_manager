using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Logging;

namespace Identity.Presentation.Controller;

public abstract class CustomControllerBase(ILogPool logPool, string module, string layer) : ControllerBase, IAsyncActionFilter
{
    private readonly string _module = module;
    private readonly string _layer = layer;

    [NonAction]
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        try
        {
            await next();
        }
        finally
        {
            await logPool.FlushAsync(_module, _layer, CancellationToken.None);
        }
    }
}
