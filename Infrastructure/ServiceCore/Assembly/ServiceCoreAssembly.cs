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
            services.AddHttpClient<IHttpCrossService, HttpCrossService>();

            services.AddSingleton<IAsyncRunnerService, AsyncRunnerService>();

            services.AddScoped<IActivityLogService, ActivityLogService>();
            services.AddScoped<IPhysicalFileService, PhysicalFileService>();
            


            services.AddScoped<IStockHelperService, StockHelperService>();
            services.AddScoped<IProductHelperService, ProductHelperService>();
            services.AddScoped<IOrganizationHelperService, OrganizationHelperService>();
            services.AddScoped<IInputTypeHelperService, InputTypeHelperService>();
            services.AddScoped<IVoucherTypeHelperService, VoucherTypeHelperService>();
            services.AddScoped<IRoleHelperService, RoleHelperService>();
            services.AddScoped<IOutsideMappingHelperService, OutsideMappingHelperService>();
            

            services.AddScoped<ICategoryHelperService, CategoryHelperService>();
            services.AddScoped<IMenuHelperService, MenuHelperService>();

            services.AddScoped<HttpCurrentContextService>();
            services.AddScoped<ICurrentContextFactory, CurrentContextFactory>();
            services.AddScoped<IDocOpenXmlService, DocOpenXmlService>();
            services.AddScoped(di => di.GetRequiredService<ICurrentContextFactory>().GetCurrentContext());
            return services;
        }
    }
}
