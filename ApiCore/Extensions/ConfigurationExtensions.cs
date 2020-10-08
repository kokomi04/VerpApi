using ActivityLogDB;
using Grpc.AspNetCore.ClientFactory;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Infrastructure.ApiCore.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ConfigMasterDBContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections, ServiceLifetime contextScope)
        {
            services.AddDbContext<MasterDBContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.MasterDatabase);
            }, contextScope);

            services.AddDbContext<UnAuthorizeMasterDBContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.MasterDatabase);
            }, ServiceLifetime.Scoped);
            
        }

        public static void ConfigStockDBContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<StockDBContext, StockDBRestrictionContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.StockDatabase);
            }, ServiceLifetime.Scoped);
        }
        public static void ConfigPurchaseOrderContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<PurchaseOrderDBContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.PurchaseOrderDatabase);
            }, ServiceLifetime.Scoped);
        }
        public static void ConfigOrganizationContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<OrganizationDBContext, OrganizationDBRestrictionContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.OrganizationDatabase);
            }, ServiceLifetime.Scoped);

            services.AddDbContext<UnAuthorizeOrganizationContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.OrganizationDatabase);
            }, ServiceLifetime.Scoped);
            
        }

        //public static void ConfigAccountingContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        //{
        //    services.AddDbContext<AccountingDBContext, AccountingDBRestrictionContext>((option) =>
        //    {
        //        option.UseSqlServer(databaseConnections.AccountingDatabase, opt =>
        //        {
        //            opt.CommandTimeout(600);
        //        });
        //    }, ServiceLifetime.Scoped);
        //}
        public static void ConfigAccountancyContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<AccountancyDBContext, AccountancyDBRestrictionContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.AccountancyDatabase);
            }, ServiceLifetime.Scoped);
        }

        public static void ConfigReportConfigDBContextContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<ReportConfigDBContext, ReportConfigDBRestrictionContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.ReportConfigDatabase);
            }, ServiceLifetime.Scoped);
        }

        public static void ConfigActivityLogContext(this IServiceCollection services, DatabaseConnectionSetting databaseConnections)
        {
            services.AddDbContext<ActivityLogDBContext>((option) =>
            {
                option.UseSqlServer(databaseConnections.ActivityLogDatabase);
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

        public static IApplicationBuilder UseEndpointsGrpcService(this IApplicationBuilder app, Assembly assembly)
        {
            app.UseEndpoints(opt =>
            {
                AddEndpointsGrpcService(opt, assembly, "Service");
            });

            return app;
        }

        public static void AddEndpointsGrpcService(IEndpointRouteBuilder builder, Assembly assembly, string surfix)
        {
            var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(surfix))
            .ToArray();

            foreach (var type in types)
            {
                var method = typeof(GrpcEndpointRouteBuilderExtensions).GetMethod("MapGrpcService")
                    .MakeGenericMethod(type);
                method?.Invoke(null, new[] { builder });
            }
        }

        public static IServiceCollection AddCustomGrpcClient(IServiceCollection services, Assembly assembly,
            Action<GrpcClientFactoryOptions> configureClient, Action<GrpcContextPropagationOptions> configureOptions, string surfix)
        {
            var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(surfix))
            .Select(t => t.BaseType)
            .ToArray();

            foreach (var type in types)
            {
                var method = typeof(GrpcClientServiceExtensions).GetMethod("AddGrpcClient", new[] { typeof(IServiceCollection), typeof(Action<GrpcClientFactoryOptions>) })
                    .MakeGenericMethod(type);
                (method?.Invoke(null, new object[] { services, configureClient }) as IHttpClientBuilder)?
                    .EnableCallContextPropagation(configureOptions);
            }

            return services;
        }

        public static IServiceCollection AddCustomGrpcClient(this IServiceCollection services, Assembly assembly,
            Action<GrpcClientFactoryOptions> configureClient, Action<GrpcContextPropagationOptions> configureOptions)
        {
            return AddCustomGrpcClient(services, assembly, configureClient, configureOptions, "Client");
        }
    }
}
