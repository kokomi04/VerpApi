using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore
{
    public static class ServiceCoreAssembly
    {
        public static Assembly Assembly => typeof(ServiceCoreAssembly).Assembly;
        public static IServiceCollection AddServiceCoreDependency(this IServiceCollection services)
        {
            services.AddSingleton<IAsyncRunnerService, AsyncRunnerService>();
            services.AddScoped<HttpCurrentContextService>();
            services.AddScoped<ICurrentContextFactory, CurrentContextFactory>();
            services.AddScoped(di => di.GetRequiredService<ICurrentContextFactory>().GetCurrentContext());
            return services;
        }
    }
}
