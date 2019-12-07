using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockWarning
    {
        public int StockId { get; set; }
        public string StockName { get; set; }
        public IList<StockWarningDetail> Warnings { get; set; }
    }
    public class StockWarningDetail
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public EnumWarningType StockWarningTypeId { get; set; }
        public string PackageCode { get; set; }
    }
}
