using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionStepEnity = VErp.Infrastructure.EF.ManufacturingDB.ProductionStep;


namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepModel : IMapFrom<ProductionStepEnity>
    {
        public long ProductionStepId { get; set; }
        public int? StepId { get; set; }
        public string Title { get; set; }
        public long? ParentId { get; set; }
        public string ParentCode { get; set; }
        public EnumProductionProcess.EnumContainerType ContainerTypeId { get; set; }
        public long ContainerId { get; set; }
        public decimal? Workload { get; set; }
        public int SortOrder { get; set; }
        public bool? IsGroup { get; set; }
        public decimal? CoordinateX { get; set; }
        public decimal? CoordinateY { get; set; }
        public string ProductionStepCode { get; set; }
        public int UnitId { get; set; }
        public bool IsFinish { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepEnity, ProductionStepModel>()
                .ForMember(m => m.Title, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.StepName : s.Title))
                .ForMember(m => m.UnitId, a => a.MapFrom(s => s.Step.UnitId))
                .ReverseMap()
                .ForMember(m => m.Step, v => v.Ignore());
        }
    }

    public class ProductionStepInfo : ProductionStepModel
    {
        public List<ProductionStepLinkDataInfo> ProductionStepLinkDatas { get; set; }
        public new void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepEnity, ProductionStepInfo>()
                .ForMember(m => m.ProductionStepLinkDatas, a => a.MapFrom(s => s.ProductionStepLinkDataRole))
                .ForMember(m => m.Title, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.StepName : s.Title))
                .ForMember(m => m.UnitId, a => a.MapFrom(s => s.Step.UnitId))
                .ReverseMap()
                .ForMember(m => m.ProductionStepLinkDataRole, a => a.Ignore())
                .ForMember(m => m.Step, v => v.Ignore());
        }
    }

    public class PorductionStepSortOrderModel
    {
        public long ProductionStepId { get; set; }
        public int SortOrder { get; set; }
    }
}
