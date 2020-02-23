using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Accountant.Service;
using System;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.AppSettings;
using VErp.Services.Master.Service;
using VErp.Services.Stock.Service;

namespace MigrateAndMappingApi
{
    public class Startup : BaseStartup
    {
        public Startup(AppConfigSetting appConfig) : base(appConfig)
        {

        }


        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureStandardServices(services, false);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            ConfigureBussinessService(services);

            return BuildService(services);
        }
        private void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(MasterServiceAssembly.Assembly);
            services.AddScopedServices(AccountantServiceAssembly.Assembly);
            services.AddScopedServices(StockServiceAssembly.Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            ConfigureBase(app, env, loggerFactory, false);

            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                //if (env.IsDevelopment())
                if(env.EnvironmentName == "Local")
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

        }
    }
}
