using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Accountant.Service.Category;
using VErp.Services.Accountant.Model.Category;
using System.Collections.Generic;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/accountingaccounts")]

    public class AccountingAccountController : VErpBaseController
    {
        private readonly IAccountingAccountService _accountingAccountService;
        public AccountingAccountController(IAccountingAccountService accountingAccountService)
        {
            _accountingAccountService = accountingAccountService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<AccountingAccountOutputModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _accountingAccountService.GetAccountingAccounts(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddAccountingAccount([FromBody] AccountingAccountInputModel accountingAccount)
        {
            var updatedUserId = UserId;
            return await _accountingAccountService.AddAccountingAccount(updatedUserId, accountingAccount);
        }

        [HttpGet]
        [Route("{accountingAccountId}")]
        public async Task<ServiceResult<AccountingAccountOutputModel>> GetAccountingAccount([FromRoute] int accountingAccountId)
        {
            return await _accountingAccountService.GetAccountingAccount(accountingAccountId);
        }

        [HttpPut]
        [Route("{accountingAccountId}")]
        public async Task<ServiceResult> UpdateAccountingAccount([FromRoute] int accountingAccountId, [FromBody] AccountingAccountInputModel accountingAccount)
        {
            var updatedUserId = UserId;
            return await _accountingAccountService.UpdateAccountingAccount(updatedUserId, accountingAccountId, accountingAccount);
        }

        [HttpDelete]
        [Route("{accountingAccountId}")]
        public async Task<ServiceResult> DeleteCategory([FromRoute] int accountingAccountId)
        {
            var updatedUserId = UserId;
            return await _accountingAccountService.DeleteAccountingAccount(updatedUserId, accountingAccountId);
        }
    }
}