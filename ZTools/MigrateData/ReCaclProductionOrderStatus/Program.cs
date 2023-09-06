using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReCaclProductionOrderStatus.Services;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.OrganizationDB;

namespace ReCaclProductionOrderStatus
{
    internal class Program
    {
        static IServiceProvider ServiceProvider;
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine($"Missing config file!");
                return;
            }
            var file = args[0];
            if (!File.Exists(file))
            {
                Console.WriteLine($"Config file {file} not found!");
                return;
            }
            Console.WriteLine("MIGRATE PRODUCTION ORDER STATUS");
            Environment.SetEnvironmentVariable("CONFIG", file);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            DI();
            ExcuteWithSubId();
            Console.WriteLine("Suscess updated production order status!");
            Console.ReadLine();
        }

        private static void DI()
        {
            var serviceCollection = new ServiceCollection();
            var setting = AppConfigSetting.Config();
            ServiceProvider = new AppStartup(setting).ConfigureServices(serviceCollection);

            var mainContextFactory = ServiceProvider.GetRequiredService<ICurrentContextFactory>();
            mainContextFactory.SetCurrentContext(new ScopeCurrentContextService(null, 0, EnumActionType.Censor, null, null, 0, null, null, null, null));

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

                    var service = scope.ServiceProvider.GetRequiredService<IRecalcProductionOrderStatusService>();
                    service.Execute().ConfigureAwait(true).GetAwaiter().GetResult();
                    Console.WriteLine("Success updated production order status for " + subId);
                }
            }
        }
    }
}