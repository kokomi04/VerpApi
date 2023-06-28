using Autofac;
using Autofac.Extensions.DependencyInjection;
using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Utilities;
using VErp.Infrastructure.ApiCore.BackgroundTasks;
using VErp.Infrastructure.ApiCore.Extensions;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.ApiCore.Middleware;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using static VErp.Commons.Library.JsonUtils;
using System.Threading.Tasks;

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

            services.AddSingleton<IConfiguration>(Configuration);

            CreateSerilogLogger(Configuration);


            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    // .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((host) => true)
                    // .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition")
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

            services.AddHostedService<SyncApiEndpointService>();
            services.AddHostedService<LongTaskStatusService>();

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
                options.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
                options.AllowEmptyInputInBodyModelBinding = true;
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));
                options.Filters.Add(typeof(ResponseStatusFilter));
                if (isRequireAuthrize)
                {
                    options.Filters.Add(typeof(AuthorizeActionFilter));
                }
                options.OutputFormatters.RemoveType<StringOutputFormatter>();
            })
           .AddNewtonsoftJson(options =>
           {
               JsonSetting(options.SerializerSettings);
           });

            ConfigureAuthService(services);

            services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcServerLoggerInterceptor>();
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSignalR();
        }

        protected void ConfigReadWriteDBContext(IServiceCollection services)
        {
            services.ConfigMasterDBContext(AppSetting.DatabaseConnections, ServiceLifetime.Scoped);
            services.ConfigStockDBContext(AppSetting.DatabaseConnections);
            services.ConfigPurchaseOrderContext(AppSetting.DatabaseConnections);
            services.ConfigOrganizationContext(AppSetting.DatabaseConnections);
            services.ConfigAccountancyContext(AppSetting.DatabaseConnections);
            services.ConfigActivityLogContext(AppSetting.DatabaseConnections);
            services.ConfigReportConfigDBContextContext(AppSetting.DatabaseConnections);
            services.ConfigManufacturingContext(AppSetting.DatabaseConnections);
        }

        protected IServiceProvider BuildService(IServiceCollection services)
        {
            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        protected void ConfigureBase(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, bool isIdentiy)
        {

            app.UseMiddleware<CultureInfoMiddleware>();
            app.UseMiddleware<RequestLogMiddleware>();

            loggerFactory.AddSerilog();

#if !DEBUG
            if (AppSetting.ElasticApm?.IsEnabled == true)
            {
                app.UseAllElasticApm(Configuration);
            }
#endif

            var pathBase = AppSetting.PathBase;
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }


            ConfigureHelthCheck(app);

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseDatabaseErrorPage();
            //}
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

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();

                var exception = feature.Error;


                var (response, statusCode) = HttpGlobalExceptionFilter.Handler(exception, AppSetting, env.IsProduction());

                var result = JsonConvert.SerializeObject(response, JsonSetting(null));

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;
                await context.Response.WriteAsync(result);
            }));

            app.UseMiddleware<ResponseLogMiddleware>();

            app.UseEndpoints(config =>
            {
                config.MapControllers();
            });

            Utils.LoggerFactory = loggerFactory;
        }
      
        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            //services.AddAuthorization(options =>
            //{                
            //    options.AddPolicy("tokens", p =>
            //    {
            //        p.AddAuthenticationSchemes("jwt", "introspection");
            //        p.RequireAuthenticatedUser();
            //    });
            //});

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // JWT tokens (default scheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                /*
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/signalr/hubs")))
                        {
                            // Read the token out of the query string
                            // context.Token = "Bearer " + accessToken;
                            context.Request.Headers.Authorization = "Bearer " + accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };*/

                options.Authority = AppSetting.Identity.Endpoint;
                options.Audience = AppSetting.Identity.ApiName;

                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                options.RequireHttpsMetadata = false;


                // if token does not contain a dot, it is a reference token
                options.ForwardDefaultSelector = (HttpContext context) =>
                {
                    var token = CustomTokenRetriever.FromHeaderAndQueryString(context.Request);

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return null;
                    }

                    if (token.Contains("."))
                    {
                        return "jwt";
                    }
                    else
                    {
                        return "introspection";
                    }
                };
            })
            // reference tokens
            .AddOAuth2Introspection("introspection", options =>
            {
                options.Authority = AppSetting.Identity.Endpoint;

                options.ClientId = AppSetting.Identity.ApiName;
                options.ClientSecret = AppSetting.Identity.ApiSecret;

                options.TokenRetriever = CustomTokenRetriever.FromHeaderAndQueryString;

                options.CacheDuration = TimeSpan.FromMinutes(10);
                options.EnableCaching = true;
            });
            //.AddIdentityServerAuthentication(options =>
            //{
            //    options.Authority = AppSetting.Identity.Endpoint;
            //    options.RequireHttpsMetadata = false;

            //    options.ApiName = AppSetting.Identity.ApiName;
            //    options.ApiSecret = AppSetting.Identity.ApiSecret;


            //    options.EnableCaching = false;
            //    options.CacheDuration = TimeSpan.FromMinutes(10);

            //    options.TokenRetriever = CustomTokenRetriever.FromHeaderAndQueryString;
            //});

            //services.AddScopeTransformation();
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
            //var seqServerUrl = configuration["Logging"];
            //var logstashUrl = configuration["Serilog:LogstashgUrl"];
            var filePathFormat = $"{AppSetting.Logging.OutputPath}/{EnviromentConfig.EnviromentName}/{AppSetting.ServiceName}/" + "Log-{Date}.log";
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

        private JsonSerializerSettings JsonSetting(JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null)
                serializerSettings = new JsonSerializerSettings();

            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            serializerSettings.ContractResolver = new CamelCaseExceptDictionaryKeysResolver();
            //serializerSettings.Converters.Add(new JsonSerializeDeepConverter(JsonUtils.JSON_MAX_DEPTH));
            return serializerSettings;
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

    public class CustomTokenRetriever
    {
        internal const string TokenItemsKey = "idsrv4:tokenvalidation:token";
        // custom token key change it to the one you use for sending the access_token to the server
        // during websocket handshake
        internal const string SignalRTokenKey = "signalr_token";

        static Func<HttpRequest, string> AuthHeaderTokenRetriever { get; set; }
        static Func<HttpRequest, string> QueryStringTokenRetriever { get; set; }

        static CustomTokenRetriever()
        {
            AuthHeaderTokenRetriever = TokenRetrieval.FromAuthorizationHeader();
            QueryStringTokenRetriever = TokenRetrieval.FromQueryString();
        }

        public static string FromHeaderAndQueryString(HttpRequest request)
        {
            var token = AuthHeaderTokenRetriever(request);

            if (string.IsNullOrEmpty(token))
            {
                token = QueryStringTokenRetriever(request);
            }

            if (string.IsNullOrEmpty(token))
            {
                token = request.HttpContext.Items[TokenItemsKey] as string;
            }

            if (string.IsNullOrEmpty(token) && request.Query.TryGetValue(SignalRTokenKey, out StringValues extract))
            {
                token = extract.ToString();
            }

            return token;
        }
    }
}
