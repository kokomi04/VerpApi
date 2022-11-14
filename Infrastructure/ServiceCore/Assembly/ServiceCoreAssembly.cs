using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Verp.Cache.Caching;
using Verp.Cache.MemCache;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.SignalR;

namespace VErp.Infrastructure.ServiceCore
{
    public static class ServiceCoreAssembly
    {
        public static Assembly Assembly => typeof(ServiceCoreAssembly).Assembly;
        public static IServiceCollection AddServiceCoreDependency(this IServiceCollection services)
        {
            services.AddHttpClient<IHttpClientFactoryService, HttpClientFactoryService>();
            services.AddTransient<IHttpCrossService, HttpCrossService>();

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
            services.AddScoped<IBarcodeConfigHelperService, BarcodeConfigHelperService>();


            services.AddScoped<ICategoryHelperService, CategoryHelperService>();
            services.AddScoped<IMenuHelperService, MenuHelperService>();

            services.AddScoped<HttpCurrentContextService>();
            services.AddScoped<ICurrentContextFactory, CurrentContextFactory>();
            services.AddScoped<IDocOpenXmlService, DocOpenXmlService>();
            services.AddScoped(di => di.GetRequiredService<ICurrentContextFactory>().GetCurrentContext());

            services.AddMemoryCache();
            services.AddScoped<ICachingService, MemCacheCachingService>();
            services.AddSingleton<IAuthDataCacheService, AuthDataCacheService>();

            services.AddScoped<IMailFactoryService, MailFactoryService>();
            services.AddScoped<INotificationFactoryService, NotificationFactoryService>();

            services.AddSingleton<IPrincipalBroadcasterService, PrincipalBroadcasterService>();

            services.AddTransient<ILongTaskResourceLockService, LongTaskResourceLockService>();

            return services;
        }
    }
}
