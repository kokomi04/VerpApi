using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ElectronicInvoiceMapping
    {
        public int ElectronicInvoiceMappingId { get; set; }
        public int ElectronicInvoiceProviderId { get; set; }
        public int ElectronicInvoiceFunctionId { get; set; }
        public int VoucherTypeId { get; set; }
        public string MappingFields { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ElectronicInvoiceProvider ElectronicInvoiceProvider { get; set; }
    }
}
