﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public partial class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(IHostingEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            if (context.Exception.GetType() == typeof(VerpException))
            {
                var json = new ApiResponse
                {
                    Code = GeneralCode.InternalError.GetErrorCodeString(),
                    Message = context.Exception.Message
                };

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                if (context.Exception.GetType() == typeof(DistributedLockExeption))
                {
                    var json = new ApiResponse<Exception>
                    {
                        Code = GeneralCode.DistributedLockExeption.GetErrorCodeString(),
                        Message = GeneralCode.DistributedLockExeption.GetEnumDescription()
                    };

                    if (_env.IsDevelopment())
                    {
                        json.Data = context.Exception;
                    }

                    context.Result = new InternalServerErrorObjectResult(json);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                }
                else
                {
                    var json = new ApiResponse<Exception>
                    {
                        Code = GeneralCode.InternalError.GetErrorCodeString(),
                        Message = context.Exception.Message
                    };

                    if (_env.IsDevelopment())
                    {
                        json.Data = context.Exception;
                    }

                    context.Result = new InternalServerErrorObjectResult(json);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            context.ExceptionHandled = true;
        }
    }
}
