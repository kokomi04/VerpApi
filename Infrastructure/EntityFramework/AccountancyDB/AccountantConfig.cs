using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class AccountantConfig
    {
        public int Id { get; set; }
        public DateTime ClosingDate { get; set; }
    }
}
