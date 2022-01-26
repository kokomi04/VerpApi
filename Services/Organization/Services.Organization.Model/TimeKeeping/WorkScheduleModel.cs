using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class WorkScheduleModel : IMapFrom<WorkSchedule>
    {
        public int WorkScheduleId { get; set; }
        public string WorkScheduleTitle { get; set; }
        public bool IsAbsenceForSaturday { get; set; }
        public bool IsAbsenceForSunday { get; set; }
        public bool IsAbsenceForHoliday { get; set; }
        public bool IsCountWorkForHoliday { get; set; }
        public bool IsDayOfTimeOut { get; set; }
        public int TimeSortConfigurationId { get; set; }
        public int FirstDayForCountedWork { get; set; }
        public bool IsRoundBackForTimeOutAfterWork { get; set; }
        public int RoundLevelForTimeOutAfterWork { get; set; }
        public int DecimalPlace { get; set; }
    }
}