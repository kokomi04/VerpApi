using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Accountancy.Model.Config;

namespace VErp.Services.Accountancy.Service.Configuration
{
    public interface IAccountantConfigService
    {
        Task<AccountantConfigModel> GetAccountantConfig();
        Task<bool> UpdateAccountantConfig(AccountantConfigModel accountantConfigModel);
    }
}
