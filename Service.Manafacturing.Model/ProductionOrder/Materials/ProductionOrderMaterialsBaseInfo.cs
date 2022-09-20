using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionOrderMaterials;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{
    public class ProductionOrderMaterialsBaseInfo : IMapFrom<ProductionOrderMaterials>
    {
        public long ProductionOrderMaterialsId { get; set; }
        public long ProductionOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal ConversionRate { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public long? ParentId { get; set; }
        public bool IsReplacement { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public EnumInventoryRequirementStatus InventoryRequirementStatusId { get; set; }
        public long? ProductionOrderMaterialSetId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
    }

    public class ProductionOrderMaterialsInput : ProductionOrderMaterialsBaseInfo
    {
        public ProductionOrderMaterialsInput()
        {
            materialsReplacement = new List<ProductionOrderMaterialsInput>();
        }

        public IList<ProductionOrderMaterialsInput> materialsReplacement { get; set; }

    }

    public class ProductionOrderMaterialsOutput : ProductionOrderMaterialsBaseInfo
    {
    }
}
