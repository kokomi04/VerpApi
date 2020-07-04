using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public partial class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            var (response, statusCode) = Handler(context.Exception);

            if (!_env.IsProduction())
            {
                response.Data = context.Exception;
            }

            if (context.Exception is BadRequestException)
            {
                context.Result = new BadRequestObjectResult(response);
            }
            else if (context.Exception.GetType() == typeof(VerpException))
            {
                context.Result = new BadRequestObjectResult(response);
            }
            else
            {
                if (context.Exception is DistributedLockExeption)
                {
                    context.Result = new InternalServerErrorObjectResult(response);
                }
                else
                {
                    context.Result = new InternalServerErrorObjectResult(response);
                }
            }

           

            context.HttpContext.Response.StatusCode = (int)statusCode;
            context.ExceptionHandled = true;
        }

        public static (ServiceResult<Exception> response, HttpStatusCode statusCode) Handler(Exception exception)
        {
            ServiceResult<Exception> response;
            HttpStatusCode statusCode;

            if (exception is BadRequestException badRequest)
            {
                response = new ServiceResult<Exception>
                {
                    Code = badRequest.Code,
                    Message = badRequest.Message
                };
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (exception is VerpException)
            {
                response = new ServiceResult<Exception>
                {
                    Code = GeneralCode.InternalError,
                    Message = exception.Message
                };

                statusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                if (exception is DistributedLockExeption)
                {
                    response = new ServiceResult<Exception>
                    {
                        Code = GeneralCode.DistributedLockExeption,
                        Message = GeneralCode.DistributedLockExeption.GetEnumDescription()
                    };

                    statusCode = HttpStatusCode.BadGateway;

                }
                else
                {
                    response = new ServiceResult<Exception>
                    {
                        Code = GeneralCode.InternalError,
                        Message = exception.Message
                    };

                    statusCode = HttpStatusCode.InternalServerError;
                }
            }
            return (response, statusCode);
        }
    }
}
