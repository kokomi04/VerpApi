using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class InternalCrossAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InternalCrossAuthorizeAttribute(
           IOptionsSnapshot<AppSetting> appSetting
           , ILogger<InternalCrossAuthorizeAttribute> logger
            , IHttpContextAccessor httpContextAccessor
       )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.Request.Headers.TryGetValue(Headers.CrossServiceKey, out var crossServiceKeys);
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
