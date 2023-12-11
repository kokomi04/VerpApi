using AutoMapper;
using System;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailShiftOvertimeModel : IMapFrom<TimeSheetDetailShiftOvertime>
    {
        public long TimeSheetDetailId { get; set; }

        public int ShiftConfigurationId { get; set; }

        public double StartTime { get; set; }

        public double EndTime { get; set; }

        public int OvertimeLevelId { get; set; }

        public long MinsOvertime { get; set; }

        public EnumTimeSheetOvertimeType OvertimeType { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetDetailShiftOvertime, TimeSheetDetailShiftOvertimeModel>()
            .ForMember(m => m.StartTime, v => v.MapFrom(m => m.StartTime.TotalSeconds))
            .ForMember(m => m.EndTime, v => v.MapFrom(m => m.EndTime.TotalSeconds))
            .ReverseMapCustom()
            .ForMember(m => m.StartTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTime)))
            .ForMember(m => m.EndTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTime)));
        }
    }
}
