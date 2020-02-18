using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryDetailAffectModel
    {
        public long InventoryId { get; set; }
        public string InventoryCode { get; set; }
        public long InventoryDetailId { get; set; }
        public long? ToPackageId { get; set; }
        public long? FromPackageId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public DateTime Date { get; set; }
    }
}
