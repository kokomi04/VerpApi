using AutoMapper;
using VErp.Commons.Enums.Organization;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Calendar
{
    public class DayOffCalendarModel : IMapFrom<DayOffCalendar>
    {
        public long Day { get; set; }
        public string Content { get; set; }
        public EnumDayOffType? DayOffType { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<DayOffCalendar, DayOffCalendarModel>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(x => x.Day.GetUnix()))
                .ReverseMapCustom()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(x => x.Day.UnixToDateTime()));
        }
    }
}