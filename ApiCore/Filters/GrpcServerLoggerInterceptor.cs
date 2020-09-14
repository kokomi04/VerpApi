using Dia2Lib;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class GrpcServerLoggerInterceptor: Interceptor
    {
        private readonly ILogger<GrpcServerLoggerInterceptor> _logger;

        public GrpcServerLoggerInterceptor(ILogger<GrpcServerLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.Unary, request, context);
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error thrown by {context.Method}.");
                throw;
            }
        }

        private void LogCall<TRequest, TResponse>(MethodType methodType, TRequest request, ServerCallContext context)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogWarning($"Starting Grpc call. Type: {methodType}. RequestPath: {context.Method}. RequestBody: {request}");
        }
    }
}
