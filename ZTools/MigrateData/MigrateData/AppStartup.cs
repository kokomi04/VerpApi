using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MigrateData.Services;
using Services.Organization.Model;
using Services.PurchaseOrder.Service;
using System;
using Verp.Services.PurchaseOrder.Model;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Service;
using VErp.Services.Grpc;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service;
using VErp.Services.Master.Model;
using VErp.Services.Master.Service;
using VErp.Services.Organization.Service;
using VErp.Services.Stock.Model;
using VErp.Services.Stock.Service;

namespace MigrateData
{
    public class AppStartup : BaseStartup
    {
        public AppStartup(AppConfigSetting appConfig) : base(appConfig)
        {

        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureStandardServices(services, true);

            ConfigReadWriteDBContext(services);

            services.AddCustomGrpcClient(GrpcServiceAssembly.Assembly,
                configureClient =>
                {
                    configureClient.Address = new Uri(AppSetting.GrpcInternal?.Address?.TrimEnd('/') ?? "http://0.0.0.0:9999/");
                }, configureOptions =>
                {
                    configureOptions.SuppressContextNotFoundErrors = true;
                });

            ConfigureBussinessService(services);

            ConfigureAutoMaper(services);

            return BuildService(services);
        }

        private static void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(ServiceCoreAssembly.Assembly);
            services.AddScopedServices(MasterServiceAssembly.Assembly);            
            services.AddScopedServices(AccountancyServiceAssembly.Assembly);
            services.AddScopedServices(StockServiceAssembly.Assembly);
            services.AddScopedServices(PurchaseOrderServiceAssembly.Assembly);
            services.AddScopedServices(ManufacturingServiceAssembly.Assembly);
            services.AddScopedServices(OrganizationServiceAssembly.Assembly);
            services.AddScopedServices(ReportConfigServiceAssembly.Assembly);
            services.AddServiceCoreDependency();

            ResolveCustomService(services);
        }

        private void ConfigureAutoMaper(IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Startup));

            var profile = new MappingProfile();
            profile.ApplyMappingsFromAssembly(OrganizationModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ManufacturingModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(MasterModelAssembly.Assembly);            
            profile.ApplyMappingsFromAssembly(AccountancyModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(PurchaseOrderModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ReportConfigModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(StockModelAssembly.Assembly);
            
            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }

        private static void ResolveCustomService(IServiceCollection services)
        {
            services.AddScoped<IMigrateProductionProcessStatusToProductService, MigrateProductionProcessStatusToProductService>();
            services.AddScoped<IMigrateProductionOrderAssignmentStatusService, MigrateProductionOrderAssignmentStatusService>();
        }
    }
}
