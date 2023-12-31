﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class ProductModel : ProductGenCodeModel
    {
        public int? ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MaxLength(128, ErrorMessage = "Tên sản phẩm quá dài")]
        public string ProductName { get; set; }
        public bool IsCanBuy { get; set; }
        public bool IsCanSell { get; set; }
        public long? MainImageFileId { get; set; }
        public int ProductCateId { get; set; }
        public int? BarcodeConfigId { get; set; }
        public EnumBarcodeStandard? BarcodeStandardId { get; set; }
        public string Barcode { get; set; }
        public int UnitId { get; set; }
        public decimal? EstimatePrice { get; set; }
        public decimal? Long { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string PackingMethod { get; set; }
        //public int? CustomerId { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? Measurement { get; set; }
        public decimal? LoadAbility { get; set; }
        public string SellDescription { get; set; }
        public string ProductNameEng { get; set; }

        public decimal? Quantitative { get; set; }
        public EnumQuantitativeUnitType? QuantitativeUnitTypeId { get; set; }
        public decimal? ProductPurity { get; set; }

        public bool IsProductSemi { get; set; }
        public bool? IsProduct { get; set; }
        public bool? IsMaterials { get; set; }
        public EnumProductionProcessStatus ProductionProcessStatusId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Coefficient { get; set; }
        public string Color { get; set; }

        public int? TargetProductivityId { get; set; }

        public decimal? PackingQuantitative { get; set; }
        public decimal? PackingWidth { get; set; }
        public decimal? PackingLong { get; set; }
        public decimal? PackingHeight { get; set; }

        public long? ProductionProcessVersion { get; set; }


        public IList<ProductModelCustomer> ProductCustomers { get; set; }
        public ProductModelExtra Extra { get; set; }
        public ProductModelStock StockInfo { get; set; }
        public string UnitName { get; set; }

        public class ProductModelExtra
        {
            public string Specification { get; set; }
            public string Description { get; set; }
        }

        public class ProductModelStock
        {
            public EnumStockOutputRule? StockOutputRuleId { get; set; }
            public long? AmountWarningMin { get; set; }
            public long? AmountWarningMax { get; set; }
            /// <summary>
            /// Thời gian cảnh báo lưu kho
            /// </summary>
            public double? TimeWarningAmount { get; set; }
            public EnumTimeType? TimeWarningTimeTypeId { get; set; }
            /// <summary>
            /// Hạn sử dụng
            /// </summary>
            public double? ExpireTimeAmount { get; set; }
            public EnumTimeType? ExpireTimeTypeId { get; set; }
            public string DescriptionToStock { get; set; }

            public IList<int> StockIds { get; set; }

            public IList<ProductModelUnitConversion> UnitConversions { get; set; }
        }

        public class ProductModelUnitConversion
        {
            public int ProductUnitConversionId { get; set; }
            public string ProductUnitConversionName { get; set; }
            public int SecondaryUnitId { get; set; }
            public bool IsDefault { get; set; }
            public bool IsFreeStyle { get; set; }
            public string FactorExpression { get; set; }
            public string ConversionDescription { get; set; }
            public int DecimalPlace { get; set; }
        }

        public class ProductModelCustomer
        {
            public int? CustomerId { get; set; }
            public string CustomerProductCode { get; set; }
            public string CustomerProductName { get; set; }
            public string CustomerProductBarcode { get; set; }
            public string CustomerProductModelType { get; set; }
            public string CustomerProductDescription { get; set; }

        }
    }

    public class ProductDefaultModel : ProductGenCodeModel
    {
        public int? ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MaxLength(128, ErrorMessage = "Tên sản phẩm quá dài")]
        public string ProductName { get; set; }
        public int UnitId { get; set; }
        public string Specification { get; set; }
    }

    public class ProductGenCodeModel
    {
        public string ProductCode { get; set; }
        public int? ProductTypeId { get; set; }
    }
}

