using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VErp.Infrastructure.AppSettings;
using VErpApi.Seeds;

namespace VErp.WebApis.VErpApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            DBSeeder.Seed(host);
            DBSeeder.NormalizeData(host);
            
            host.Run();
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
                .UseUrls($"http://0.0.0.0:{appSetting.AppSetting.Port}")
                .UseStartup<Startup>();
        }
    }
}
