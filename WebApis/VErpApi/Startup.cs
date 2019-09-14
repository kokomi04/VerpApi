using IdentityServer4.EntityFramework.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Accountant.Service;
using System;
using System.Reflection;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Services.Master.Service;
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
            ConfigureStandardServices(services);           

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

            return BuildService(services);
        }
        private void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(MasterServiceAssembly.Assembly);
            services.AddScopedServices(AccountantServiceAssembly.Assembly);            
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {            
            ConfigureBase(app, env, loggerFactory, true);
            
        }
    }
}
