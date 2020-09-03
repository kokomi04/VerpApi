using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Model.Config;
using VErp.Services.Accountancy.Service.Configuration;

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/accountantconfig")]
    public class AccountantConfigController: VErpBaseController
    {
        private readonly IAccountantConfigService _accountantConfigService;

        public AccountantConfigController(IAccountantConfigService accountantConfigService)
        {
            _accountantConfigService = accountantConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<AccountantConfigModel> GetAccountantConfig()
        {
            return await _accountantConfigService.GetAccountantConfig();
        }

        [HttpPut]
        [Route("configId")]
        public async Task<bool> UpdateAccountantConfig([FromRoute] int configId, [FromBody] AccountantConfigModel accountantConfigModel)
        {
            return await _accountantConfigService.UpdateAccountantConfig(configId, accountantConfigModel);
        }
    }
}
