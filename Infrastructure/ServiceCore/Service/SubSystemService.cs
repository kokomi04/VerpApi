using System;
using System.Collections.Generic;
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
        Task<string[]> GetDbBySubSystemId(EnumModuleType subSystemId);
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
            var rs = new List<SubSystemInfo>();
            foreach(var value in Enum.GetValues(typeof(EnumModuleType)))
            {
                var name = ((Enum)value).GetEnumDescription();
                var id = (EnumModuleType)value;

                rs.Add(new SubSystemInfo
                {
                    SubSystemId = id,
                    SubSystemName = name,
                });
            }

            return rs;
        }

        public async Task<string[]> GetDbBySubSystemId(EnumModuleType subSystemId)
        {
            if (!SubSystemWithDB.ContainsKey(subSystemId)) throw new BadRequestException(GeneralCode.ItemNotFound);
            return SubSystemWithDB[subSystemId];
        }
    }
}
