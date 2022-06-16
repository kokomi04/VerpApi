﻿namespace VErp.Services.Stock.Model.Location
{
    /// <summary>
    /// Vị trí trong kho
    /// </summary>
    public partial class LocationInput
    {
        /// <summary>
        /// Id vị trí
        /// </summary>
        //public int LocationId { get; set; }

        /// <summary>
        /// Mã kho
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// Tên vị trí
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }
        //public DateTime? CreatedDatetimeUtc { get; set; }
        //public DateTime? UpdatedDatetimeUtc { get; set; }
        //public bool IsDeleted { get; set; }
    }
}
