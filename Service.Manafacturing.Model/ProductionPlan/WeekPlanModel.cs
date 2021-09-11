using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionPlan
{
    public class WeekPlanModel: IMapFrom<WeekPlan>
    {

        public int WeekPlanId { get; set; }
        public int MonthPlanId { get; set; }
        public string WeekPlanName { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string WeekNote { get; set; }

        public WeekPlanModel()
        {
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<WeekPlan, WeekPlanModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                 .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
                 .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.UnixToDateTime()));
        }
    }

}
