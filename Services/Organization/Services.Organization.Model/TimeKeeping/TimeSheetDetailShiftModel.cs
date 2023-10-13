using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailShiftModel : IMapFrom<TimeSheetDetailShift>
    {
        public long TimeSheetDetailId { get; set; }

        public int ShiftConfigurationId { get; set; }

        public double? TimeIn { get; set; }

        public double? TimeOut { get; set; }

        public int? AbsenceTypeSymbolId { get; set; }

        public long? MinsLate { get; set; }

        public long? MinsEarly { get; set; }

        public long? ActualWorkMins { get; set; }

        public int? DateAsOvertimeLevelId { get; set; }

        public bool HasOvertimePlan { get; set; }

        public IList<TimeSheetDetailShiftCountedModel> TimeSheetDetailShiftCounted { get; set; } = new List<TimeSheetDetailShiftCountedModel>();

        public IList<TimeSheetDetailShiftOvertimeModel> TimeSheetDetailShiftOvertime { get; set; } = new List<TimeSheetDetailShiftOvertimeModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetDetailShiftModel, TimeSheetDetailShift>()
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeIn.Value) : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeOut.Value) : null))
            .ForMember(m => m.TimeSheetDetailShiftCounted, v => v.MapFrom(m => m.TimeSheetDetailShiftCounted))
            .ForMember(m => m.TimeSheetDetailShiftOvertime, v => v.MapFrom(m => m.TimeSheetDetailShiftOvertime))
            .ReverseMapCustom()
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (double?)m.TimeIn.Value.TotalSeconds : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (double?)m.TimeOut.Value.TotalSeconds : null))
            .ForMember(m => m.TimeSheetDetailShiftCounted, v => v.MapFrom(m => m.TimeSheetDetailShiftCounted))
            .ForMember(m => m.TimeSheetDetailShiftOvertime, v => v.MapFrom(m => m.TimeSheetDetailShiftOvertime));
        }
    }
}
