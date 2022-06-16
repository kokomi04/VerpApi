using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class InternalCrossAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextFactory _currentContextFactory;
        private readonly ICurrentContextService _currentContextService;

        public InternalCrossAuthorizeAttribute(
           IOptionsSnapshot<AppSetting> appSetting
           , ILogger<InternalCrossAuthorizeAttribute> logger
           , ICurrentContextFactory currentContextFactory
           , ICurrentContextService currentContextService
       )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
            _currentContextFactory = currentContextFactory;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;
            headers.TryGetValue(Headers.CrossServiceKey, out var crossServiceKeys);
            if (crossServiceKeys.ToString() != _appSetting?.Configuration?.InternalCrossServiceKey)
            {
                _logger.LogError("InternalCrossAuthorizeAttribute: " + crossServiceKeys);
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }


            return base.OnActionExecutionAsync(context, next);
        }
    }
}
