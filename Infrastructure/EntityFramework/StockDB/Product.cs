using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class Product
{
    public int ProductId { get; set; }

    public int SubsidiaryId { get; set; }

    /// <summary>
    /// Mã sản phẩm
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Tên nội bộ
    /// </summary>
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

    public string SellDescription { get; set; }

    public string ProductNameEng { get; set; }

    public decimal? Quantitative { get; set; }

    public int? QuantitativeUnitTypeId { get; set; }

    public bool IsProductSemi { get; set; }

    /// <summary>
    /// Cơ số sản phẩm
    /// </summary>
    public decimal Coefficient { get; set; }

    public bool? IsProduct { get; set; }

    public string Color { get; set; }

    public bool? IsMaterials { get; set; }

    public decimal? PackingQuantitative { get; set; }

    public decimal? PackingWidth { get; set; }

    public decimal? PackingLong { get; set; }

    public decimal? PackingHeight { get; set; }

    public long? ProductionProcessVersion { get; set; }

    public decimal? ProductPurity { get; set; }

    public int? TargetProductivityId { get; set; }

    public int ProductionProcessStatusId { get; set; }

    public string AccountNumber { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetail { get; set; } = new List<InventoryDetail>();

    public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; } = new List<InventoryRequirementDetail>();

    public virtual ICollection<ProductAttachment> ProductAttachment { get; set; } = new List<ProductAttachment>();

    public virtual ICollection<ProductBom> ProductBomChildProduct { get; set; } = new List<ProductBom>();

    public virtual ICollection<ProductBom> ProductBomProduct { get; set; } = new List<ProductBom>();

    public virtual ProductCate ProductCate { get; set; }

    public virtual ICollection<ProductCustomer> ProductCustomer { get; set; } = new List<ProductCustomer>();

    public virtual ProductExtraInfo ProductExtraInfo { get; set; }

    public virtual ICollection<ProductIgnoreStep> ProductIgnoreStepProduct { get; set; } = new List<ProductIgnoreStep>();

    public virtual ICollection<ProductIgnoreStep> ProductIgnoreStepRootProduct { get; set; } = new List<ProductIgnoreStep>();

    public virtual ICollection<ProductMaterial> ProductMaterialProduct { get; set; } = new List<ProductMaterial>();

    public virtual ICollection<ProductMaterial> ProductMaterialRootProduct { get; set; } = new List<ProductMaterial>();

    public virtual ICollection<ProductMaterialsConsumption> ProductMaterialsConsumptionMaterialsConsumption { get; set; } = new List<ProductMaterialsConsumption>();

    public virtual ICollection<ProductMaterialsConsumption> ProductMaterialsConsumptionProduct { get; set; } = new List<ProductMaterialsConsumption>();

    public virtual ICollection<ProductProperty> ProductPropertyProduct { get; set; } = new List<ProductProperty>();

    public virtual ICollection<ProductProperty> ProductPropertyRootProduct { get; set; } = new List<ProductProperty>();

    public virtual ProductStockInfo ProductStockInfo { get; set; }

    public virtual ICollection<ProductStockValidation> ProductStockValidation { get; set; } = new List<ProductStockValidation>();

    public virtual ProductType ProductType { get; set; }

    public virtual ICollection<ProductUnitConversion> ProductUnitConversion { get; set; } = new List<ProductUnitConversion>();
}
