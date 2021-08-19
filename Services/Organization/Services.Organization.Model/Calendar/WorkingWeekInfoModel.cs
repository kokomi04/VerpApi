using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Calendar
{
    public class WorkingWeekInfoModel : IMapFrom<WorkingWeekInfo>
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsDayOff { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<WorkingWeekInfo, WorkingWeekInfoModel>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(x => (DayOfWeek)x.DayOfWeek))
                .ReverseMap()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(x => (int)x.DayOfWeek));
        }
    }
}