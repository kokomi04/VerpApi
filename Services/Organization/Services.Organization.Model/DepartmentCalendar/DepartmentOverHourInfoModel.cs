﻿using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentOverHourInfoModel : IMapFrom<DepartmentOverHourInfo>
    {
        public long DepartmentOverHourInfoId { get; set; }
        public int DepartmentId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public double OverHour { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<DepartmentOverHourInfo, DepartmentOverHourInfoModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.EndDate.GetUnix()))
                .ReverseMapCustom()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.StartDate.UnixToDateTime()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.EndDate.UnixToDateTime()));
        }
    }
}
