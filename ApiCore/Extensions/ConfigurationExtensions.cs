using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Infrastructure.ApiCore.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ConfigMasterDBContext(this IServiceCollection services, AppSetting appSetting, ServiceLifetime contextScope)
        {
            services.AddDbContext<MasterDBContext>((option) =>
            {
                option.UseLazyLoadingProxies(false);
                option.UseSqlServer(appSetting.DatabaseConnections.MasterDatabase);
            }, contextScope);
        }

        public static void ConfigStockDBContext(this IServiceCollection services, AppSetting appSetting)
        {
            services.AddDbContext<StockDBContext, StockDBRestrictionContext>((option) =>
            {
                option.UseLazyLoadingProxies(false);
                option.UseSqlServer(appSetting.DatabaseConnections.StockDatabase);
            }, ServiceLifetime.Scoped);
        }
        public static void ConfigPurchaseOrderContext(this IServiceCollection services, AppSetting appSetting)
        {
            services.AddDbContext<PurchaseOrderDBContext>((option) =>
            {
                option.UseLazyLoadingProxies(false);
                option.UseSqlServer(appSetting.DatabaseConnections.PurchaseOrderDatabase);
            }, ServiceLifetime.Scoped);
        }
       
        public static IServiceCollection AddScopedServices(this IServiceCollection services, Assembly assembly)
        {
            return services
                .AddScopedInferfaces("Service", assembly);
        }

        public static IServiceCollection AddScopedInferfaces(this IServiceCollection services, string interfaceSurfix, Assembly assembly)
        {
            var classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(interfaceSurfix))
                .ToArray();

            foreach (var classType in classTypes)
            {
                var interfaceType = classType.GetInterface("I" + classType.Name);
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, classType);
                }
            }

            return services;
        }
    }
}
