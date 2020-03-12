using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggestDetail
    {
        public long PurchasingSuggestDetailId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public int? CustomerId { get; set; }       
        public string PurchasingRequestCode { get; set; }
        public int ProductId { get; set; }
        public int PrimaryUnitId { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public decimal PrimaryQuantity { get; set; }
        
        /// <summary>
        /// Đơn giá
        /// </summary>
        public decimal? PrimaryUnitPrice { get; set; }
        
        
        public decimal? Tax { get; set; }
        public DateTime? CreatedDatetimeUtc { get; set; }
        public DateTime? UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
