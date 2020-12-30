using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class PrintConfigService : IPrintConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly AppSetting _appSetting;
        private readonly IDocOpenXmlService _docOpenXmlService;
        private readonly IPhysicalFileService _physicalFileService;

        public PrintConfigService(MasterDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            , IPhysicalFileService physicalFileService
            , IDocOpenXmlService docOpenXmlService
            )
        {
            _masterDBContext = accountancyDBContext;
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
            var printConfig = await _masterDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (printConfig == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }
            return _mapper.Map<PrintConfigModel>(printConfig);
        }

        public async Task<ICollection<PrintConfigModel>> GetPrintConfigs(int moduleTypeId, int activeForId)
        {
            var query = _masterDBContext.PrintConfig.AsQueryable()
                .Where(p => p.ModuleTypeId == moduleTypeId);

            if (activeForId > 0)
            {
                query = query.Where(p => p.ActiveForId == activeForId);
            }
            var lst = await query.OrderBy(p => p.Title)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return lst.AsQueryable().ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider).ToList();
        }

        public async Task<int> AddPrintConfig(PrintConfigModel data)
        {
            //using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.ActiveForId));
            //if (!_accountancyDBContext.InputType.Any(i => i.InputTypeId == data.ActiveForId))
            //{
            //    throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            //}
            if (_masterDBContext.PrintConfig.Any(p => p.PrintConfigName == data.PrintConfigName))
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNameAlreadyExisted);
            }

            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var configExtract = _mapper.Map<PrintConfigExtract>(data);
                var config = _mapper.Map<PrintConfig>(configExtract);
                await _masterDBContext.PrintConfig.AddAsync(config);
                await _masterDBContext.SaveChangesAsync();

                var configDetail = _mapper.Map<PrintConfigDetail>(data as PrintConfigDetailModel);
                configDetail.PrintConfigId = config.PrintConfigId;
                configDetail.IsOrigin = true;
                await _masterDBContext.PrintConfigDetail.AddAsync(configDetail);
                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Thêm cấu hình phiếu in chứng từ {config.PrintConfigName} ", data.JsonSerialize());
                return config.PrintConfigId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddPrintConfig");
                throw;
            }
        }

        public async Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data)
        {
            //using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.ActiveForId));
            var config = await _masterDBContext.PrintConfig.Include(x=>x.PrintConfigDetail).FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
            if (config == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }
            if (_masterDBContext.PrintConfig.Any(p => p.PrintConfigId != printConfigId && p.PrintConfigName == data.PrintConfigName))
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNameAlreadyExisted);
            }

            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var configExtract = _mapper.Map<PrintConfigExtract>(data);
                _mapper.Map(configExtract, config);
                await _masterDBContext.SaveChangesAsync();

                if (config.PrintConfigDetail.Count > 1)
                {
                    var detail = await _masterDBContext.PrintConfigDetail.FirstOrDefaultAsync(p => !p.IsOrigin);
                    _mapper.Map(data, detail);
                }
                else
                {
                    var detail = _mapper.Map<PrintConfigDetail>(data);
                    detail.PrintConfigId = config.PrintConfigId;
                    detail.IsOrigin = false;
                    await _masterDBContext.PrintConfigDetail.AddAsync(detail);
                }
                await _masterDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Cập nhật cấu hình phiếu in chứng từ {config.PrintConfigName}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePrintConfig");
                throw;
            }
        }

        public async Task<bool> DeletePrintConfig(int printConfigId)
        {
            var config = await _masterDBContext.PrintConfig.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            var details = await _masterDBContext.PrintConfigDetail.Where(p => p.PrintConfigId == printConfigId).ToListAsync();
            
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                config.IsDeleted = true;
                details.ForEach(x => x.IsDeleted = true);

                await _masterDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigId, $"Xóa cấu hình phiếu in chứng từ {config.PrintConfigName}", config.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletePrintConfig");
                throw;
            }
        }

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, int fileId, PrintTemplateInput templateModel)
        {
            var printConfig = await _masterDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null) throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            var fileInfo = await _physicalFileService.GetSimpleFileInfo(printConfig.TemplateFileId.Value);

            if (fileInfo == null) throw new BadRequestException(FileErrorCode.FileNotFound);

            try
            {
                var newFile = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);
                return (System.IO.File.OpenRead(newFile.filePath), newFile.contentType, newFile.fileName);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(InputErrorCode.DoNotGeneratePrintTemplate, ex.Message);
            }
        }

        public async Task<IList<EntityField>> GetSuggestionField(int moduleTypeId)
        {
            var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ModuleTypeId", moduleTypeId)
                };
            var resultData = await _masterDBContext.ExecuteDataProcedure("asp_PrintConfig_SuggestionField", parammeters);
            return resultData.ConvertData<EntityField>()
                .ToList();
        }

        public async Task<IList<EntityField>> GetSuggestionField(Assembly assembly)
        {
            var classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttributes().Any(a => a is PrintSuggestionConfigAttribute))
                .ToArray();
            var fields = new List<EntityField>();
            foreach (var type in classTypes)
            {
                foreach (var prop in type.GetProperties())
                {
                    EntityField field = new EntityField
                    {
                        FieldName = prop.Name,
                        Title = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.Name ?? prop.Name,
                        Group = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.GroupName
                    };
                    fields.Add(field);
                }
            }

            return fields;
        }
    }
}

