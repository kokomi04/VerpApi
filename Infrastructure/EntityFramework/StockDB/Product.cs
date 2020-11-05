using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Product
    {
        public Product()
        {
            InventoryDetail = new HashSet<InventoryDetail>();
            ProductAttachmentAttachment = new HashSet<ProductAttachment>();
            ProductAttachmentProduct = new HashSet<ProductAttachment>();
            ProductBomChildProduct = new HashSet<ProductBom>();
            ProductBomProduct = new HashSet<ProductBom>();
            ProductStockValidation = new HashSet<ProductStockValidation>();
            ProductUnitConversion = new HashSet<ProductUnitConversion>();
        }

        public int ProductId { get; set; }
        public int SubsidiaryId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductInternalName { get; set; }
        public bool IsCanBuy { get; set; }
        public bool IsCanSell { get; set; }
        public long? MainImageFileId { get; set; }
        public int? ProductTypeId { get; set; }
        public int ProductCateId { get; set; }
        public int? BarcodeStandardId { get; set; }
        public int? BarcodeConfigId { get; set; }
        public string Barcode { get; set; }
        public int UnitId { get; set; }
        public decimal? EstimatePrice { get; set; }
        public decimal? Long { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int ProductStatusId { get; set; }
        public string PackingMethod { get; set; }
        public int? CustomerId { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? Measurement { get; set; }
        public decimal? LoadAbility { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNameEng { get; set; }
        public decimal? Quantitative { get; set; }
        public int? QuantitativeUnitTypeId { get; set; }

        public virtual ProductCate ProductCate { get; set; }
        public virtual ProductType ProductType { get; set; }
        public virtual ProductExtraInfo ProductExtraInfo { get; set; }
        public virtual ProductStockInfo ProductStockInfo { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetail { get; set; }
        public virtual ICollection<ProductAttachment> ProductAttachmentAttachment { get; set; }
        public virtual ICollection<ProductAttachment> ProductAttachmentProduct { get; set; }
        public virtual ICollection<ProductBom> ProductBomChildProduct { get; set; }
        public virtual ICollection<ProductBom> ProductBomProduct { get; set; }
        public virtual ICollection<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual ICollection<ProductUnitConversion> ProductUnitConversion { get; set; }
    }
}
