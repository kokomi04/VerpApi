using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Constants.Caching
{
    public static class ProductionOrderCacheKeys
    {
        public static readonly string CACHE_CALC_PRODUCTION_ORDER_STATUS = "CACHE_CALC_PRODUCTION_ORDER_STATUS";
        public static string CalcProductionOrderStatusPending(string productionOrderCode)
        {
            return $"CALC_PRODUCTION_ORDER_STATUS_PENDING_{productionOrderCode?.ToUpper()}";
        }
    }
}
