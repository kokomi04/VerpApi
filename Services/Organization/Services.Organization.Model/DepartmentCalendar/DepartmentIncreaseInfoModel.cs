using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentIncreaseInfoModel : IMapFrom<DepartmentIncreaseInfo>
    {
        public long DepartmentIncreaseInfoId { get; set; }
        public int DepartmentId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentIncreaseInfo, DepartmentIncreaseInfoModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.EndDate.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.UnixToDateTime()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.EndDate.UnixToDateTime()));
        }
    }
}
