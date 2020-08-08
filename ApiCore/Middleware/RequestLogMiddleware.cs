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
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.ToString().Contains("connect/introspect") && new[] { "POST", "PUT", "DELETE" }.Contains(context.Request.Method))
            {
                context.Request.EnableBuffering();

                var body = string.Empty;
                if (context.Request.HasFormContentType)
                {
                    var value = new NonCamelCaseDictionary();
                    value.Add("Form",
                        context.Request.Form
                        .GroupBy(f => f.Key)
                        .ToNonCamelCaseDictionaryData(f => f.Key, f => new StringValues(f.SelectMany(v => v.Value.ToArray()).ToArray()))
                        .JsonSerialize()
                        );
                    value.Add("Files", context.Request.Form.Files.Select(f => new { f.FileName, f.ContentType, f.Length, f.ContentDisposition }).JsonSerialize());
                    body = value.JsonSerialize();
                }
                else
                {
                    body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                }
                context.Request.Body.Position = 0;

                var log = new
                {
                    context.Request.Path,
                    context.Request.Method,
                    QueryString = context.Request.QueryString.ToString(),
                    Payload = body
                };
                _logger.LogInformation("RequestContent: " + log.JsonSerialize());
            }

            await _next.Invoke(context);
        }
    }
}
