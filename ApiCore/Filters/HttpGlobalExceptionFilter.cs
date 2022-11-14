using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public partial class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;
        private readonly AppSetting _appSetting;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env
            , ILogger<HttpGlobalExceptionFilter> logger
            , IOptions<AppSetting> appSetting)
        {
            _env = env;
            _logger = logger;
            _appSetting = appSetting.Value;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is BadRequestException)
            {

                _logger.LogWarning(context.Exception, context.Exception.Message);
            }
            else
            {
                _logger.LogError(context.Exception, context.Exception.Message);
            }

            var (response, statusCode) = Handler(context.Exception, _appSetting);

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

        public static string RemoveAbsolutePathResource(AppSetting appSetting, string message)
        {
            var absolutePath = Path.GetFullPath(appSetting.Configuration.FileUploadFolder);
            return message.Replace(absolutePath, string.Empty);
        }

        public static (ApiErrorResponse<Exception> response, HttpStatusCode statusCode) Handler(Exception exception, AppSetting appSetting)
        {
            ApiErrorResponse<Exception> response;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case BadRequestException badRequest:
                    response = new ApiErrorResponse<Exception>
                    {
                        Code = badRequest.Code.GetErrorCodeString(),
                        Message = RemoveAbsolutePathResource(appSetting, badRequest.Message)
                    };
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case VerpException:

                    response = new ApiErrorResponse<Exception>
                    {
                        Code = GeneralCode.InternalError.GetErrorCodeString(),
                        Message = RemoveAbsolutePathResource(appSetting, exception.Message)
                    };

                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case LongTaskResourceLockException:

                    response = new ApiErrorResponse<Exception>
                    {
                        Code = GeneralCode.LongTaskIsRunning.GetErrorCodeString(),
                        Message = GeneralCode.LongTaskIsRunning.GetEnumDescription()
                    };

                    statusCode = HttpStatusCode.BadGateway;
                    break;

                case DistributedLockExeption:
                    response = new ApiErrorResponse<Exception>
                    {
                        Code = GeneralCode.DistributedLockExeption.GetErrorCodeString(),
                        Message = GeneralCode.DistributedLockExeption.GetEnumDescription()
                    };

                    statusCode = HttpStatusCode.BadGateway;
                    break;

                default:
                    response = new ApiErrorResponse<Exception>
                    {
                        Code = GeneralCode.InternalError.GetErrorCodeString(),
                        Message = RemoveAbsolutePathResource(appSetting, exception.Message)
                    };

                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            return (response, statusCode);
        }
    }
}
