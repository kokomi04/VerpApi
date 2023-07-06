using Microsoft.Extensions.DependencyInjection;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.AppSettings;
using MigrateProductProcessStatus.Services;
using VErp.Services.Stock.Service.Products;
using Newtonsoft.Json.Linq;
using VErp.Infrastructure.EF.OrganizationDB;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.MasterEnum;

namespace MigrateInternalProductProcessStatus
{
    internal class Program
    {
        
        static IServiceProvider ServiceProvider;
        static AppSetting AppSetting;
        static void Main(string[] args)
        {
            var development = args[0];
            var file = args[1];
            if (!File.Exists(file))
            {
                Console.WriteLine($"File {file} not found!");
                return;
            }
            Console.WriteLine("Wellcome to migrate product internal name");
            SetEnviroment(development, file);
            DI();
            ExcuteWithSubId();
            Console.WriteLine("Suscess update product process status all data!");
            Console.ReadLine();
        }
        private static void SetEnviroment(string development, string file)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", development);
            Environment.SetEnvironmentVariable("CONFIG", file);
            Console.WriteLine($"\nSet enviroment variable ASPNETCORE_ENVIRONMENT = {development}");
        }
        private static void DI()
        {
            var serviceCollection = new ServiceCollection();
            var setting = AppConfigSetting.Config();
            AppSetting = setting.AppSetting;
            ServiceProvider = new AppStartup(setting).ConfigureServices(serviceCollection);

            var mainContextFactory = ServiceProvider.GetRequiredService<ICurrentContextFactory>();
            mainContextFactory.SetCurrentContext(new ScopeCurrentContextService(null, 0, VErp.Commons.Enums.MasterEnum.EnumActionType.Censor, null, null, 0, null, null, null, null));

        }
        private static void ExcuteWithSubId()
        {
            var organizationDBContext = ServiceProvider.GetRequiredService<OrganizationDBContext>();
            var subIds = organizationDBContext.Subsidiary.IgnoreQueryFilters().Where(s => !s.IsDeleted).Select(s => s.SubsidiaryId).ToList();
            foreach (var subId in subIds)
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var contextFactory = scope.ServiceProvider.GetRequiredService<ICurrentContextFactory>();
                    contextFactory.SetCurrentContext(new ScopeCurrentContextService(null, 0, EnumActionType.Censor, null, null, subId, null, null, null, null));

                    var service = scope.ServiceProvider.GetRequiredService<IMigrateProductProcessStatus>();
                    service.Execute().ConfigureAwait(true).GetAwaiter().GetResult();
                    Console.WriteLine("Success update status for subid " + subId);
                }
            }
        }

    }
}