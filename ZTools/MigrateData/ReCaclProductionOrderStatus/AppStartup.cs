using Microsoft.Extensions.DependencyInjection;
using ReCaclProductionOrderStatus.Services;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Grpc;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service;
using VErp.Services.Stock.Model;
using VErp.Services.Stock.Service;
using VErp.Infrastructure.ApiCore.Extensions;

namespace ReCaclProductionOrderStatus
{
    internal class AppStartup : BaseStartup
    {
        public AppStartup(AppConfigSetting appConfig) : base(appConfig)
        {

        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCustomGrpcClient(GrpcServiceAssembly.Assembly,
             configureClient =>
             {
                 configureClient.Address = new Uri(AppSetting.GrpcInternal?.Address?.TrimEnd('/') ?? "http://0.0.0.0:9999/");
             }, configureOptions =>
             {
                 configureOptions.SuppressContextNotFoundErrors = true;
             });

            ConfigureStandardServices(services, true);

            ConfigReadWriteDBContext(services);          
           
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
            services.AddScoped<IMigrateInventoryService, MigrateInventoryService>();
            services.AddScoped<IRecalcProductionOrderStatusService, RecalcProductionOrderStatusService>();
        }
    }
}
