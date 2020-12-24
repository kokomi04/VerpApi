using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProductPriceGetTableInput
    {
        public int? ProductId { get; set; }
        public string OrderCode { get; set; }
        public string MaLsx { get; set; }
        public int? StockId { get; set; }

        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public bool IsByLsx { get; set; }
        public bool IsByOrder { get; set; }
        public bool IsByStock { get; set; }

        public NonCamelCaseDictionary<decimal> AllocationRate { get; set; }
        public NonCamelCaseDictionary<decimal> CustomPrice { get; set; }
        public NonCamelCaseDictionary<decimal> DirectMaterialFee { get; set; }
        public NonCamelCaseDictionary<decimal> DirectLaborFee { get; set; }
        public NonCamelCaseDictionary<decimal> OtherFee { get; set; }

        public EnumCalcProductPriceAllocationType IndirectMaterialFeeAllocationTypeId { get; set; }
        public decimal? IndirectMaterialFeeSumCustom { get; set; }

        public EnumCalcProductPriceAllocationType IndirectLaborFeeAllocationTypeId { get; set; }
        public decimal? IndirectLaborFeeSumCustom { get; set; }

        public EnumCalcProductPriceAllocationType GeneralManufacturingAllocationTypeId { get; set; }
        public decimal? GeneralManufacturingSumCustom { get; set; }

        public bool IsReviewUpdate { get; set; }
        public bool IsUpdate { get; set; }
    }

    public class CalcProductPriceGetTableOutput
    {
        public IList<NonCamelCaseDictionary> Data { get; set; }
        public decimal? IndirectMaterialFeeSum { get; set; }
        public decimal? IndirectLaborFeeSum { get; set; }
        public decimal? GeneralManufacturingSum { get; set; }
    }
}
