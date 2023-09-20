using AutoMapper;
using System;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class ShiftConfigurationModel : IMapFrom<ShiftConfiguration>
    {
        public int ShiftConfigurationId { get; set; }
        public int? OvertimeConfigurationId { get; set; }
        public string ShiftCode { get; set; }
        public string Description { get; set; }
        public double EntryTime { get; set; }
        public double ExitTime { get; set; }
        public bool IsNightShift { get; set; }
        public bool? IsCheckOutDateTimekeeping { get; set; }
        public double? LunchTimeStart { get; set; }
        public double? LunchTimeFinish { get; set; }
        public long ConvertToMins { get; set; }
        public decimal ConfirmationUnit { get; set; }
        public double StartTimeOnRecord { get; set; }
        public double EndTimeOnRecord { get; set; }
        public double StartTimeOutRecord { get; set; }
        public double EndTimeOutRecord { get; set; }
        public bool IsSkipSaturdayWithShift { get; set; }
        public bool IsSkipSundayWithShift { get; set; }
        public bool IsSkipHolidayWithShift { get; set; }
        public bool IsCountWorkForHoliday { get; set; }
        public EnumPartialShiftCalculationMode PartialShiftCalculationMode { get; set; }

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
        public int? MaxLateMins { get; set; }
        public int? MaxEarlyMins { get; set; }
        public bool IsExceededLateAbsenceType { get; set; }
        public bool IsExceededEarlyAbsenceType { get; set; }
        public int? ExceededLateAbsenceTypeId { get; set; }
        public int? ExceededEarlyAbsenceTypeId { get; set; }
        public int? NoEntryTimeAbsenceTypeId { get; set; }
        public int? NoExitTimeAbsenceTypeId { get; set; }
        public bool IsNoEntryTimeWorkMins { get; set; }
        public bool IsNoExitTimeWorkMins { get; set; }
        public long? NoEntryTimeWorkMins { get; set; }
        public long? NoExitTimeWorkMins { get; set; }

        public OvertimeConfigurationModel OvertimeConfiguration { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ShiftConfiguration, ShiftConfigurationModel>()
            .ForMember(m => m.EntryTime, v => v.MapFrom(m => m.EntryTime.TotalSeconds))
            .ForMember(m => m.ExitTime, v => v.MapFrom(m => m.ExitTime.TotalSeconds))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => m.LunchTimeStart != null ? m.LunchTimeStart.Value.TotalSeconds : (double?)null))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => m.LunchTimeFinish != null ? m.LunchTimeFinish.Value.TotalSeconds : (double?)null))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => m.StartTimeOnRecord.TotalSeconds))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => m.EndTimeOutRecord.TotalSeconds))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => m.EndTimeOnRecord.TotalSeconds))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => m.StartTimeOutRecord.TotalSeconds))
            .ForMember(m => m.OvertimeConfiguration, v => v.MapFrom(m => m.OvertimeConfiguration))
            .ReverseMapCustom()
            .ForMember(m => m.EntryTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EntryTime)))
            .ForMember(m => m.ExitTime, v => v.MapFrom(m => TimeSpan.FromSeconds(m.ExitTime)))
            .ForMember(m => m.LunchTimeStart, v => v.MapFrom(m => m.LunchTimeStart != null ? TimeSpan.FromSeconds(m.LunchTimeStart.Value) : (TimeSpan?)null))
            .ForMember(m => m.LunchTimeFinish, v => v.MapFrom(m => m.LunchTimeFinish != null ? TimeSpan.FromSeconds(m.LunchTimeFinish.Value) : (TimeSpan?)null))
            .ForMember(m => m.StartTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOnRecord)))
            .ForMember(m => m.EndTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOutRecord)))
            .ForMember(m => m.EndTimeOnRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOnRecord)))
            .ForMember(m => m.StartTimeOutRecord, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOutRecord)))
            .ForMember(m => m.OvertimeConfiguration, v => v.MapFrom(m => m.OvertimeConfiguration));
        }
    }
}