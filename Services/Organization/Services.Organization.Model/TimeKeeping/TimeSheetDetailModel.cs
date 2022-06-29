using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailModel : IMapFrom<TimeSheetDetail>
    {
        public long TimeSheetDetailId { get; set; }
        public long TimeSheetId { get; set; }
        public long EmployeeId { get; set; }
        public long Date { get; set; }
        public double? TimeIn { get; set; }
        public double? TimeOut { get; set; }
        public int? AbsenceTypeSymbolId { get; set; }
        public long? MinsOvertime { get; set; }
        public long? MinsLate { get; set; }
        public long? MinsEarly { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<TimeSheetDetailModel, TimeSheetDetail>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeIn.Value) : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(m.TimeOut.Value) : null))
            .ReverseMap()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.HasValue ? (double?)m.TimeIn.Value.TotalSeconds : null))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.HasValue ? (double?)m.TimeOut.Value.TotalSeconds : null));
        }
    }
}
