using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetail
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }

        /// <summary>
        /// Xuất từ kiện hàng nào
        /// </summary>
        public long? FromPackageId { get; set; }

        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }



        public virtual Inventory Inventory { get; set; }
        public virtual Product Product { get; set; }
    }
}
