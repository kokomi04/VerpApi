using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetAggregateModel : IMapFrom<TimeSheetAggregate>
    {
        public long TimeSheetAggregateId { get; set; }
        public long TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public decimal CountedWeekday { get; set; }
        public decimal CountedWeekend { get; set; }
        public decimal CountedWeekdayHour { get; set; }
        public decimal CountedWeekendHour { get; set; }
        public long MinsLate { get; set; }
        public int CountedLate { get; set; }
        public long MinsEarly { get; set; }
        public int CountedEarly { get; set; }
        public decimal Overtime1 { get; set; }
        public decimal Overtime2 { get; set; }
        public decimal Overtime3 { get; set; }
        public int CountedAbsence { get; set; }
    }
}