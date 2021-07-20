using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ApiCore.Middleware
{
    public class CultureInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;

        public CultureInfoMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger, ICurrentContextService currentContextService)
        {
            _next = next;
            _logger = logger;
            _currentContextService = currentContextService;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (!string.IsNullOrWhiteSpace(_currentContextService.Language))
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(_currentContextService.Language);

            await _next.Invoke(context);
        }
    }
}
