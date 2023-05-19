using AutoMapper;
using System;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionInventoryRequirementBaseModel
    {
        public int ProductId { get; set; }
        public int CreatedByUserId { get; set; }
        public decimal RequirementQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public int? AssignStockId { get; set; }
        public string StockName { get; set; }
        public long? ProductionStepId { get; set; }
        public int? DepartmentId { get; set; }
        public string Content { get; set; }
        public string InventoryCode { get; set; }
        public long? InventoryId { get; set; }
        public long? InventoryDetailId { get; set; }
        public long? OutsourceStepRequestId { get; set; }
        public string InventoryRequirementCode { get; set; }
        public long? InventoryRequirementId { get; set; }
        public long? InventoryRequirementDetailId { get; set; }
    }

    public class ProductionInventoryRequirementModel : ProductionInventoryRequirementBaseModel, IMapFrom<InternalProductionInventoryRequirementModel>
    {
        public EnumProductionInventoryRequirementStatus Status { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public EnumInventoryType InventoryTypeId { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMapCustom<InternalProductionInventoryRequirementModel, ProductionInventoryRequirementModel>()
                .ForMember(m => m.InventoryTypeId, v => v.MapFrom(m => m.InventoryTypeId))
                .ForMember(m => m.Status, v => v.MapFrom(m => (EnumProductionInventoryRequirementStatus)m.Status))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()));
        }
    }

}
