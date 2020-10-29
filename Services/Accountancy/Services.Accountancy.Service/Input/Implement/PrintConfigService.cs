using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
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
        private readonly AppSetting _appSetting;
        private readonly IDocOpenXmlService _docOpenXmlService;
        private readonly IPhysicalFileService _physicalFileService;

        public PrintConfigService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            , IPhysicalFileService physicalFileService
            , IDocOpenXmlService docOpenXmlService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _appSetting = appSetting.Value;
            _docOpenXmlService = docOpenXmlService;
            _physicalFileService = physicalFileService;
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

        public async Task<ICollection<PrintConfigModel>> GetPrintConfigs(int moduleTypeId, int activeForId)
        {
            var query = _accountancyDBContext.PrintConfig.AsQueryable()
                .Where(p => p.ModuleTypeId == moduleTypeId);

            if (activeForId > 0)
            {
                query = query.Where(p => p.ActiveForId == activeForId);
            }
            var lst = await query.OrderBy(p => p.Title)
                .ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return lst;
        }

        public async Task<int> AddPrintConfig(PrintConfigModel data)
        {
            //using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.ActiveForId));
            //if (!_accountancyDBContext.InputType.Any(i => i.InputTypeId == data.ActiveForId))
            //{
            //    throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            //}
            if (_accountancyDBContext.PrintConfig.Any(p => p.PrintConfigName == data.PrintConfigName))
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNameAlreadyExisted);
            }

            try
            {
                PrintConfig config = _mapper.Map<PrintConfig>(data);
                await _accountancyDBContext.PrintConfig.AddAsync(config);
                await _accountancyDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Thêm cấu hình phiếu in chứng từ {config.PrintConfigName} ", data.JsonSerialize());

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
            //using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.ActiveForId));
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
                config.ActiveForId = data.ActiveForId;
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
                config.TemplateFileId = data.TemplateFileId;
                config.GenerateToString = data.GenerateToString;
                config.ModuleTypeId = data.ModuleTypeId;

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

            await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Xóa cấu hình phiếu in chứng từ {config.PrintConfigName}", config.JsonSerialize());
            return true;
        }

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, int fileId, PrintTemplateInput templateModel)
        {
            var printConfig = await _accountancyDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null) throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            var fileInfo = await _physicalFileService.GetSimpleFileInfo(printConfig.TemplateFileId.Value);

            if (fileInfo == null) throw new BadRequestException(FileErrorCode.FileNotFound);

            try
            {
                var newFile  = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, templateModel.JsonSerialize(), _accountancyDBContext);
                return (System.IO.File.OpenRead(newFile.filePath), newFile.contentType, newFile.fileName);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(InputErrorCode.DoNotGeneratePrintTemplate, ex.Message);
            }
        }
       
    }
}

