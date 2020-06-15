using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore
{
    public static class ServiceCoreAssembly
    {
        public static Assembly Assembly => typeof(ServiceCoreAssembly).Assembly;
        public static IServiceCollection AddServiceCoreDependency(this IServiceCollection services)
        {
            services.AddSingleton<IAsyncRunnerService, AsyncRunnerService>();
            services.AddHttpClient<IActivityLogService, ActivityLogService>();
            services.AddHttpClient<IPhysicalFileService, PhysicalFileService>();
            services.AddHttpClient<IHttpCrossService, HttpCrossService>();
            services.AddHttpClient<IProductHelperService, ProductHelperService>();            
            services.AddScoped<HttpCurrentContextService>();
            services.AddScoped<ICurrentContextFactory, CurrentContextFactory>();
            services.AddScoped(di => di.GetRequiredService<ICurrentContextFactory>().GetCurrentContext());
            return services;
        }
    }
}
