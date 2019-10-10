using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductModel
    {
        public string ProductCode { get; set;}
        public string ProductName { get; set; }
        public bool IsCanBuy { get; set; }
        public bool IsCanSell { get; set; }
        public long? MainImageMediaId { get; set; }
        public int? ProductTypeId { get; set; }
        public int ProductCateId { get; set; }
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
            public double? TimeWarningAmount { get; set; }
            public EnumTimeType? TimeWarningTimeTypeId { get; set; }
            public string DescriptionToStock { get; set; }

            public IList<int> StockIds { get; set; }

            public IList<ProductModelUnitConversion> UnitConversions { get; set; }
        }

        public class ProductModelUnitConversion
        {
            public int SecondaryUnitId { get; set; }
            public string FactorExpression { get; set; }
            public string ConversionDescription { get; set; }
        }
    }
}
