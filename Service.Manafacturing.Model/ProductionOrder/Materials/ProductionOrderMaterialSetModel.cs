using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{


    public class ProductionOrderMaterialSetModel
    {
        public long ProductionOrderMaterialSetId { get; set; }
        //public EnumInventoryRequirementStatus InventoryRequirementStatusId { get; set; }
        public string Title { get; set; }        
        public IList<int> ProductMaterialsConsumptionGroupIds { get; set; }
        //public int ProductionOrderMaterialSetTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public IList<ProductionOrderMaterialAssign> Materials { get; set; }

    }

    public class ProductionOrderMaterialGroupStandardModel
    {
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public IList<ProductionOrderMaterialStandard> Materials { get; set; }

    }

    public class ProductionOrderMaterialStandard
    {
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? DepartmentId { get; set; }
        public long? ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public int? StepId { get; set; }
        public string StepName { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
        public decimal RateQuantity { get; set; }
    }

    public class ProductionOrderMaterialAssign : ProductionOrderMaterialStandard, IMapFrom<ProductionOrderMaterialStandard>
    {
        public long? ProductionOrderMaterialsId { get; set; }
        public long? ParentId { get; set; }
        public decimal? AssignmentQuantity { get; set; }
        public decimal ConversionRate { get; set; }
        public bool IsReplacement { get; set; }
    }

    public class ProductionOrderMaterialInfo
    {
        public bool IsReset { get; set; }
        public IList<ProductionOrderMaterialGroupStandardModel> Standards { get; set; }
        public IList<ProductionOrderMaterialSetModel> Sets { get; set; }
    }
}
