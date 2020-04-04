using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class AccountingAccountService : IAccountingAccountService
    {
        private readonly AccountingDBContext _accountingContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public AccountingAccountService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _accountingContext = accountingContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<AccountingAccountOutputModel>> GetAccountingAccount(int accountingAccountId)
        {
            var accountingAccount = await _accountingContext.AccountingAccount
                .Include(a => a.ParentAccountingAccount)
                .FirstOrDefaultAsync(a => a.AccountingAccountId == accountingAccountId);
            if (accountingAccount == null)
            {
                return AccountingAccountErrorCode.AccountingAccountNotFound;
            }
            AccountingAccountOutputModel accountingAccountModel = _mapper.Map<AccountingAccountOutputModel>(accountingAccount);

            return accountingAccountModel;
        }

        public async Task<PageData<AccountingAccountOutputModel>> GetAccountingAccounts(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.AccountingAccount.Include(a => a.ParentAccountingAccount).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.AccountNumber.Contains(keyword)
                || a.AccountNameVi.Contains(keyword)
                || a.AccountNameEn.Contains(keyword));
            }

            query = query.OrderBy(a => a.SortOrder);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<AccountingAccountOutputModel> lst = new List<AccountingAccountOutputModel>();

            foreach (var item in query)
            {
                AccountingAccountOutputModel accountingAccountModel = _mapper.Map<AccountingAccountOutputModel>(item);
                lst.Add(accountingAccountModel);
            }

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddAccountingAccount(int updatedUserId, AccountingAccountInputModel data)
        {
            var existedAccountingAccount = await _accountingContext.AccountingAccount
                .FirstOrDefaultAsync(a => a.AccountNumber == data.AccountNumber || a.AccountNameVi == data.AccountNameVi || a.AccountNameEn == data.AccountNameEn);
            if (existedAccountingAccount != null)
            {
                if (string.Compare(existedAccountingAccount.AccountNumber, data.AccountNumber, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return AccountingAccountErrorCode.AccountingAccountNumberAlreadyExisted;
                }
                else if (string.Compare(existedAccountingAccount.AccountNameVi, data.AccountNameVi, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return AccountingAccountErrorCode.AccountNameViAlreadyExisted;
                }
                return AccountingAccountErrorCode.AccountNameEnAlreadyExisted;
            }
            // Check parent
            if (data.ParentAccountingAccountId.HasValue)
            {
                if (!_accountingContext.AccountingAccount.Any(a => a.AccountingAccountId == data.ParentAccountingAccountId.Value))
                {
                    return AccountingAccountErrorCode.AccountingAccountParentNotFound;
                }
            }
            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    AccountingAccount accountingAccount = _mapper.Map<AccountingAccount>(data);
                    accountingAccount.UpdatedUserId = updatedUserId;

                    await _accountingContext.AccountingAccount.AddAsync(accountingAccount);
                    await _accountingContext.SaveChangesAsync();

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.AccountingAccount, accountingAccount.AccountingAccountId, $"Thêm tài khoản {accountingAccount.AccountNameVi}", data.JsonSerialize());
                    return accountingAccount.AccountingAccountId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdateAccountingAccount(int updatedUserId, int accountingAccountId, AccountingAccountInputModel data)
        {
            var accountingAccount = await _accountingContext.AccountingAccount
                .Include(a => a.ParentAccountingAccount)
                .FirstOrDefaultAsync(a => a.AccountingAccountId == accountingAccountId);
            if (accountingAccount == null)
            {
                return AccountingAccountErrorCode.AccountingAccountNotFound;
            }
            if (accountingAccount.AccountNumber != data.AccountNumber || accountingAccount.AccountNameVi != data.AccountNameVi || accountingAccount.AccountNameEn != data.AccountNameEn)
            {
                var existedAccountingAccount = await _accountingContext.AccountingAccount
                    .FirstOrDefaultAsync(a => a.AccountingAccountId != accountingAccountId &&
                    (a.AccountNumber == data.AccountNumber
                    || a.AccountNameVi == data.AccountNameVi
                    || a.AccountNameEn == data.AccountNameEn));
                if (existedAccountingAccount != null)
                {
                    if (string.Compare(existedAccountingAccount.AccountNumber, data.AccountNumber, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return AccountingAccountErrorCode.AccountingAccountNumberAlreadyExisted;
                    }
                    else if (string.Compare(existedAccountingAccount.AccountNameVi, data.AccountNameVi, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return AccountingAccountErrorCode.AccountNameViAlreadyExisted;
                    }
                    return AccountingAccountErrorCode.AccountNameEnAlreadyExisted;
                }
            }
            if (data.ParentAccountingAccountId.HasValue && accountingAccount.ParentAccountingAccountId != data.ParentAccountingAccountId)
            {
                if (!_accountingContext.AccountingAccount.Any(a => a.AccountingAccountId == data.ParentAccountingAccountId.Value))
                {
                    return AccountingAccountErrorCode.AccountingAccountParentNotFound;
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    accountingAccount.AccountingAccountId = data.AccountingAccountId;
                    accountingAccount.ParentAccountingAccountId = data.ParentAccountingAccountId;
                    accountingAccount.AccountNumber = data.AccountNumber;
                    accountingAccount.AccountLevel = data.AccountLevel;
                    accountingAccount.AccountNameVi = data.AccountNameVi;
                    accountingAccount.AccountNameEn = data.AccountNameEn;
                    accountingAccount.IsStock = data.IsStock;
                    accountingAccount.IsLiability = data.IsLiability;
                    accountingAccount.IsForeignCurrency = data.IsForeignCurrency;
                    accountingAccount.IsBranch = data.IsBranch;
                    accountingAccount.IsCorp = data.IsCorp;
                    accountingAccount.Currency = data.Currency;
                    accountingAccount.Description = data.Description;
                    accountingAccount.UpdatedUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                   
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.AccountingAccount, accountingAccount.AccountingAccountId, $"Cập nhật tài khoản {accountingAccount.AccountNameVi}", data.JsonSerialize());
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Update");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeleteAccountingAccount(int updatedUserId, int accountingAccountId)
        {
            var accountingAccount = await _accountingContext.AccountingAccount.FirstOrDefaultAsync(a => a.AccountingAccountId == accountingAccountId);
            if (accountingAccount == null)
            {
                return AccountingAccountErrorCode.AccountingAccountNotFound;
            }
            // Check tồn tại sub account
            if (_accountingContext.AccountingAccount.Any(a => a.ParentAccountingAccountId == accountingAccountId))
            {
                return AccountingAccountErrorCode.AccountingAccountChildNotFound;
            }
            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    accountingAccount.IsDeleted = true;
                    accountingAccount.UpdatedUserId = updatedUserId;

                    var deleteFields = _accountingContext.AccountingAccount.Where(a => a.AccountingAccountId == accountingAccountId);
                    foreach (var item in deleteFields)
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.AccountingAccount, accountingAccount.AccountingAccountId, $"Xóa tài khoản {accountingAccount.AccountNameVi}", accountingAccount.JsonSerialize());
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Delete");
                    return GeneralCode.InternalError;
                }
            }
        }
    }
}
