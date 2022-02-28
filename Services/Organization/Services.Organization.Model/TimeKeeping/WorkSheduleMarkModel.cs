using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class WorkScheduleMarkModel:IMapFrom<WorkScheduleMark>
    {
        public int WorkScheduleMarkId { get; set; }
        public int EmployeeId { get; set; }
        public int WorkScheduleId { get; set; }
        public long BeginDate { get; set; }
        public long ExpiryDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<WorkScheduleMarkModel, WorkScheduleMark>()
            .ForMember(m=>m.BeginDate, v=>v.MapFrom(m=>m.BeginDate.UnixToDateTime()))
            .ForMember(m=>m.ExpiryDate, v=>v.MapFrom(m=>m.ExpiryDate.UnixToDateTime()))
            .ReverseMap()
            .ForMember(m=>m.BeginDate, v=>v.MapFrom(m=>m.BeginDate.GetUnix()))
            .ForMember(m=>m.ExpiryDate, v=>v.MapFrom(m=>m.ExpiryDate.GetUnix()));
        }
    }
}