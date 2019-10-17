﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Stock
    {
        public int StockId { get; set; }
        
        /// <summary>
        /// Tên kho
        /// </summary>
        public string StockName { get; set; }

        // <summary>
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

        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
