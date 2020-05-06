using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class AccountingAccount
    {
        public AccountingAccount()
        {
            InverseParentAccountingAccount = new HashSet<AccountingAccount>();
        }

        public int AccountingAccountId { get; set; }
        public int? ParentAccountingAccountId { get; set; }
        public string AccountNumber { get; set; }
        public int AccountLevel { get; set; }
        public string AccountNameVi { get; set; }
        public string AccountNameEn { get; set; }
        public int SortOrder { get; set; }
        public bool IsStock { get; set; }
        public bool IsLiability { get; set; }
        public bool IsForeignCurrency { get; set; }
        public bool IsBranch { get; set; }
        public bool IsCorp { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedUserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual AccountingAccount ParentAccountingAccount { get; set; }
        public virtual ICollection<AccountingAccount> InverseParentAccountingAccount { get; set; }
    }
}
