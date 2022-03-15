
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class DashboardTypeListModel : IMapFrom<DashboardType>
    {
        public int DashboardTypeId { get; set; }
        public int DashboardTypeGroupId { get; set; }
        public string DashboardTypeName { get; set; }
        public int SortOrder { get; set; }
    }

    public class DashboardTypeModel: DashboardTypeListModel
    {
        public string BodySql { get; set; }
        public string Columns { get; set; }
        public string JsProcessedChart { get; set; }
        public int ModuleTypeId { get; set; }
        public bool IsHide { get; set; }

    }
}