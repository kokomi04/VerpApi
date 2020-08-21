using GrpcProto.Protos;
using GrpcService.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace GrpcService.Assembly
{
    public static class GrpcServiceAssembly
    {
        public static System.Reflection.Assembly Assembly => typeof(GrpcServiceAssembly).Assembly;

        public static IApplicationBuilder UseEndpointsGrpcService(this IApplicationBuilder app)
        {
            app.UseEndpoints(opt =>{
                opt.MapGrpcService<InternalActivityLogService>();
            });

            return app;
        }

        public static IServiceCollection AddCustomGrpcClient(this IServiceCollection services, Uri address)
        {
            services.AddGrpc(options =>{
                    
            });

            services.AddGrpcClient<InternalActivityLog.InternalActivityLogClient>(opt =>{
                opt.Address = address;
            })
                .EnableCallContextPropagation(opts => opts.SuppressContextNotFoundErrors = true);

            return services;
        }

        
    }
}
