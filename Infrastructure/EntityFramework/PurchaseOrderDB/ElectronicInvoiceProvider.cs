using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ElectronicInvoiceProvider
    {
        public int ElectronicInvoiceProviderId { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int ElectronicInvoiceProviderStatusId { get; set; }
        public string ConnectionConfig { get; set; }
        public string FieldsConfig { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
