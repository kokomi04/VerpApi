using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class OpeningBalanceModel
    {
        public string CateName { set; get; }

        public string CatePrefixCode { set; get; }

        public string ProductCode { set; get; }

        public string ProductName { set; get; }

        public string Unit1 { set; get; }

        public decimal Qty1 { set; get; }

        public decimal UnitPrice { set; get; }

        public string Specification { set; get; }

        public string Unit2 { set; get; }

       
        public decimal Qty2 { set; get; }

        /// <summary>
        /// Tỷ lệ
        /// </summary>
        public decimal Factor { set; get; }

        public decimal Height { set; get; }

        public decimal Width { set; get; }

        public decimal Long { set; get; }
    }
}
