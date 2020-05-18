using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class CustomerBankAccount
    {
        public int CustomerBankAccountId { get; set; }
        public int CustomerId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string SwiffCode { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
