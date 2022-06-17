using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductAttachment
    {
        public long ProductAttachmentId { get; set; }
        public int ProductId { get; set; }
        public long AttachmentFileId { get; set; }
        public string Title { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Product Product { get; set; }
    }
}
