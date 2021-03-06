using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionInventoryRequirementBaseModel
    {
        public long InventoryRequirementDetailId { get; set; }
        public int ProductId { get; set; }
        public int CreatedByUserId { get; set; }
        public decimal RequirementQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public int? AssignStockId { get; set; }
        public string StockName { get; set; }
        public long ProductionStepId { get; set; }
        public int? DepartmentId { get; set; }
        public string Content { get; set; }
    }

    public class ProductionInventoryRequirementModel : ProductionInventoryRequirementBaseModel, IMapFrom<ProductionInventoryRequirementEntity>
    {
        public EnumProductionInventoryRequirementStatus Status { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public EnumInventoryType InventoryTypeId { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionInventoryRequirementEntity, ProductionInventoryRequirementModel>()
                .ForMember(m => m.InventoryTypeId, v => v.MapFrom(m => (EnumInventoryType)m.InventoryTypeId))
                .ForMember(m => m.Status, v => v.MapFrom(m => (EnumProductionInventoryRequirementStatus)m.Status))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()));
        }
    }

    public class ProductionInventoryRequirementEntity : ProductionInventoryRequirementBaseModel
    {
        public int Status { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int InventoryTypeId { get; set; }
    }
}
