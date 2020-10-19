using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IManageVErpModuleService
    {
        Task<IList<ProductModuleInfo>> GetAllModule();
        Task<string[]> GetDbByModule(EnumModuleType moduleType);
    }
    public class ManageVErpModuleService : IManageVErpModuleService
    {
        private static readonly Dictionary<EnumModuleType, string[]> ModuleWithDB = new Dictionary<EnumModuleType, string[]>()
        {
            {EnumModuleType.Master, new[] { "MasterDB", "ActivityLogDB" } },
            {EnumModuleType.Accountant, new[] { "AccountancyDB", "ReportConfigDB" } },
            {EnumModuleType.Organization, new[] { "OrganizationDB" } },
            {EnumModuleType.PurchaseOrder, new[] { "PurchaseOrderDB" } },
            {EnumModuleType.Stock, new[] { "StockDB" } },
        };

        public async Task<IList<ProductModuleInfo>> GetAllModule()
        {
            var rs = new List<ProductModuleInfo>();
            foreach (var (m, db) in ModuleWithDB)
            {
                rs.Add(new ProductModuleInfo
                {
                    ModuleId = m,
                    ModuleName = m.GetEnumDescription(),
                });
            }

            return rs;
        }

        public async Task<string[]> GetDbByModule(EnumModuleType moduleType)
        {
            if (!ModuleWithDB.ContainsKey(moduleType)) throw new BadRequestException(GeneralCode.ItemNotFound);
            return ModuleWithDB[moduleType];
        }
    }
}
