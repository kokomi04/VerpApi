﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{
    public class ProductionMaterialsRequirementModel: IMapFrom<ProductionMaterialsRequirement>
    {
        public long ProductionMaterialsRequirementId { get; set; }
        public string RequirementCode { get; set; }
        public long? RequirementDate { get; set; }
        public string RequirementContent { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public int? CensorByUserId { get; set; }
        public int CensorStatus { get; set; }
        public int CreatedByUserId { get; set; }

        public IList<ProductionMaterialsRequirementDetailModel> MaterialsRequirementDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionMaterialsRequirement, ProductionMaterialsRequirementModel>()
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.GetUnix()))
                .ForMember(m=>m.MaterialsRequirementDetails, v=>v.MapFrom(m=>m.ProductionMaterialsRequirementDetail))
                .ForMember(m=>m.ProductionOrderCode, v=>v.MapFrom(m=>m.ProductionOrder.ProductionOrderCode))
                .ReverseMap()
                .ForMember(m => m.ProductionMaterialsRequirementDetail, v => v.Ignore())
                .ForMember(m => m.ProductionOrder, v => v.Ignore())
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.UnixToDateTime()));
        }
    }
}
