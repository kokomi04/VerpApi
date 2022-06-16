﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionPlan
{
    public class ProductionWeekPlanModel : IMapFrom<ProductionWeekPlan>
    {
        public long StartDate { get; set; }
        public decimal? ProductQuantity { get; set; }
        public ICollection<ProductionWeekPlanDetailModel> ProductionWeekPlanDetail { get; set; }

        public ProductionWeekPlanModel()
        {
            ProductionWeekPlanDetail = new List<ProductionWeekPlanDetailModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionWeekPlan, ProductionWeekPlanModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
                .ForMember(dest => dest.ProductionWeekPlanDetail, opt => opt.Ignore());
        }
    }

}
