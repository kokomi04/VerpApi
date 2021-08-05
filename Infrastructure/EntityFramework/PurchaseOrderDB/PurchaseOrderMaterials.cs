using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderMaterials
    {
        public long PurchaseOrderMaterialsId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }
    }
}
