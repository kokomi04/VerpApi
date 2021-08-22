using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentCalendarListModel
    {
        public int DepartmentId { get; set; }
        public ICollection<DepartmentWorkingHourInfoModel> DepartmentWorkingHourInfo { get; set; }
        public ICollection<DepartmentDayOffCalendarModel> DepartmentDayOffCalendar { get; set; }
        public ICollection<DepartmentOverHourInfoModel> DepartmentOverHourInfo { get; set; }
        public DepartmentCalendarListModel()
        {
            DepartmentWorkingHourInfo = new List<DepartmentWorkingHourInfoModel>();
            DepartmentDayOffCalendar = new List<DepartmentDayOffCalendarModel>();
            DepartmentOverHourInfo = new List<DepartmentOverHourInfoModel>();
        }
    }

    public class DepartmentWorkingHourInfoModel : IMapFrom<DepartmentWorkingHourInfo>
    {
        public int DepartmentId { get; set; }
        public double WorkingHourPerDay { get; set; }
        public long StartDate { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentWorkingHourInfo, DepartmentWorkingHourInfoModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.UnixToDateTime()));
        }
    }
}
