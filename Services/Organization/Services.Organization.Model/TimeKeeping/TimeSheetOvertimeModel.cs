using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetOvertimeModel : IMapFrom<TimeSheetOvertime>
    {
        public long TimeSheetOvertimeId { get; set; }
        public long TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public int OvertimeLevelId { get; set; }
        public decimal MinsOvertime { get; set; }
    }
}