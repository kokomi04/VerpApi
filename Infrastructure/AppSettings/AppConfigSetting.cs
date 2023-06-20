using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Infrastructure.AppSettings
{
    public class AppConfigSetting
    {
        private AppConfigSetting() { }

        public IConfigurationRoot Configuration { get; private set; }
        public AppSetting AppSetting { get; private set; }

        public static AppConfigSetting Config(string environmentName = null, string basePath = null)
        {
            var exeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty;
            if (EnviromentConfig.IsUnitTest)
            {
                exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            exeFolder = exeFolder
                .Replace(@"file:\", "")
                .Replace(@"file:", "")
                .TrimStart('/');

            var modeName = EnviromentConfig.EnviromentName;

            var configFile = Environment.GetEnvironmentVariable("CONFIG");


            var builder = new ConfigurationBuilder()
                    .SetBasePath(exeFolder)
                    .AddJsonFile($"AppSetting.json", true, true)
                    .AddJsonFile($"AppSetting.{environmentName ?? modeName}.json", false, true)
                    .AddJsonFile($"AppService.json", true, true)
                    .AddJsonFile($"AppServiceCustom.json", true, true);

            if (!string.IsNullOrWhiteSpace(configFile))
            {
                builder.AddJsonFile(configFile, false, true);
            }
            else
            {
                throw new MissingFieldException("Missing config file, check your enviroment variable CONFIG");
            }


            var config = builder.Build();

            var result = new AppConfigSetting();
            result.Configuration = config;

            result.AppSetting = new AppSetting();
            config.Bind(result.AppSetting);

            return result;
        }

    }
}
