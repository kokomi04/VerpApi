using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RefProduct
    {
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
        public string SellDescription { get; set; }
        public string ProductNameEng { get; set; }
        public decimal? Quantitative { get; set; }
        public int? QuantitativeUnitTypeId { get; set; }
    }
}
