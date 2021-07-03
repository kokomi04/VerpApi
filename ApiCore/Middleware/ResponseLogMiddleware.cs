using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    public class ResponseLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ResponseLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentType?.Contains("grpc") == true)
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;

            try
            {
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await _next(context);

                    if (context.Response.ContentType != null && context.Response.ContentType.Contains("json") && !context.Request.Path.ToString().Contains("connect/introspect") && new[] { "POST", "PUT", "DELETE" }.Contains(context.Request.Method))
                    {
                        memStream.Position = 0;
                        var responseBody = new StreamReader(memStream).ReadToEnd();
                        if (responseBody != null && responseBody.Length > 2000)
                        {
                            responseBody = responseBody.Substring(0, 2000);
                        }
                        var log = new
                        {
                            Response = responseBody,
                            ResponseCode = context.Response.StatusCode.ToString(),
                            IsSuccessStatusCode = (context.Response.StatusCode == 200 || context.Response.StatusCode == 201),
                            Status = context.Response.StatusCode
                        };
                        _logger.LogInformation("ResponseContent: " + log.JsonSerialize());
                    }

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }

            }
            finally
            {
                context.Response.Body = originalBody;
            }

        }


    }
}
