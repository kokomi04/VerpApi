using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryDetailOutput
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }
        
        public int? PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int? ProductUnitConversionId { set; get; }

        public decimal? ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }

        public long? FromPackageId { set; get; }

        public long? ToPackageId { set; get; }

        public int? PackageOptionId { set; get; }

        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }

        public int? SortOrder { get; set; }

        public string Description { get; set; }

        public ProductListOutput ProductOutput { get; set; }

        public ProductUnitConversion ProductUnitConversion { set; get; }
    }
}
