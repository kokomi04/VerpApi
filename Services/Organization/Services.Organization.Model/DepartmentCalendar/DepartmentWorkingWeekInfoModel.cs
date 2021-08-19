using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.DepartmentCalendar
{
    public class DepartmentWorkingWeekInfoModel : IMapFrom<DepartmentWorkingWeekInfo>
    {
        public int DepartmentId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsDayOff { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentWorkingWeekInfo, DepartmentWorkingWeekInfoModel>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(x => (DayOfWeek)x.DayOfWeek))
                .ReverseMap()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(x => (int)x.DayOfWeek));
        }
    }
}