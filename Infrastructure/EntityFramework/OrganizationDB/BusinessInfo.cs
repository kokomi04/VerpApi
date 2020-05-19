using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class BusinessInfo
    {
        public int BusinessInfoId { get; set; }
        public string CompanyName { get; set; }
        public string LegalRepresentative { get; set; }
        public string Address { get; set; }
        public string TaxIdNo { get; set; }
        public string Website { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int? LogoFileId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int UpdatedUserId { get; set; }
    }
}
