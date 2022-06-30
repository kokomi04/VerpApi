using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderAttachment
    {
        public int ProductionOrderAttachmentId { get; set; }
        public long ProductionOrderId { get; set; }
        public string Title { get; set; }
        public long AttachmentFileId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductionOrder ProductionOrder { get; set; }
    }
}
