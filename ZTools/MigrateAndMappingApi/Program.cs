using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VErp.Infrastructure.AppSettings;

namespace MigrateAndMappingApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var appSetting = AppConfigSetting.Config();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((services) =>
                {
                    services.AddSingleton(appSetting);
                })
                .UseStartup<Startup>();
        }
    }
}
