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
    public class MonthPlanModel : IMapFrom<MonthPlan>
    {

        public int MonthPlanId { get; set; }
        public string MonthPlanName { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string MonthNote { get; set; }
        public IList<WeekPlanModel> WeekPlans { get; set; }

        public MonthPlanModel()
        {
            WeekPlans = new List<WeekPlanModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<MonthPlan, MonthPlanModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                 .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
                 .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.UnixToDateTime()));
        }
    }

}
