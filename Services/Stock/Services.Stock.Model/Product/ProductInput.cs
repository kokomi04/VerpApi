using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm")]
        [MaxLength(128, ErrorMessage = "Mã sản phẩm quá dài")]
        public string ProductCode { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MaxLength(128, ErrorMessage = "Tên sản phẩm quá dài")]
        public string ProductName { get; set; }
        public bool IsCanBuy { get; set; }
        public bool IsCanSell { get; set; }
        public long? MainImageFileId { get; set; }
        public int? ProductTypeId { get; set; }
        public int ProductCateId { get; set; }
        public int? BarcodeConfigId { get; set; }
        public EnumBarcodeStandard? BarcodeStandardId { get; set; }
        public string Barcode { get; set; }
        public int UnitId { get; set; }
        public decimal? EstimatePrice { get; set; }

        public ProductModelExtra Extra { get; set; }
        public ProductModelStock StockInfo { get; set; }

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
            [Required(ErrorMessage = "Vui lòng nhập quy cách đơn vị chuyển đổi")]
            [MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi quá dài")]
            public string ProductUnitConversionName { get; set; }
            public int SecondaryUnitId { get; set; }
            public string FactorExpression { get; set; }
            public string ConversionDescription { get; set; }
        }
    }
}
