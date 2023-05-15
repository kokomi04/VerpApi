﻿using AutoMapper;
using IdentityServer4.EntityFramework.Stores;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Services.Organization.Model;
using Services.PurchaseOrder.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Verp.Services.PurchaseOrder.Model;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.Constants;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Queue;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Service;
using VErp.Services.Grpc;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Master.Model;
using VErp.Services.Master.Service;
using VErp.Services.Organization.Service;
using VErp.Services.Stock.Model;
using VErp.Services.Stock.Service;
using VErp.Services.Stock.Service.Stock;
using VErp.WebApis.VErpApi.Validator;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;

namespace VErp.WebApis.VErpApi
{
    public class Startup : BaseStartup
    {
        public Startup(AppConfigSetting appConfig) : base(appConfig)
        {

        }

        private X509Certificate2 _cert;
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

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            _cert = Certificate.Get(AppSetting.Configuration.SigninCert, AppSetting.Configuration.SigninCertPassword);

            services
                .AddIdentityServer()
                .AddSigningCredential(_cert)
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

            ConfigSwagger(services);

            ConfigureAutoMaper(services);

            services.AddHttpClient<PushServiceClient>();
            // services.AddHostedService<WebPushNotificationsProducer>();

            AddBackgroundQueueProcess(services);

            return BuildService(services);
        }

        private static void AddBackgroundQueueProcess(IServiceCollection services)
        {
            services.AddInProcessBackgroundQueuePublisher();

            services.AddInProcessBackgroundConsummer<IInventoryService, string>(PRODUCTION_INVENTORY_STATITICS, async (_inventoryService, msg, calcelToken) =>
            {
                await _inventoryService.ProductionOrderInventory(new[] { msg.Data }, EnumProductionStatus.ProcessingLessStarted, string.Empty);
            });

            services.AddInProcessBackgroundConsummer<IProductionProgressService, ProductionOrderStatusDataModel>(PRODUCTION_INVENTORY_APPROVED, async (_productionOrderService, msg, calcelToken) =>
            {
                await _productionOrderService.CalcAndUpdateProductionOrderStatus(msg.Data);
            });
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
            services.AddScopedServices(ManufacturingServiceAssembly.Assembly);
            services.AddServiceCoreDependency();
        }

        private void ConfigureAutoMaper(IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Startup));

            var profile = new MappingProfile();
            profile.ApplyMappingsFromAssembly(MasterModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(OrganizationModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(StockModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(AccountancyModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ReportConfigModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(PurchaseOrderModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ManufacturingModelAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(GlobalObjectAssembly.Assembly);
            profile.ApplyMappingsFromAssembly(ServiceCoreAssembly.Assembly);

            services.AddAutoMapper(cfg => cfg.AddProfile(profile), this.GetType().Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var pathBase = AppSetting.PathBase;
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }


            ConfigureBase(app, env, loggerFactory, true);

            app.UseEndpointsGrpcService(GrpcServiceAssembly.Assembly);
            app.UseSignalRHubEndpoints(ServiceCoreAssembly.Assembly);

            app.UseSwagger()
              .UseSwaggerUI(c =>
              {
                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/system/swagger.json", "SYSTEM.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/organization/swagger.json", "ORGANIZATION.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/stock/swagger.json", "STOCK.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/purchaseorder/swagger.json", "PURCHASE-ORDER.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/accountancy/swagger.json", "ACCOUNTANTCY.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/report/swagger.json", "REPORT.API V1");

                  c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}swagger/manufacturing/swagger.json", "MANUFACTURING.API V1");

                  c.OAuthClientId("web");
                  c.OAuthClientSecret("secretWeb");
                  c.OAuthAppName("VERP Swagger UI");
                  c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { OAuthFormKeyConstants.SubsidiaryCode, AppSetting.Developer?.SubsidiaryCodes?.FirstOrDefault() } });
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
                options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "VErpApi.xml"));


                options.SwaggerDoc("system", new OpenApiInfo
                {
                    Title = "VERP System HTTP API",
                    Version = "v1",
                    Description = "The system Service HTTP API"
                });

                options.SwaggerDoc("organization", new OpenApiInfo
                {
                    Title = "VERP Organization HTTP API",
                    Version = "v1",
                    Description = "The organization Service HTTP API"
                });

                options.SwaggerDoc("stock", new OpenApiInfo
                {
                    Title = "VERP Stock HTTP API",
                    Version = "v1",
                    Description = "The Stock Service HTTP API"
                });

                options.SwaggerDoc("purchaseorder", new OpenApiInfo
                {
                    Title = "VERP System HTTP API",
                    Version = "v1",
                    Description = "The system Service HTTP API"
                });


                options.SwaggerDoc("accountancy", new OpenApiInfo
                {
                    Title = "VERP Accountancy HTTP API",
                    Version = "v1",
                    Description = "The Accountancy Service HTTP API"
                });

                options.SwaggerDoc("report", new OpenApiInfo
                {
                    Title = "VERP Report HTTP API",
                    Version = "v1",
                    Description = "The Report Service HTTP API"
                });

                options.SwaggerDoc("manufacturing", new OpenApiInfo
                {
                    Title = "VERP Manufacturing HTTP API",
                    Version = "v1",
                    Description = "The Manufacturing Service HTTP API"
                });


                options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Password = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{AppSetting.Identity.Endpoint?.TrimEnd('/')}/connect/authorize"),
                            TokenUrl = new Uri($"{AppSetting.Identity.Endpoint?.TrimEnd('/')}/connect/token"),
                            Scopes = new Dictionary<string, string>()
                            {
                                { "verp offline_access", "verp api common access" }
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
