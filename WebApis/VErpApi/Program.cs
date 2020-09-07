using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VErp.Infrastructure.AppSettings;
using Microsoft.Extensions.DependencyInjection;

namespace VErp.WebApis.VErpApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var appSetting = AppConfigSetting.Config();
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((s) =>
                {
                    s.AddSingleton(appSetting);
                })
                .UseKestrel()
                .UseUrls($"http://0.0.0.0:{appSetting.AppSetting.Port},https://0.0.0.0:{appSetting.AppSetting.HttpsPort}")
                .UseStartup<Startup>();
        }
    }
}
