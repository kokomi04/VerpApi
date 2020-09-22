using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Config;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Configuration.Implement
{
    public class AccountantConfigService : IAccountantConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyContext;
        private readonly ICurrentContextService _currentContextService;

        public AccountantConfigService(AccountancyDBContext accountancyContext
            , ILogger<AccountantConfigService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IMapper mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
        }

        public async Task<AccountantConfigModel> GetAccountantConfig()
        {
            var config = await _accountancyContext.AccountantConfig
                .FirstOrDefaultAsync(x => x.SubsidiaryId == _currentContextService.SubsidiaryId);

            AccountantConfigModel result;
            if (config != null)
                result = new AccountantConfigModel
                {
                    SubsidiaryId = config.SubsidiaryId,
                    ClosingDate = config.ClosingDate.GetUnix(),
                    AutoClosingDate = config.AutoClosingDate,
                    FreqClosingDate = config.FreqClosingDate.JsonDeserialize<FreqClosingDate>()
                };
            else result = new AccountantConfigModel { SubsidiaryId = _currentContextService.SubsidiaryId };

            return result;
        }

        public async Task<bool> UpdateAccountantConfig(int keyId, AccountantConfigModel accountantConfigModel)
        {
            using (var trans = await _accountancyContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var config = _accountancyContext.AccountantConfig.FirstOrDefault(x => x.SubsidiaryId == _currentContextService.SubsidiaryId);
                    if (config != null)
                    {
                        config.ClosingDate = Utils.UnixToDateTime(accountantConfigModel.ClosingDate).Value;
                        config.AutoClosingDate = accountantConfigModel.AutoClosingDate;
                        config.FreqClosingDate = accountantConfigModel.FreqClosingDate.JsonSerialize();
                        _accountancyContext.AccountantConfig.Update(config);

                    }
                    else
                    {
                        await _accountancyContext.AccountantConfig.AddAsync(new AccountantConfig
                        {
                            SubsidiaryId = _currentContextService.SubsidiaryId,
                            AutoClosingDate = accountantConfigModel.AutoClosingDate,
                            FreqClosingDate = accountantConfigModel.FreqClosingDate.JsonSerialize(),
                            ClosingDate = Utils.UnixToDateTime(accountantConfigModel.ClosingDate).Value
                        });
                    }

                    await _accountancyContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.AccountantConfig, keyId, $"Cập nhật thông số hệ thống", accountantConfigModel.JsonSerialize());

                    return true;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }
        }


    }
}
