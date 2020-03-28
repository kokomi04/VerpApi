using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.Json;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using static IdentityModel.OidcConstants;

namespace VErp.Infrastructure.ApiCore
{
    public class BaseStartup
    {
        protected AppSetting AppSetting { get; private set; }
        protected IConfigurationRoot Configuration { get; set; }

        protected BaseStartup(AppConfigSetting appConfig)
        {
            AppSetting = appConfig.AppSetting;
            Configuration = appConfig.Configuration;
        }

        protected void ConfigureStandardServices(IServiceCollection services, bool isRequireAuthrize)
        {

            services.Configure<ApiBehaviorOptions>(cfg =>
           {
               cfg.SuppressModelStateInvalidFilter = true;
           });

            services.Configure<AppSetting>(Configuration);

            CreateSerilogLogger(Configuration);

            ConfigDBContext(services);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    //.AllowCredentials()
                    .AllowAnyOrigin()
                    );
            })
              .AddHttpContextAccessor()
              .AddOptions()
              .AddCustomHealthCheck(Configuration)
              .AddDataProtection()
              .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
              {
                  EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
                  ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
              });

            services.AddControllers(options =>
            {
                options.Conventions.Add(new ApiExplorerGroupPerVersionConvention());

                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));
                options.Filters.Add(typeof(ResponseStatusFilter));
                if (isRequireAuthrize)
                {
                    options.Filters.Add(typeof(AuthorizeActionFilter));
                }
            })
           .AddNewtonsoftJson(options => {
               options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
               options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
           });


            services.AddRazorPages(options =>
            {
                //options.Conventions.Add(new ApiExplorerGroupPerVersionConvention());

                //options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                //options.Filters.Add(typeof(ValidateModelStateFilter));
                //if (isRequireAuthrize)
                //{
                //    options.Filters.Add(typeof(AuthorizeActionFilter));
                //}

            })
            .AddJsonOptions(options =>
            {
                //options.JsonSerializerOptions
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.IgnoreNullValues = true;


                //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
            })
           .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
           .AddControllersAsServices();

            ConfigureAuthService(services);

            ConfigSwagger(services);

        }

        private void ConfigDBContext(IServiceCollection services)
        {
            services.ConfigMasterDBContext(AppSetting, ServiceLifetime.Scoped);
            services.ConfigStockDBContext(AppSetting);
            services.ConfigPurchaseOrderContext(AppSetting);
            services.ConfigOrganizationContext(AppSetting);
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
                        "VErpApi.xml"));


                options.SwaggerDoc("stock", new OpenApiInfo
                {
                    Title = "VERP Stock HTTP API",
                    Version = "v1",
                    Description = "The Stock Service HTTP API"
                });

                options.SwaggerDoc("system", new OpenApiInfo
                {
                    Title = "VERP System HTTP API",
                    Version = "v1",
                    Description = "The system Service HTTP API"
                });


                options.SwaggerDoc("purchaseorder", new OpenApiInfo
                {
                    Title = "VERP System HTTP API",
                    Version = "v1",
                    Description = "The system Service HTTP API"
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
            });
        }


        protected IServiceProvider BuildService(IServiceCollection services)
        {
            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        protected void ConfigureBase(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, bool isIdentiy)
        {
            loggerFactory.AddSerilog();

            var pathBase = AppSetting.PathBase;
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            ConfigureHelthCheck(app);

            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}



            app.UseForwardedHeaders();
            

            app.UseRouting();

            /*For most apps, calls to UseAuthentication, UseAuthorization, and UseCors must appear between the calls to UseRouting and UseEndpoints to be effective.
*/
            app.UseCors("CorsPolicy");

            if (isIdentiy)
            {
                app.UseIdentityServer();
            }

            app.UseAuthorization();

            app.UseEndpoints(config =>
            {
                config.MapControllers();
            });

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/system/swagger.json", "SYSTEM.API V1");

                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/stock/swagger.json", "STOCK.API V1");

                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/purchaseorder/swagger.json", "PURCHASE-ORDER.API V1");

                   c.OAuthClientId("web");
                   c.OAuthClientSecret("secretWeb");
                   c.OAuthAppName("VERP Swagger UI");
               });

        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = AppSetting.Identity.Endpoint;
                options.RequireHttpsMetadata = false;

                options.ApiName = AppSetting.Identity.ApiName;
                options.ApiSecret = AppSetting.Identity.ApiSecret;


                options.EnableCaching = false;
                options.CacheDuration = TimeSpan.FromMinutes(10);
            });
        }

        protected virtual void ConfigureHelthCheck(IApplicationBuilder app)
        {
            app.UseHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });

        }

        private void CreateSerilogLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Logging"];
            var logstashUrl = configuration["Serilog:LogstashgUrl"];
            var filePathFormat = $"{AppSetting.Logging.OutputPath}/{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}/{AppSetting.ServiceName}/" + "Log-{Date}.log";
            var logTemplate = "{Level:u5} {Timestamp:yyyy-MM-dd HH:mm:ss.fff} - [R#{RequestId}]{Message:j}{EscapedException}{NewLine}{NewLine}";
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With(new ExceptionEnricher())
                .Enrich.WithProperty("ApplicationContext", AppSetting.ServiceName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(filePathFormat, restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: logTemplate, retainedFileCountLimit: 30, shared: true)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Logger = logger;
        }
    }
    class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception == null)
                return;

            var logEventProperty = propertyFactory.CreateProperty("EscapedException", logEvent.Exception.ToString().Replace("\r\n", "\\r\\n"));
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();
            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
            return services;
        }
    }
}
