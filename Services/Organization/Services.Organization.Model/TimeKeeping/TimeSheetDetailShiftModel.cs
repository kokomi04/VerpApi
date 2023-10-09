using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailShiftModel : IMapFrom<TimeSheetDetailShift>
    {
        public long TimeSheetDetailId { get; set; }

        public int ShiftConfigurationId { get; set; }

        public int CountedSymbolId { get; set; }

        public double? TimeIn { get; set; }

        public double? TimeOut { get; set; }

        public int? AbsenceTypeSymbolId { get; set; }

        public long? MinsLate { get; set; }

        public long? MinsEarly { get; set; }

        public int? OvertimeLevelId { get; set; }

        public long? MinsOvertime { get; set; }

        public int? OvertimeLevelId2 { get; set; }

        public long? MinsOvertime2 { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetDetailShiftModel, TimeSheetDetailShift>()
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeIn.Value) : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeOut.Value) : null))
            .ReverseMapCustom()
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (double?)m.TimeIn.Value.TotalSeconds : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (double?)m.TimeOut.Value.TotalSeconds : null));
        }
    }
}
