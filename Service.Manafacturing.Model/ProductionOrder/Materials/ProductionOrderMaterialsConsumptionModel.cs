using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{
    public class ProductionOrderMaterialsConsumptionModel: IMapFrom<ProductionOrderMaterialsConsumption>
    {
        public long ProductionOrderMaterialsConsumptionId { get; set; }
        public long ProductionOrderId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public long ProductId { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal Quantity { get; set; }
        public int? DepartmentId { get; set; }
        public EnumProductionOrderMaterials.EnumInventoryRequirementStatus InventoryRequirementStatusId { get; set; }
        public long? ParentId { get; set; }
        public bool IsReplacement { get; set; }
        public int UnitId { get; set; }
    }
}
