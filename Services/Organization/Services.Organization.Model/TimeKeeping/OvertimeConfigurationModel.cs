using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimeConfigurationModel : IMapFrom<OvertimeConfiguration>
    {
        public int OvertimeConfigurationId { get; set; }

        public int? RoundMinutes { get; set; }

        public bool IsRoundBack { get; set; }

        public EnumOvertimeCalculationMode OvertimeCalculationMode { get; set; }

        public int? OvertimeThresholdMins { get; set; }

        public bool IsOvertimeThresholdMins { get; set; }

        public bool IsCalculationThresholdMins { get; set; }

        public int MinsLimitOvertime { get; set; }

        public int MinsReaches { get; set; }

        public int MinsBonusWhenMinsReaches { get; set; }

        public int? MinThresholdMinutesBeforeWork { get; set; }

        public int? MinThresholdMinutesAfterWork { get; set; }

        public bool IsMinThresholdMinutesBeforeWork { get; set; }

        public bool IsMinThresholdMinutesAfterWork { get; set; }

        public bool IsCalculationThresholdMinsBeforeWork { get; set; }

        public bool IsCalculationThresholdMinsAfterWork { get; set; }

        public int MinsLimitOvertimeBeforeWork { get; set; }

        public int MinsLimitOvertimeAfterWork { get; set; }

        public int MinsReachesBeforeWork { get; set; }

        public int MinsReachesAfterWork { get; set; }

        public int MinsBonusWhenMinsReachesBeforeWork { get; set; }

        public int MinsBonusWhenMinsReachesAfterWork { get; set; }

        public virtual IList<OvertimeConfigurationMappingModel> OvertimeConfigurationMapping { get; set; }

        public virtual IList<OvertimeConfigurationTimeFrameModel> OvertimeConfigurationTimeFrame { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OvertimeConfiguration, OvertimeConfigurationModel>()
            .ForMember(m => m.OvertimeConfigurationMapping, v => v.MapFrom(m => m.OvertimeConfigurationMapping))
            .ForMember(m => m.OvertimeConfigurationTimeFrame, v => v.MapFrom(m => m.OvertimeConfigurationTimeFrame))
            .ReverseMapCustom()
            .ForMember(m => m.OvertimeConfigurationMapping, v => v.MapFrom(m => m.OvertimeConfigurationMapping))
            .ForMember(m => m.OvertimeConfigurationTimeFrame, v => v.MapFrom(m => m.OvertimeConfigurationTimeFrame));
        }
    }

    public class OvertimeConfigurationMappingModel : IMapFrom<OvertimeConfigurationMapping>
    {
        public int OvertimeConfigurationId { get; set; }

        public int OvertimeLevelId { get; set; }

        public int MinsLimit { get; set; }
    }
}