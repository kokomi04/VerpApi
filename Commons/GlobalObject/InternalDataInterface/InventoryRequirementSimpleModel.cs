using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using static VErp.Commons.Enums.Manafacturing.EnumProductionOrderMaterials;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InventoryRequirementSimpleModel
    {
        public string InventoryRequirementCode { get; set; }
        public string Content { get; set; }
        public long Date { get; set; }
        public int? DepartmentId { get; set; }
        public int CreatedByUserId { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? ProductionStepId { get; set; }
        public string Shipper { get; set; }
        public int? CustomerId { get; set; }
        public string BillForm { get; set; }
        public string BillCode { get; set; }
        public string BillSerial { get; set; }
        public long BillDate { get; set; }
        public EnumModuleType ModuleTypeId { get; set; }
        public EnumInventoryRequirementType InventoryRequirementTypeId { get; set; }
        public EnumInventoryOutsideMappingType InventoryOutsideMappingTypeId { get; set; }

        public ICollection<InventoryRequirementSimpleDetailModel> InventoryRequirementDetail { get; set; }

    }

    public class InventoryRequirementSimpleDetailModel
    {
        public long InventoryRequirementDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public string Pocode { get; set; }
        public string ProductionOrderCode { get; set; }
        public int? SortOrder { get; set; }
        public int? AssignStockId { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }
        public string OrderCode { get; set; }
        public int? DepartmentId { get; set; }
        public long? ProductionStepId { get; set; }
    }
}
