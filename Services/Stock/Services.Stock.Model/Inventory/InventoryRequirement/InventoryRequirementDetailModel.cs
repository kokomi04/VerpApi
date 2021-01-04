using AutoMapper;
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
    }

    public class InventoryRequirementDetailOutputModel : InventoryRequirementDetailInputModel
    {
        public ProductUnitConversion ProductUnitConversion { set; get; }
    }
}
