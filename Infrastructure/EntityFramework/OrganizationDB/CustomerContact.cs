using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class CustomerContact
    {
        public int CustomerContactId { get; set; }
        public int SubsidiaryId { get; set; }
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public int? GenderId { get; set; }
        public string Position { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
