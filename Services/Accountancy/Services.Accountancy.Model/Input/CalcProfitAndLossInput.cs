using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Data;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProfitAndLossInput: IFilterHashData
    {
        public int? ProductId { get; set; }
        public string OrderCode { get; set; }
        public string MaLsx { get; set; }

        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public bool IsByLsx { get; set; }
        public bool IsByOrder { get; set; }

        public NonCamelCaseDictionary<decimal> Custom_AllocationRate { get; set; }
        public NonCamelCaseDictionary<decimal> Custom_PriceSellDirectly { get; set; }
        public NonCamelCaseDictionary<decimal> Custom_CostSellDirectly { get; set; }
        public NonCamelCaseDictionary<decimal> Custom_CostManagerDirectly { get; set; }
        public NonCamelCaseDictionary<decimal> Custom_OtherFee { get; set; }

        public EnumCalcProfitAndLossAllocation PriceSellInDirectlyAllocationTypeId { get; set; }
        public decimal? PriceSellInDirectlySumCustom { get; set; }

        public EnumCalcProfitAndLossAllocation CostAccountingAllocationTypeId { get; set; }
        public decimal? CostAccountingSumCustom { get; set; }


        public EnumCalcProfitAndLossAllocation CostSellInDirectlyAllocationTypeId { get; set; }
        public decimal? CostSellInDirectlySumCustom { get; set; }

        public EnumCalcProfitAndLossAllocation CostManagerAllowcationAllocationTypeId { get; set; }
        public decimal? CostManagerSumCustom { get; set; }

        public bool IsSave { get; set; }
        public string Title { get; set; }
        public string Descirption { get; set; }

        public string GetHashString()
        {
            return $"{ProductId}_{OrderCode}_{MaLsx}_{FromDate}_{ToDate}_{IsByLsx}_{IsByOrder}";
        }
    }

    public class CalcProfitAndLossTableOutput
    {
        public IList<NonCamelCaseDictionary> Data { get; set; }
        public decimal? PriceSellInDirectlySum { get; set; }
        public decimal? CostAccountingSum { get; set; }
        public decimal? CostSellInDirectlySum { get; set; }
        public decimal? CostManagerSum { get; set; }

        public long? CalcPeriodId { get; set; }
    }

    public class CalcProfitAndLossView
    {
        public CalcPeriodListModel CalcPeriodInfo { get; set; }
        public CalcProfitAndLossInput FilterData { get; set; }
        public CalcProfitAndLossTableOutput OutputData { get; set; }
    }
}
