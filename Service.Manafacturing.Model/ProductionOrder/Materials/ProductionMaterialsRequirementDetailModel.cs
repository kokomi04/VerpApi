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
    public class ProductionMaterialsRequirementDetailModel: IMapFrom<ProductionMaterialsRequirementDetail>
    {
        public long ProductionMaterialsRequirementDetailId { get; set; }
        public long ProductionMaterialsRequirementId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionMaterialsRequirementDetail, ProductionMaterialsRequirementDetailModel>()
                .ForMember(m => m.ProductionStepTitle, v => v.MapFrom(m => string.Concat(m.ProductionStep.Step.StepName, $" (#{m.ProductionStepId})")))
                .ReverseMap()
                .ForMember(m => m.ProductionStep, v => v.Ignore());
        }
    }

    public class ProductionMaterialsRequirementDetailSearch : ProductionMaterialsRequirementDetailExtrackBase, IMapFrom<ProductionMaterialsRequirementDetailExtrackInfo>
    {
        public long? RequirementDate { get; set; }

        public new  void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionMaterialsRequirementDetailExtrackInfo, ProductionMaterialsRequirementDetailSearch>()
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.RequirementDate, v => v.MapFrom(m => m.RequirementDate.UnixToDateTime()));
        }
    }

    public class ProductionMaterialsRequirementDetailExtrackInfo: ProductionMaterialsRequirementDetailExtrackBase
    {
        public DateTime? RequirementDate { get; set; }

    }

    public class ProductionMaterialsRequirementDetailExtrackBase: ProductionMaterialsRequirementDetailModel
    {
        public string RequirementCode { get; set; }
        public string RequirementContent { get; set; }
        public int CreatedByUserId { get; set; }
        public string DepartmentTitle { get; set; }
        public string ProductTitle { get; set; }
        public int UnitId { get; set; }
        public EnumProductionMaterialsRequirementStatus CensorStatus { get; set; }

    }
}
