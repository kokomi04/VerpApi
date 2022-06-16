using System;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderOutsourceMapping
    {
        public long PurchaseOrderOutsourceMappingId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long OutsourcePartRequestId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
    }
}
