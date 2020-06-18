using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Service;

namespace ConfigApi
{
    public class Startup : BaseStartup
    {
        public Startup(AppConfigSetting appConfig) : base(appConfig)
        {

        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureStandardServices(services, true);

            ConfigSwagger(services);

            ConfigDbOwnerContext(services);

            ConfigureBussinessService(services);

            ConfigureAutoMaper(services);

            return BuildService(services);
        }
        private void ConfigureBussinessService(IServiceCollection services)
        {
            services.AddScopedServices(ServiceCoreAssembly.Assembly);
            //services.AddScopedServices(MasterServiceAssembly.Assembly);
            //services.AddScopedServices(AccountantServiceAssembly.Assembly);
            services.AddScopedServices(AccountancyServiceAssembly.Assembly);
            //services.AddScopedServices(StockServiceAssembly.Assembly);
            //services.AddScopedServices(PurchaseOrderServiceAssembly.Assembly);
            // services.AddScopedServices(OrganizationServiceAssembly.Assembly);
            //services.AddScopedServices(ReportConfigServiceAssembly.Assembly);
            services.AddServiceCoreDependency();
        }

        private void ConfigureAutoMaper(IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Startup));

            var profile = new MappingProfile();
            //profile.ApplyMappingsFromAssembly(OrganizationModelAssembly.Assembly);
            //profile.ApplyMappingsFromAssembly(AccountantModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(AccountancyModelAssembly.Assembly);
            //profile.ApplyMappingsFromAssembly(ReportConfigModelAssembly.Assembly);
            //profile.ApplyMappingsFromAssembly(PurchaseOrderModelAssembly.Assembly);

            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var pathBase = AppSetting.PathBase;
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }


            ConfigureBase(app, env, loggerFactory, false);

            app.UseSwagger()
              .UseSwaggerUI(c =>
              {
                  c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/accountancy/swagger.json", "ACCOUNTANTCY.API V1");

                  c.OAuthClientId("web");
                  c.OAuthClientSecret("secretWeb");
                  c.OAuthAppName("VERP Swagger UI");
              });

        }

        private void ConfigSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // options.DocumentFilter<CustomModelDocumentFilter>();
                //        //options.UseReferencedDefinitionsForEnums();
                //        //options.DescribeAllEnumsAsStrings();
                //        //options.UseReferencedDefinitionsForEnums();

                options.OperationFilter<HeaderFilter>();
                options.OperationFilter<AuthorizeCheckOperationFilter>();
                options.OperationFilter<SwaggerFileOperationFilter>();
                options.IncludeXmlComments(Path.Combine(
                        PlatformServices.Default.Application.ApplicationBasePath,
                        "ConfigApi.xml"));

                options.SwaggerDoc("accountancy", new OpenApiInfo
                {
                    Title = "VERP Accountancy HTTP API",
                    Version = "v1",
                    Description = "The Accountancy Service HTTP API"
                });             

                options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Password = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{AppSetting.Identity.Endpoint}/connect/authorize"),
                            TokenUrl = new Uri($"{AppSetting.Identity.Endpoint}/connect/token"),
                            Scopes = new Dictionary<string, string>()
                            {
                                { "scope", "verp offline_access openId" }
                            }
                        }
                    },
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Name = "Authorization",

                });


                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Name = "Authorization",
                    BearerFormat = "Bearer {token}",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""

                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme(){ Reference = new OpenApiReference(){ Type = ReferenceType.SecurityScheme, Id="OAuth2" } }, new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme(){ Reference = new OpenApiReference(){ Type = ReferenceType.SecurityScheme, Id="Bearer" } }, new List<string>()
                    }
                });
            })
            .AddSwaggerGenNewtonsoftSupport();
        }
    }
}
