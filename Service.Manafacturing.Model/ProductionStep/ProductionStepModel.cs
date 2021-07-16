using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using ProductionStepEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionStep;


namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepModel : IMapFrom<ProductionStepEntity>
    {
        public long ProductionStepId { get; set; }
        public int? StepId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long? ParentId { get; set; }
        public string ParentCode { get; set; }
        public EnumProductionProcess.EnumContainerType ContainerTypeId { get; set; }
        public long ContainerId { get; set; }
        public int SortOrder { get; set; }
        public bool? IsGroup { get; set; }
        public decimal? CoordinateX { get; set; }
        public decimal? CoordinateY { get; set; }
        public string ProductionStepCode { get; set; }
        public int UnitId { get; set; }
        public bool IsFinish { get; set; }
        public decimal? ShrinkageRate { get; set; }
        public EnumHandoverTypeStatus? HandoverTypeId { get; set; }
        public long? OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepEntity, ProductionStepModel>()
                .ForMember(m => m.Title, a => a.MapFrom(s => string.IsNullOrEmpty(s.Title) ? s.Step == null ? null : s.Step.StepName : s.Title))
                .ForMember(m => m.Description, a => a.MapFrom(s => s.Step == null ? null : s.Step.Description))
                .ForMember(m => m.ShrinkageRate, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.ShrinkageRate : 0))
                .ForMember(m => m.HandoverTypeId, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.HandoverTypeId : (int)EnumHandoverTypeStatus.Push))
                .ForMember(m => m.UnitId, a => a.MapFrom(s => s.StepId.HasValue? s.Step.UnitId : 0))
                .ForMember(m => m.OutsourceStepRequestCode, a => a.MapFrom(s => s.OutsourceStepRequest.OutsourceStepRequestCode))
                .ReverseMap()
                .ForMember(m => m.Step, v => v.Ignore())
                .ForMember(m => m.OutsourceStepRequest, v => v.Ignore());
        }
    }

    public class ProductionStepInfo : ProductionStepModel
    {
        public List<ProductionStepLinkDataInfo> ProductionStepLinkDatas { get; set; }
        public new void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepEntity, ProductionStepInfo>()
                .ForMember(m => m.ProductionStepLinkDatas, a => a.MapFrom(s => s.ProductionStepLinkDataRole))
                .ForMember(m => m.Title, a => a.MapFrom(s => string.IsNullOrEmpty(s.Title) ? s.Step == null ? null : s.Step.StepName : s.Title))
                .ForMember(m => m.UnitId, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.UnitId : 0))
                .ForMember(m => m.ShrinkageRate, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.ShrinkageRate : 0))
                .ForMember(m => m.HandoverTypeId, a => a.MapFrom(s => s.StepId.HasValue ? s.Step.HandoverTypeId : (int)EnumHandoverTypeStatus.Push))
                .ForMember(m => m.OutsourceStepRequestCode, a => a.MapFrom(s => s.OutsourceStepRequest.OutsourceStepRequestCode))
                .ReverseMap()
                .ForMember(m => m.ProductionStepLinkDataRole, a => a.Ignore())
                .ForMember(m => m.Step, v => v.Ignore());
        }
    }

    public class ProductionStepSortOrderModel
    {
        public long ProductionStepId { get; set; }
        public int SortOrder { get; set; }
    }
}
