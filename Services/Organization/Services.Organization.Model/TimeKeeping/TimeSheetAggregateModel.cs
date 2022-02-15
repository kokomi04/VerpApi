using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetAggregateModel: IMapFrom<TimeSheetAggregate>
    {
        public long TimeSheetAggregateId { get; set; }
        public long TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public int CountedWeekday { get; set; }
        public int CountedWeekend { get; set; }
        public long CountedWeekdayHour { get; set; }
        public long CountedWeekendHour { get; set; }
        public long MinsLate { get; set; }
        public int CountedLate { get; set; }
        public long MinsEarly { get; set; }
        public int CountedEarly { get; set; }
        public long Overtime1 { get; set; }
        public long Overtime2 { get; set; }
        public long Overtime3 { get; set; }
        public int CountedAbsence { get; set; }
    }
}