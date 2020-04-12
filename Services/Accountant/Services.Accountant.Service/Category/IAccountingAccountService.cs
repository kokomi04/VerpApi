using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface IAccountingAccountService
    {
        Task<ServiceResult<AccountingAccountOutputModel>> GetAccountingAccount(int accountingAccountId);
        Task<PageData<AccountingAccountOutputModel>> GetAccountingAccounts(string keyword, int page, int size);
        Task<ServiceResult<int>> AddAccountingAccount(int updatedUserId, AccountingAccountInputModel data);
        Task<Enum> UpdateAccountingAccount(int updatedUserId, int accountingAccountId, AccountingAccountInputModel data);
        Task<Enum> DeleteAccountingAccount(int updatedUserId, int accountingAccountId);
    }
}
