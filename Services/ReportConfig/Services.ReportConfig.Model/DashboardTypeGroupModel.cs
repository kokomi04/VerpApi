
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class DashboardTypeGroupModel : IMapFrom<DashboardTypeGroup>
    {
        public int DashboardTypeGroupId { get; set; }
        public string DashboardTypeGroupName { get; set; }
        public int ModuleTypeId { get; set; }
        public int SortOrder { get; set; }
    }
}