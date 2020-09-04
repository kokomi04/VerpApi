using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Config
{
    public class AccountantConfigModel
    {
        public int Id { get; set; }
        public long ClosingDate { get; set; }
    }

}
