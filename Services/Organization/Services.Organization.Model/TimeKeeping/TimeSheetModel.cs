using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetModel : IMapFrom<TimeSheet>
    {
        public long TimeSheetId { get; set; }
        public long EmployeeId { get; set; }
        public long Date { get; set; }
        public double TimeIn { get; set; }
        public double TimeOut { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TimeSheetModel, TimeSheet>()
            .ForMember(m=>m.Date, v=>v.MapFrom(m=>m.Date.UnixToDateTime()))
            .ForMember(m=>m.TimeIn, v=>v.MapFrom(m=>TimeSpan.FromSeconds(m.TimeIn)))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => TimeSpan.FromSeconds(m.TimeOut)))
            .ReverseMap()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.TotalSeconds))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.TotalSeconds));
        }
    }
}
