using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Product
    {
        public Product()
        {
            ProductStockValidation = new HashSet<ProductStockValidation>();
            ProductUnitConversion = new HashSet<ProductUnitConversion>();
        }

        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool IsCanBuy { get; set; }
        public bool IsCanSell { get; set; }
        public long? MainImageMediaId { get; set; }
        public int ProductCateId { get; set; }
        public int ProductIdentityCodeId { get; set; }
        public int? BarcodeStandardId { get; set; }
        public string Barcode { get; set; }
        public int UnitId { get; set; }
        public decimal? EstimatePrice { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ProductCate ProductCate { get; set; }
        public virtual ProductIdentityCode ProductIdentityCode { get; set; }
        public virtual ProductExtraInfo ProductExtraInfo { get; set; }
        public virtual ProductStockInfo ProductStockInfo { get; set; }
        public virtual ICollection<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual ICollection<ProductUnitConversion> ProductUnitConversion { get; set; }
    }
}
