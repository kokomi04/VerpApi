using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface ISubSystemService
    {
        Task<IList<SubSystemInfo>> GetSubSystems();
        Task<string[]> GetDbByModuleTypeId(EnumModuleType moduleTypeId);
    }
    public class SubSystemService : ISubSystemService
    {
        private static readonly Dictionary<EnumModuleType, string[]> SubSystemWithDB = new Dictionary<EnumModuleType, string[]>()
        {
            {EnumModuleType.Master, new[] { "MasterDB", "ActivityLogDB" } },
            {EnumModuleType.Accountant, new[] { "AccountancyDB", "ReportConfigDB" } },
            {EnumModuleType.Organization, new[] { "OrganizationDB" } },
            {EnumModuleType.PurchaseOrder, new[] { "PurchaseOrderDB" } },
            {EnumModuleType.Stock, new[] { "StockDB" } },
        };

        public async Task<IList<SubSystemInfo>> GetSubSystems()
        {
            var ss = EnumExtensions.GetEnumMembers<EnumModuleType>().Select(m => new SubSystemInfo
            {
                ModuleTypeId = m.Enum,
                Title = m.Description
            }).ToList();

            return ss;
        }

        public async Task<string[]> GetDbByModuleTypeId(EnumModuleType moduleTypeId)
        {
            if (!SubSystemWithDB.ContainsKey(moduleTypeId)) throw new BadRequestException(GeneralCode.ItemNotFound);
            return SubSystemWithDB[moduleTypeId];
        }
    }
}
