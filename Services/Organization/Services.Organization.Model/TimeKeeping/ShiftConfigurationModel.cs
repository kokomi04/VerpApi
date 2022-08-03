using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class ShiftConfigurationModel : IMapFrom<ShiftConfiguration>
    {
        public int ShiftConfigurationId { get; set; }
        public int? OvertimeConfigurationId { get; set; }
        public string ShiftCode { get; set; }
        public double BeginDate { get; set; }
        public double EndDate { get; set; }
        public int NumberOfTransition { get; set; }
        public double LunchTimeStart { get; set; }
        public double LunchTimeFinish { get; set; }
        public long ConvertToMins { get; set; }
        public decimal ConfirmationUnit { get; set; }
        public double StartTimeOnRecord { get; set; }
        public double EndTimeOnRecord { get; set; }
        public double StartTimeOutRecord { get; set; }
        public double EndTimeOutRecord { get; set; }
        public long MinsWithoutTimeOn { get; set; }
        public long MinsWithoutTimeOut { get; set; }
        public int PositionOnReport { get; set; }

        public bool IsSubtractionForLate { get; set; }
        public bool IsSubtractionForEarly { get; set; }
        public long MinsAllowToLate { get; set; }
        public long MinsAllowToEarly { get; set; }
        public bool IsCalculationForLate { get; set; }
        public bool IsCalculationForEarly { get; set; }
        public long MinsRoundForLate { get; set; }
        public long MinsRoundForEarly { get; set; }
        public bool IsRoundBackForLate { get; set; }
        public bool IsRoundBackForEarly { get; set; }

        public OvertimeConfigurationModel OvertimeConfiguration { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<ShiftConfiguration, ShiftConfigurationModel>()
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.TotalSeconds))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.TotalSeconds))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => m.LunchTimeStart.TotalSeconds))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => m.LunchTimeFinish.TotalSeconds))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => m.StartTimeOnRecord.TotalSeconds))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => m.EndTimeOutRecord.TotalSeconds))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => m.EndTimeOnRecord.TotalSeconds))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => m.StartTimeOutRecord.TotalSeconds))
            .ForMember(m => m.OvertimeConfiguration, v => v.Ignore())
            .ReverseMapIgnoreNoneExist()
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => TimeSpan.FromSeconds(m.BeginDate)))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndDate)))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => TimeSpan.FromSeconds(m.LunchTimeStart)))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => TimeSpan.FromSeconds(m.LunchTimeFinish)))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOnRecord)))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOutRecord)))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOnRecord)))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOutRecord)))
            .ForMember(m => m.OvertimeConfiguration, v => v.Ignore());
        }
    }
}