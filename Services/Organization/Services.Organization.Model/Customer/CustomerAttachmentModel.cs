using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerAttachmentModel: IMapFrom<CustomerAttachment>
    {
        public long CustomerAttachmentId { get; set; }
        public int CustomerId { get; set; }
        public long AttachmentFileId { get; set; }
        public string Title { get; set; }
    }
}
