using System;
using System.Collections.Generic;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class ShiftConfigurationModel : IMapFrom<ShiftConfiguration>
    {
        public int ShiftConfigurationId { get; set; }
        public string ShiftCode { get; set; }
        public long BeginDate { get; set; }
        public long EndDate { get; set; }
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
            profile.CreateMap<ShiftConfiguration, ShiftConfigurationModel>()
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.GetUnix()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.GetUnix()))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => m.LunchTimeStart.TotalMinutes))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => m.LunchTimeFinish.TotalMinutes))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => m.StartTimeOnRecord.TotalMinutes))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => m.EndTimeOutRecord.TotalMinutes))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => m.EndTimeOnRecord.TotalMinutes))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => m.StartTimeOutRecord.TotalMinutes))
            .ForMember(m => m.OvertimeConfiguration, v => v.Ignore())
            .ReverseMap()
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.UnixToDateTime()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.UnixToDateTime()))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => TimeSpan.FromMinutes(m.LunchTimeStart)))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => TimeSpan.FromMinutes(m.LunchTimeFinish)))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromMinutes(m.StartTimeOnRecord)))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromMinutes(m.EndTimeOutRecord)))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromMinutes(m.EndTimeOnRecord)))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromMinutes(m.StartTimeOutRecord)))
            .ForMember(m => m.OvertimeConfiguration, v => v.Ignore());
        }
    }
}