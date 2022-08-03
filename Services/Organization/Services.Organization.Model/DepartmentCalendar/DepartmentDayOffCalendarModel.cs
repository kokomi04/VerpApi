//using AutoMapper;
//using System;
//using VErp.Commons.GlobalObject;
//using VErp.Commons.Library;
//using VErp.Infrastructure.EF.OrganizationDB;

//namespace VErp.Services.Organization.Model.DepartmentCalendar
//{
//    public class DepartmentDayOffCalendarModel : IMapFrom<DepartmentDayOffCalendar>
//    {
//        public int DepartmentId { get; set; }
//        public long Day { get; set; }
//        public string Content { get; set; }
//        public void Mapping(Profile profile)
//        {
//            profile.CreateMapIgnoreNoneExist<DepartmentDayOffCalendar, DepartmentDayOffCalendarModel>()
//                .ForMember(dest => dest.Day, opt => opt.MapFrom(x =>x.Day.GetUnix()))
//                .ReverseMapIgnoreNoneExist()
//                .ForMember(dest => dest.Day, opt => opt.MapFrom(x => x.Day.UnixToDateTime()));
//        }
//    }
//}