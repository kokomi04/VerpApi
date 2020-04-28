using AutoMapper;
using IdentityServer4.EntityFramework.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Accountant.Service;
using Services.Organization.Model;
using Services.PurchaseOrder.Service;
using System;
using System.Reflection;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service;
using VErp.Services.Organization.Service;
using VErp.Services.Stock.Service;
using VErp.WebApis.VErpApi.Validator;

namespace VErp.WebApis.VErpApi
{
    public class Startup : BaseStartup
    {
        public Startup(AppConfigSetting appConfig) : base(appConfig)
        {

        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureStandardServices(services, true);           

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services
                .AddIdentityServer()
                .AddSigningCredential(Certificate.Get(AppSetting.Configuration.SigninCert, AppSetting.Configuration.SigninCertPassword))
                .AddConfigurationStore((option) =>
                {
                    option.ConfigureDbContext = (builder) =>
                    {
                        builder.UseSqlServer(AppSetting.DatabaseConnections.IdentityDatabase, sql => sql.MigrationsAssembly(migrationsAssembly));
                    };
                })
                .AddOperationalStore((option) =>
                {
                    option.ConfigureDbContext = (builder) =>
                    {
                        builder.UseSqlServer(AppSetting.DatabaseConnections.IdentityDatabase, sql => sql.MigrationsAssembly(migrationsAssembly));

                    };
                    option.EnableTokenCleanup = true;
                    option.TokenCleanupInterval = 3600;
                })
                .AddInMemoryCaching()
                .AddClientStoreCache<ClientStore>()
                .AddConfigurationStoreCache()
                .AddResourceStoreCache<ResourceStore>()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddProfileService<ProfileService>()
                .AddCustomTokenRequestValidator<CustomTokenRequestValidator>();

            ConfigureBussinessService(services);

            ConfigureAutoMaper(services);

            return BuildService(services);
        }
        private void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(ServiceCoreAssembly.Assembly);
            services.AddScopedServices(MasterServiceAssembly.Assembly);
            services.AddScopedServices(AccountantServiceAssembly.Assembly);
            services.AddScopedServices(StockServiceAssembly.Assembly);
            services.AddScopedServices(PurchaseOrderServiceAssembly.Assembly);
            services.AddScopedServices(OrganizationServiceAssembly.Assembly);
            services.AddServiceCoreDependency();
        }

        private void ConfigureAutoMaper(IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Startup));

            var profile = new MappingProfile();
            profile.ApplyMappingsFromAssembly(OrganizationModelAssembly.Assembly);

            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {            
            ConfigureBase(app, env, loggerFactory, true);
            
        }
    }
}
