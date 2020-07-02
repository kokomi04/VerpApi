using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public interface IProductModel
    {
        public int? ProductId { get; set; }

        string ProductCode { get; set; }

        string ProductName { get; set; }
        bool IsCanBuy { get; set; }
        bool IsCanSell { get; set; }
        long? MainImageFileId { get; set; }
        int? ProductTypeId { get; set; }
        int ProductCateId { get; set; }
        int? BarcodeConfigId { get; set; }
        EnumBarcodeStandard? BarcodeStandardId { get; set; }
        string Barcode { get; set; }
        int UnitId { get; set; }
        decimal? EstimatePrice { get; set; }

        IProductModelExtra Extra { get; set; }
        IProductModelStock StockInfo { get; set; }
    }

    public interface IProductModelExtra
    {
        string Specification { get; set; }
        string Description { get; set; }
    }

    public interface IProductModelStock
    {
        EnumStockOutputRule? StockOutputRuleId { get; set; }
        long? AmountWarningMin { get; set; }
        long? AmountWarningMax { get; set; }
        double? TimeWarningAmount { get; set; }
        EnumTimeType? TimeWarningTimeTypeId { get; set; }
        double? ExpireTimeAmount { get; set; }
        EnumTimeType? ExpireTimeTypeId { get; set; }
        string DescriptionToStock { get; set; }

        IList<int> StockIds { get; set; }

        IList<IProductModelUnitConversion> UnitConversions { get; set; }
    }

    public interface IProductModelUnitConversion
    {
        int ProductUnitConversionId { get; set; }
        string ProductUnitConversionName { get; set; }
        bool IsDefault { get; set; }
        int SecondaryUnitId { get; set; }
        string FactorExpression { get; set; }
        string ConversionDescription { get; set; }
    }
}
