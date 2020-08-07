using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class PrintConfigService : IPrintConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICurrentContextService _currentContextService;

        public PrintConfigService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _currentContextService = currentContextService;
        }

        public async Task<PrintConfigModel> GetPrintConfig(int printConfigId)
        {
            var printConfig = await _accountancyDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (printConfig == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }
            return printConfig;
        }

        public async Task<ICollection<PrintConfigModel>> GetPrintConfigs(int inputTypeId)
        {
            var lst = await _accountancyDBContext.PrintConfig
                .Where(p => p.InputTypeId == inputTypeId)
                .OrderBy(p => p.Title)
                .ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return lst;
        }

        public async Task<int> AddPrintConfig(PrintConfigModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.InputTypeId));
            if (!_accountancyDBContext.InputType.Any(i => i.InputTypeId == data.InputTypeId))
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            if (_accountancyDBContext.PrintConfig.Any(p => p.PrintConfigName == data.PrintConfigName))
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNameAlreadyExisted);
            }

            try
            {
                PrintConfig config = _mapper.Map<PrintConfig>(data);
                await _accountancyDBContext.PrintConfig.AddAsync(config);
                await _accountancyDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.InputTypeId, $"Thêm cấu hình phiếu in chứng từ {config.PrintConfigName} ", data.JsonSerialize());

                return config.PrintConfigId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.InputTypeId));
            var config = await _accountancyDBContext.PrintConfig.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
            if (config == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }
            if (_accountancyDBContext.PrintConfig.Any(p => p.PrintConfigId != printConfigId && p.PrintConfigName == data.PrintConfigName))
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNameAlreadyExisted);
            }
            try
            {
                config.InputTypeId = data.InputTypeId;
                config.PrintConfigName = data.PrintConfigName;
                config.Title = data.Title;
                config.BodyTable = data.BodyTable;
                config.GenerateCode = data.GenerateCode;
                config.PaperSize = data.PaperSize;
                config.Layout = data.Layout;
                config.HeadTable = data.HeadTable;
                config.FootTable = data.FootTable;
                config.StickyFootTable = data.StickyFootTable;
                config.StickyHeadTable = data.StickyHeadTable;
                config.HasTable = data.HasTable;
                config.Background = data.Background;
                await _accountancyDBContext.SaveChangesAsync();


                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Cập nhật cấu hình phiếu in chứng từ {config.PrintConfigName}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeletePrintConfig(int printConfigId)
        {
            var config = await _accountancyDBContext.PrintConfig.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
            if (config == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }

            config.IsDeleted = true;
            await _accountancyDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InventoryInput, config.PrintConfigId, $"Xóa cấu hình phiếu in chứng từ {config.PrintConfigName}", config.JsonSerialize());
            return true;
        }
    }
}

