using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrateData.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MigrateData
{
    internal class Program
    {
        static IServiceProvider ServiceProvider;
        static AppSetting AppSetting;
        static void Main(string[] args)
        {
            Console.WriteLine("Wellcome to migrate data ");
           

            SetEnviroment();
            DI();
            var migrateProductionProcessStatusService = ServiceProvider.GetRequiredService<IMigrateProductionProcessStatusToProductService>();
            migrateProductionProcessStatusService.Execute().ConfigureAwait(true).GetAwaiter().GetResult();
            Console.WriteLine("Success migrate production process status to products !");

            var migrateAssignStatusService = ServiceProvider.GetRequiredService<IMigrateProductionOrderAssignmentStatusService>();
            migrateAssignStatusService.Execute().ConfigureAwait(true).GetAwaiter().GetResult();
            Console.WriteLine("Success migrate production order assignment status!");
            
            Console.ReadLine();
        }

    //    public static IHostBuilder CreateHostBuilder(string[] args) =>
    //Host.CreateDefaultBuilder(args)
    //    .ConfigureLogging(logging =>
    //    {
    //        logging.ClearProviders();
    //        logging.AddConsole();
    //    })
    //    .ConfigureWebHostDefaults(webBuilder =>
    //    {
    //        webBuilder.UseStartup<Startup>();
    //    });

        private static void SetEnviroment()
        {
            var enviroments = new Dictionary<char, string>()
            {
                {'l',"Local" },
                {'d',"Development" },
                {'s',"Staging" },
                {'p',"Production" },
                {'e',"Exit" },
            };

            Console.WriteLine("Enter enviroment:  \nl - Local \nd - Development \ns - Staging \np - Production \ne - Exit\n");

            var command = '\0';
            while (command == '\0')
            {
                var _char = Console.ReadKey().KeyChar;
                if (enviroments.Keys.Contains(_char))
                {
                    command = _char;
                }
            }

            if (command == 'e')
            {
                Environment.Exit(0);
                return;
            }

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", enviroments[command]);
            Console.WriteLine($"\nSet enviroment variable ASPNETCORE_ENVIRONMENT = {enviroments[command]}");
        }

        private static void DI()
        {
            var serviceCollection = new ServiceCollection();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                //builder
                //    .AddFilter("Microsoft", LogLevel.Warning)
                //    .AddFilter("System", LogLevel.Warning)
                //    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                //    .AddConsole();
            });
            loggerFactory.AddSerilog();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            var setting = AppConfigSetting.Config();
            AppSetting = setting.AppSetting;
            ServiceProvider = new AppStartup(setting).ConfigureServices(serviceCollection);
            var contextFactory = ServiceProvider.GetRequiredService<ICurrentContextFactory>();

            contextFactory.SetCurrentContext(new ScopeCurrentContextService(null,0, VErp.Commons.Enums.MasterEnum.EnumActionType.Censor, null, null, 0, null, null, null, null));
        }


    }
}
