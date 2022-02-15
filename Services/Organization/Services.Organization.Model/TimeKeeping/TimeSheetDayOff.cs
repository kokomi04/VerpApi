using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDayOffModel: IMapFrom<TimeSheetDayOff>
    {
        public long TimeSheetDayOffId { get; set; }
        public long TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public int AbsenceTypeSymbolId { get; set; }
        public int CountedDayOff { get; set; }
    }
}