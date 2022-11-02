using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
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

                    if (context.Response.ContentType != null && context.Response.ContentType.Contains("json") && !context.Request.Path.ToString().Contains("connect/introspect")
                        && (new[] { "POST", "PUT", "DELETE" }.Contains(context.Request.Method) || context.Response.StatusCode != (int)HttpStatusCode.OK)
                        )
                    {
                        memStream.Position = 0;
                        var responseBodyFull = new StreamReader(memStream).ReadToEnd();
                        var responseBodyCut = "";
                        if (responseBodyFull != null && responseBodyFull.Length > 2000)
                        {
                            responseBodyCut = responseBodyFull.Substring(0, 2000);
                        }
                        var log = new
                        {
                            Response = responseBodyCut,
                            XHeaders = context.Request.Headers?.Where(h => h.Key?.ToLower()?.StartsWith('x') == true),
                            ResponseCode = context.Response.StatusCode.ToString(),
                            IsSuccessStatusCode = (context.Response.StatusCode == 200 || context.Response.StatusCode == 201),
                            Status = context.Response.StatusCode
                        };
                        _logger.LogInformation("ResponseContent: " + log.JsonSerialize());

                        //if (context.RequestAborted.IsCancellationRequested)
                        //{
                        LongTaskResourceLockFactory.SetResponse(context.TraceIdentifier, context.Response.StatusCode, responseBodyFull);
                        //}
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
