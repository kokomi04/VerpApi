using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class SimpleStockInfo
    {
        /// <summary>
        /// Mã kho
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// Tên kho
        /// </summary>
        public string StockName { get; set; }
        public string StockCode { get; set; }
    }
}
