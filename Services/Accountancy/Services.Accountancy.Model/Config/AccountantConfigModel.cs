using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Config
{
    public class AccountantConfigModel: IMapFrom<AccountantConfig>
    {
        public int Id { get; set; }
        public DateTime? ClosingDate { get; set; }
    }

    public class AccountantConfigFieldModel
    {
        public string fieldName { get; set; }
        public AccountantConfigModel accountantConfig{ get; set; }
    }
    
}
