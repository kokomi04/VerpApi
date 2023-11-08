using AutoMapper;
using System.Collections.Generic;
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

        public decimal CountedHoliday { get; set; }

        public decimal CountedWeekdayHour { get; set; }

        public decimal CountedWeekendHour { get; set; }

        public decimal CountedHolidayHour { get; set; }

        public long MinsLate { get; set; }

        public int CountedLate { get; set; }

        public long MinsEarly { get; set; }

        public int CountedEarly { get; set; }

        public IList<TimeSheetAggregateAbsenceModel> TimeSheetAggregateAbsence { get; set; } = new List<TimeSheetAggregateAbsenceModel>();

        public IList<TimeSheetAggregateOvertimeModel> TimeSheetAggregateOvertime { get; set; } = new List<TimeSheetAggregateOvertimeModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetAggregate, TimeSheetAggregateModel>()
            .ForMember(m => m.TimeSheetAggregateAbsence, v => v.MapFrom(m => m.TimeSheetAggregateAbsence))
            .ForMember(m => m.TimeSheetAggregateOvertime, v => v.MapFrom(m => m.TimeSheetAggregateOvertime))
            .ReverseMapCustom()
            .ForMember(m => m.TimeSheetAggregateAbsence, v => v.MapFrom(m => m.TimeSheetAggregateAbsence))
            .ForMember(m => m.TimeSheetAggregateOvertime, v => v.MapFrom(m => m.TimeSheetAggregateOvertime));
        }
    }
}