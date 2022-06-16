using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;

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

        private static CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");
        public async Task InvokeAsync(HttpContext context)
        {

            var language = GetLanguageHeader();
            CultureInfo culture;
            if (!string.IsNullOrWhiteSpace(language))
            {
                culture = (CultureInfo)new CultureInfo(language, true).Clone();
            }
            else
            {
                culture = (CultureInfo)new CultureInfo("vi-VN", true).Clone();
                // culture = CultureInfo.GetCultureInfo("vi-VN");
            }


            culture.DateTimeFormat = usCulture.DateTimeFormat;
            culture.NumberFormat = usCulture.NumberFormat;

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;

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
