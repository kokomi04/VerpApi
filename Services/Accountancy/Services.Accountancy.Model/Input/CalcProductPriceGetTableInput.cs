using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProductPriceGetTableInput
    {
        public long FromDate { get; set; } 
        public long ToDate { get; set; }
        public IList<string> GroupColumns { get; set; }
        public NonCamelCaseDictionary<decimal> OtherFee { get; set; }
        public EnumCalcProductPriceAllocationCriteria CP_NVL_GT_TCPB_ID { get; set; }
        public EnumCalcProductPriceAllocationCriteria CP_NHANC_GT_TCPB_ID { get; set; }
        public EnumCalcProductPriceAllocationCriteria CP_SXCHUNG_TCPB_ID { get; set; }
    }
}
