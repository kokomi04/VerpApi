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
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Config;

namespace VErp.Services.Accountancy.Service.Configuration.Implement
{
    public class AccountantConfigService : IAccountantConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyContext;

        public AccountantConfigService(AccountancyDBContext accountancyContext
            , ILogger<AccountantConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _mapper = mapper;
        }

        public async Task<AccountantConfigModel> GetAccountantConfig()
        {
            var config = await _accountancyContext.AccountantConfig
                .OrderByDescending(x => x.Id)
                .ProjectTo<AccountantConfigModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return config;
        }

        public async Task<bool> UpdateAccountantConfig(int keyId, AccountantConfigModel accountantConfigModel)
        {
            using (var trans = await _accountancyContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var config = _mapper.Map<AccountantConfig>(accountantConfigModel);
                    _accountancyContext.AccountantConfig.Update(config);

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
