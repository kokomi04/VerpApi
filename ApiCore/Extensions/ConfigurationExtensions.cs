using ActivityLogDB;
using GrpcProto.Protos;
using GrpcService.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public static IApplicationBuilder UseEndpointsGrpcService(this IApplicationBuilder app)
        {
            app.UseEndpoints(opt => {
                opt.MapGrpcService<InternalActivityLogService>();
            });

            return app;
        }

        public static IServiceCollection AddCustomGrpcClient(this IServiceCollection services, Uri address)
        {
            services.AddGrpc(options => {

            });

            services.AddGrpcClient<InternalActivityLog.InternalActivityLogClient>(opt => {
                opt.Address = address;
            })
                .EnableCallContextPropagation(opts => opts.SuppressContextNotFoundErrors = true);

            return services;
        }
    }
}
