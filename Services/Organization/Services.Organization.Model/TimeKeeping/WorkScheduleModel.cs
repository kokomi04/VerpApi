using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class WorkScheduleModel : IMapFrom<WorkSchedule>
    {
        public WorkScheduleModel()
        {
            ArrangeShifts = new List<ArrangeShiftModel>();
        }
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

        public bool IsCountInShift { get; set; }
        public long? TotalMins { get; set; }
        public int? CountShift { get; set; }
        public long? MinsThresholdRecord { get; set; }
        public bool? IsMinsThresholdRecord { get; set; }
        public long? MinsThresholdOvertime1 { get; set; }
        public long? MinsThresholdOvertime2 { get; set; }

        public virtual IList<ArrangeShiftModel> ArrangeShifts { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<WorkSchedule, WorkScheduleModel>()
            .ForMember(x => x.ArrangeShifts, v => v.MapFrom(m => m.ArrangeShift))
            .ReverseMapCustom()
            .ForMember(x => x.ArrangeShift, v => v.Ignore());
        }
    }
}