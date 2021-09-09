using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ApiCore.Middleware
{
    public class CultureInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CultureInfoMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var language = GetLanguageHeader();
            if (!string.IsNullOrWhiteSpace(language))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(language);
                            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("vi-VN");
            }

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            await _next.Invoke(context);
        }


        private string GetLanguageHeader()
        {

            var languages = new StringValues();
            _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(Headers.Language, out languages);

            if (languages.Count == 0)
            {
                return "";
            }
            return languages.ToString();
        }
    }
}
