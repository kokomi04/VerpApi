﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock

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
    }

    public class StockOutput : SimpleStockInfo
    {
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Mã Id thủ kho
        /// </summary>
        public int? StockKeeperId { get; set; }

        /// <summary>
        /// Tên thủ kho
        /// </summary>
        public string StockKeeperName { get; set; }

        /// <summary>
        /// Loại
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Id mã tự sinh 
        /// </summary>
        public int? ObjectGenCodeId { get; set; }
    }
}
