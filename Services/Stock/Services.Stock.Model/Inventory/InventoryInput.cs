using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryInput
    {
        //public long InventoryId { get; set; }

        public int StockId { get; set; }

        /// <summary>
        /// Mã code phiếu nhập / xuất kho
        /// </summary>
        public string InventoryCode { get; set; }
        
        /// <summary>
        /// Loại
        /// </summary>
        public int InventoryTypeId { get; set; }
        public string Shipper { get; set; }
        public string Content { get; set; }
        public DateTime DateUtc { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? UserId { get; set; }

        /// <summary>
        /// Tệp tin chứng từ liên quan
        /// </summary>
        public long? InvoiceFileId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        //public DateTime CreatedDatetimeUtc { get; set; }
        //public DateTime UpdatedDatetimeUtc { get; set; }
        //public bool IsDeleted { get; set; }

        public List<InventoryDetailInput> InventoryDetailInputList { set; get; }
    }
}
