using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrateProductInternalName.Services;
using Services.Organization.Model;
using Services.PurchaseOrder.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Verp.Services.PurchaseOrder.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Service;
using VErp.Services.Master.Service;
using VErp.Services.Organization.Service;
using VErp.Services.Stock.Service;
using VErp.Commons.Library;

namespace MigrateProductInternalName
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

          
            ConfigureBussinessService(services);

            ConfigureAutoMaper(services);

            return BuildService(services);
        }

        private static void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(ServiceCoreAssembly.Assembly);
            services.AddScopedServices(MasterServiceAssembly.Assembly);
            //services.AddScopedServices(AccountantServiceAssembly.Assembly);
            services.AddScopedServices(AccountancyServiceAssembly.Assembly);
            services.AddScopedServices(StockServiceAssembly.Assembly);
            services.AddScopedServices(PurchaseOrderServiceAssembly.Assembly);
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
            //profile.ApplyMappingsFromAssembly(AccountantModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(AccountancyModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(PurchaseOrderModelAssembly.Assembly);


            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }       

        private static void ResolveCustomService(IServiceCollection services)
        {
            services.AddScoped<IMigrateProductInternalNameService, MigrateProductInternalNameService>();
        }
    }
}
