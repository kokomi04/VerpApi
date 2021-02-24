using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{
    public class ProductionOrderMaterialsCalc
    {
        public long? ProductionOrderMaterialsId { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? AssignmentQuantity { get; set; }
        public int? DepartmentId { get; set; }
        public decimal RateQuantity { get; set; }
        public bool IsReplacement { get; set; }
        public long? ParentId { get; set; }
        public EnumProductionOrderMaterials.EnumInventoryRequirementStatus InventoryRequirementStatusId { get; set; }
    }
}
