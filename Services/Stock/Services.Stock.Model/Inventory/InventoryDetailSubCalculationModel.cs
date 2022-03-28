using System.Collections.Generic;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryDetailSubCalculationModel
    {
        public int InventoryDetailSubCalculationId { get; set; }
        public long InventoryDetailId { get; set; }
        public long ProductBomId { get; set; }
        public int UnitConversionId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
    }

    public class CoupleDataInventoryDetail
    {
        public InventoryDetail Detail { get; set; }
        public IList<InventoryDetailSubCalculation> Subs { get; set; }
    }
}