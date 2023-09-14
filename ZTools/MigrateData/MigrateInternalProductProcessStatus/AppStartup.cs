using Lib.Net.Http.WebPush;
using Microsoft.Extensions.DependencyInjection;
using MigrateProductProcessStatus.Services;
using Services.Organization.Model;
using Services.PurchaseOrder.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.PurchaseOrder.Model;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.Library;
using VErp.Commons.Library.Queue;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Service;
using VErp.Services.Grpc;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Manafacturing.Service.ProductionProcess.Implement;
using VErp.Services.Master.Model;
using VErp.Services.Master.Service;
using VErp.Services.Organization.Service;
using VErp.Services.Stock.Model;
using VErp.Services.Stock.Service;
using VErp.Services.Stock.Service.Stock;

namespace MigrateInternalProductProcessStatus
{
    internal class AppStartup : BaseStartup
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
            services.AddScopedServices(StockServiceAssembly.Assembly);
            services.AddScopedServices(ManufacturingServiceAssembly.Assembly);
            services.AddServiceCoreDependency();

            ResolveCustomService(services);
        }

        private void ConfigureAutoMaper(IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Startup));

            var profile = new MappingProfile();
            profile.ApplyMappingsFromAssembly(StockModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ManufacturingModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(GlobalObjectAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ServiceCoreAssembly.Assembly);

            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }

        private static void ResolveCustomService(IServiceCollection services)
        {
            services.AddScoped<IMigrateProductProcessStatus, MigrateProductProcessStatus.Services.MigrateProductProcessStatus>();
        }
    }
}
