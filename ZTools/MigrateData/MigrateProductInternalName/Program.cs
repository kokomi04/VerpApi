﻿using Microsoft.Extensions.DependencyInjection;
using MigrateProductInternalName.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;

namespace MigrateProductInternalName
{
    class Program
    {
        static IServiceProvider ServiceProvider;
        static AppSetting AppSetting;
        static void Main(string[] args)
        {
            Console.WriteLine("Wellcome to migrate product internal name");
            SetEnviroment();
            DI();
            var service = ServiceProvider.GetRequiredService<IMigrateProductInternalNameService>();
            service.Execute().ConfigureAwait(true).GetAwaiter().GetResult();
            Console.WriteLine("Success!");
            Console.ReadLine();
        }

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
            var setting = AppConfigSetting.Config();
            AppSetting = setting.AppSetting;
            ServiceProvider = new AppStartup(setting).ConfigureServices(serviceCollection);
            var contextFactory = ServiceProvider.GetRequiredService<ICurrentContextFactory>();
            contextFactory.SetCurrentContext(new ScopeCurrentContextService(0, VErp.Commons.Enums.MasterEnum.EnumActionType.Censor, null, null, 0, null, null, null, null));
        }
    }
}
