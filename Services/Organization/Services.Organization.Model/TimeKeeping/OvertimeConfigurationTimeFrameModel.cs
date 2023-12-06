using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimeConfigurationTimeFrameModel : IMapFrom<OvertimeConfigurationTimeFrame>
    {
        public int OvertimeConfigurationId { get; set; }

        public EnumTimeSheetDateType TimeSheetDateType { get; set; }

        public double StartTime { get; set; }

        public double EndTime { get; set; }

        public int? OvertimeLevelId { get; set; }

        public bool IsWorkingHours { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OvertimeConfigurationTimeFrame, OvertimeConfigurationTimeFrameModel>()
            .ForMember(m => m.StartTime, v => v.MapFrom(m => m.StartTime.TotalSeconds))
            .ForMember(m => m.EndTime, v => v.MapFrom(m => m.EndTime.TotalSeconds))
            .ReverseMapCustom()
            .ForMember(m => m.StartTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTime)))
            .ForMember(m => m.EndTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTime)));
        }
    }
}