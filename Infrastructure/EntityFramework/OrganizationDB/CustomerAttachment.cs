using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class CustomerAttachment
    {
        public long CustomerAttachmentId { get; set; }
        public int CustomerId { get; set; }
        public long AttachmentFileId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
