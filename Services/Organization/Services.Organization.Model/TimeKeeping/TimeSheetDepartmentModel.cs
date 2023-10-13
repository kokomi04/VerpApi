using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDepartmentModel : IMapFrom<TimeSheetDepartment>
    {
        public long TimeSheetId { get; set; }

        public int DepartmentId { get; set; }
    }
}