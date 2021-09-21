using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InternalProductListOutput
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public long? MainImageFileId { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductTypeCode { get; set; }
        public string ProductTypeName { get; set; }
        public int ProductCateId { get; set; }
        public string ProductCateName { get; set; }
        public string BarcodeConfigName { get; set; }
        public string Barcode { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal? EstimatePrice { get; set; }
        public bool IsProductSemi { get; set; }
        public bool IsProduct { get; set; }
        public bool IsMaterials { get; set; }
        public int Coefficient { get; set; }
        public decimal? Long { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public int DecimalPlace { get; set; }

        //public int? CustomerId { get; set; }

        public string PackingMethod { get; set; }

        public decimal? Quantitative { get; set; }

        public EnumQuantitativeUnitType? QuantitativeUnitTypeId { get; set; }

        public decimal? Measurement { get; set; }

        public decimal? NetWeight { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? LoadAbility { get; set; }


        public EnumStockOutputRule? StockOutputRuleId { get; set; }

        public long? AmountWarningMin { get; set; }

        public long? AmountWarningMax { get; set; }

        public double? ExpireTimeAmount { get; set; }

        public EnumTimeType? ExpireTimeTypeId { get; set; }


        public List<InternalStockProductOutput> StockProductModelList { set; get; }

        public IList<InternalProductModelUnitConversion> ProductUnitConversions { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }

    public class InternalStockProductOutput
    {
        public int StockId { set; get; }

        public int ProductId { set; get; }

        public int PrimaryUnitId { set; get; }

        public decimal PrimaryQuantityRemaining { set; get; }

        public int? ProductUnitConversionId { set; get; }

        public decimal ProductUnitConversionRemaining { set; get; }
    }

    public class InternalProductModelUnitConversion
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
}
