using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Infrastructure.AppSettings
{
    public class AppConfigSetting
    {
        private AppConfigSetting() { }

        public IConfigurationRoot Configuration { get; private set; }
        public AppSetting AppSetting { get; private set; }

        public static AppConfigSetting Config(string environmentName = null, bool excludeSensitiveConfig = false, string basePath = null)
        {
            var exeFolder = basePath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase) ?? string.Empty;
            exeFolder = exeFolder
                .Replace(@"file:\", "")
                .Replace(@"file:", "");

            var modeName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                    .SetBasePath(exeFolder)
                    .AddJsonFile($"AppSetting.json", false, true)
                    .AddJsonFile($"AppSetting.{environmentName ?? modeName}.json", false, true)
                    .AddJsonFile($"AppService.json", false, true);

            if (!excludeSensitiveConfig)
            {
                AddEnvironmentConfig(builder);
            }

            var config = builder.Build();


            var result = new AppConfigSetting();
            result.Configuration = config;

            result.AppSetting = new AppSetting();
            config.Bind(result.AppSetting);

            return result;
        }

        public static void AddEnvironmentConfig(IConfigurationBuilder builder)
        {
            var appConfigTemp = builder.Build().Get<AppSetting>();

            builder.AddJsonFile(appConfigTemp.Configuration.ConfigFileKey, false, true);
        }


    }
}
