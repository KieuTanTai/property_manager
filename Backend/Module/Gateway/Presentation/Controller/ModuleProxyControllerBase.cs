using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Presentation.Controller
{
    public abstract class ModuleProxyControllerBase(ModuleProxyService proxyService) : ControllerBase
    {
        protected abstract string ModuleUrlKey { get; }

        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD")]
        [Route("{**path}")]
        public async Task<IActionResult> Forward(CancellationToken cancellationToken)
        {
            await proxyService.ForwardAsync(HttpContext, ModuleUrlKey);
            return new EmptyResult();
        }
    }
}
