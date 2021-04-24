using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementDetailInputModel : IMapFrom<InventoryRequirementDetail>
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

        public long? OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
    }

    public class InventoryRequirementDetailOutputModel : InventoryRequirementDetailInputModel
    {
        public ProductUnitConversion ProductUnitConversion { set; get; }
        public IList<InventorySimpleInfo> InventoryInfo { set; get; }
        public decimal InventoryQuantity { get; set; }
        public InventoryRequirementDetailOutputModel()
        {
            InventoryInfo = new List<InventorySimpleInfo>();
        }
    }

    public class InventorySimpleInfo
    {
        public string InventoryCode { get; set; }
        public long InventoryId { get; set; }
    }
}
