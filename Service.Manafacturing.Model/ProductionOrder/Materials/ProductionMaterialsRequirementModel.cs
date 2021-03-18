using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
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
        public EnumProductionMaterialsRequirementStatus CensorStatus { get; set; }
        public int CreatedByUserId { get; set; }
        public long? CreatedDatetimeUtc { get; set; }

        public IList<ProductionMaterialsRequirementDetailModel> MaterialsRequirementDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionMaterialsRequirement, ProductionMaterialsRequirementModel>()
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.GetUnix()))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m=>m.MaterialsRequirementDetails, v=>v.MapFrom(m=>m.ProductionMaterialsRequirementDetail))
                .ForMember(m=>m.ProductionOrderCode, v=>v.MapFrom(m=>m.ProductionOrder.ProductionOrderCode))
                .ReverseMap()
                .ForMember(m => m.ProductionMaterialsRequirementDetail, v => v.Ignore())
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.ProductionOrder, v => v.Ignore())
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.UnixToDateTime()));
        }
    }


    public class ProductionMaterialsRequirementDetailListModel : IMapFrom<ProductionMaterialsRequirementDetail>
    {
        public long? RequirementDate { get; set; }
        public string RequirementContent { get; set; }
        public int CensorStatus { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionMaterialsRequirementDetail, ProductionMaterialsRequirementDetailListModel>()
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.ProductionMaterialsRequirement.RequirementDate.GetUnix()))
                .ForMember(m => m.RequirementContent, v => v.MapFrom(m => m.ProductionMaterialsRequirement.RequirementContent))
                .ForMember(m => m.CensorStatus, v => v.MapFrom(m => m.ProductionMaterialsRequirement.CensorStatus))
                .ForMember(m => m.ProductionOrderId, v => v.MapFrom(m => m.ProductionMaterialsRequirement.ProductionOrderId));
        }

    }
}
