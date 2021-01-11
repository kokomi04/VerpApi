using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
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
using VErp.Commons.Enums.StockEnum;
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

        private static readonly Dictionary<string, EnumFileType> FileExtensionTypes = new Dictionary<string, EnumFileType>()
        {

            { ".doc" , EnumFileType.Document },
            { ".docx", EnumFileType.Document },
        };

        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>()
        {
            { ".doc" , "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        };

        private static readonly Dictionary<EnumFileType, string[]> FileTypeExtensions = FileExtensionTypes.GroupBy(t => t.Value).ToDictionary(t => t.Key, t => t.Select(v => v.Key).ToArray());

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

        public async Task<PrintConfigModel> GetPrintConfig(int printConfigId, bool isOrigin)
        {
            if (isOrigin && !_currentContextService.IsDeveloper)
                throw new BadRequestException(GeneralCode.GeneralError, "Không có quyền truy cập vào phiếu in gốc");

            var printConfig = await _masterDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (printConfig == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }

            printConfig.PrintConfigDetailModel = printConfig.PrintConfigDetailModel.Where(x => x.IsOrigin == isOrigin).ToList();
            return _mapper.Map<PrintConfigModel>(printConfig);
        }

        public async Task<ICollection<PrintConfigModel>> GetPrintConfigs(int moduleTypeId)
        {
            var query = _masterDBContext.PrintConfig.AsQueryable()
                .Where(p => p.ModuleTypeId == moduleTypeId);

            var lst = await query.OrderBy(p => p.Title)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return lst.AsQueryable().ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider).ToList();
        }

        public async Task<int> AddPrintConfig(PrintConfigModel data, IFormFile file)
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
                if (file != null)
                {
                    var fileInfo = await Upload(EnumObjectType.PrintConfig, EnumFileType.Document, file);
                    data.TemplateFileName = fileInfo.FileName;
                    data.TemplateFilePath = fileInfo.FilePath;
                    data.ContentType = fileInfo.ContentType;
                }

                var configExtract = _mapper.Map<PrintConfigExtract>(data);
                var config = _mapper.Map<PrintConfig>(configExtract);
                await _masterDBContext.PrintConfig.AddAsync(config);
                await _masterDBContext.SaveChangesAsync();

                if (_currentContextService.IsDeveloper)
                {
                    var configOrigin = _mapper.Map<PrintConfigDetail>(data as PrintConfigDetailModel);
                    configOrigin.PrintConfigId = config.PrintConfigId;
                    configOrigin.IsOrigin = true;

                    await _masterDBContext.PrintConfigDetail.AddAsync(configOrigin);
                }

                var configDetail = _mapper.Map<PrintConfigDetail>(data as PrintConfigDetailModel);
                configDetail.PrintConfigId = config.PrintConfigId;
                configDetail.IsOrigin = false;
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

        public async Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data, IFormFile file)
        {
            //using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(data.ActiveForId));
            var config = await _masterDBContext.PrintConfig.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
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
                if (file != null)
                {
                    var fileInfo = await Upload(EnumObjectType.PrintConfig, EnumFileType.Document, file);
                    data.TemplateFilePath = fileInfo.FilePath;
                    data.TemplateFileName = fileInfo.FileName;
                    data.ContentType = fileInfo.ContentType;
                }

                var configExtract = _mapper.Map<PrintConfigExtract>(data);
                _mapper.Map(configExtract, config);
                await _masterDBContext.SaveChangesAsync();

                PrintConfigDetail detail;
                if (_currentContextService.IsDeveloper)
                    detail = await _masterDBContext.PrintConfigDetail.FirstOrDefaultAsync(x => x.PrintConfigId == config.PrintConfigId && x.IsOrigin == data.IsOrigin);
                else
                {
                    detail = await _masterDBContext.PrintConfigDetail.FirstOrDefaultAsync(x => x.PrintConfigId == config.PrintConfigId && !x.IsOrigin);
                    data.IsOrigin = false;
                }


                if (detail != null)
                    _mapper.Map(data, detail);
                else
                {
                    var detailEnity = _mapper.Map<PrintConfigDetail>(data);
                    detailEnity.PrintConfigId = config.PrintConfigId;
                    await _masterDBContext.PrintConfigDetail.AddAsync(detailEnity);
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
                for (int i = 0; i < details.Count; i++)
                {
                    var detail = details[i];
                    detail.IsDeleted = true;
                    if (detail.TemplateFileId != null && detail.TemplateFileId.HasValue)
                        await _physicalFileService.DeleteFile(detail.TemplateFileId.Value);
                }

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

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, PrintTemplateInput templateModel)
        {
            var printConfig = await _masterDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (printConfig == null) throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(printConfig.TemplateFilePath)) throw new BadRequestException(FileErrorCode.FileNotFound);

            try
            {
                var fileInfo = new SimpleFileInfo
                {
                    ContentType = printConfig.ContentType,
                    FileName = printConfig.TemplateFileName,
                    FilePath = printConfig.TemplateFilePath
                };
                var newFile = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);
                return (File.OpenRead(newFile.filePath), newFile.contentType, newFile.fileName);
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

        public Task<IList<EntityField>> GetSuggestionField(Assembly assembly)
        {
            var classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttributes().Any(a => a is PrintSuggestionConfigAttribute))
                .ToArray();
            IList<EntityField> fields = new List<EntityField>();
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

            return Task.FromResult(fields);
        }

        public async Task<bool> RollbackPrintConfig(long printConfigId)
        {
            var config = await _masterDBContext.PrintConfig.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId);
            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            var detailOrigin = await _masterDBContext.PrintConfigDetail.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId && p.IsOrigin);

            if (detailOrigin == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound, "Không có bản phiếu in gốc");

            var detailModify = await _masterDBContext.PrintConfigDetail.FirstOrDefaultAsync(p => p.PrintConfigId == printConfigId && !p.IsOrigin);
            detailModify.IsDeleted = true;

            await _masterDBContext.SaveChangesAsync();
            return true;
        }

        private async Task<SimpleFileInfo> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!FileTypeExtensions.ContainsKey(fileTypeId))
            {
                throw new BadRequestException(FileErrorCode.InvalidFileType);
            }

            if (!FileTypeExtensions[fileTypeId].Contains(ext))
            {
                throw new BadRequestException(FileErrorCode.InvalidFileExtension);
            }

            return await Upload(objectTypeId, file);
        }

        private async Task<SimpleFileInfo> Upload(EnumObjectType objectTypeId, IFormFile file)
        {
            var (validate, fileTypeId) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            string filePath = GenerateTempFilePath(file.FileName);

            using (var stream = File.Create(GetPhysicalFilePath(filePath)))
            {
                await file.CopyToAsync(stream);
            }

            return new SimpleFileInfo
            {
                FileName = file.FileName,
                FilePath = filePath,
                FileTypeId = (int)fileTypeId,
                ContentType = file.ContentType,
                FileLength = file.Length
            };
        }


        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/_document_template_/{Guid.NewGuid().ToString()}";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }

        private (Enum, EnumFileType?) ValidateUploadFile(IFormFile uploadFile)
        {
            if (uploadFile == null || string.IsNullOrWhiteSpace(uploadFile.FileName) || uploadFile.Length == 0)
            {
                return (FileErrorCode.InvalidFile, null);
            }
            if (uploadFile.Length > _appSetting.Configuration.FileUploadMaxLength)
            {
                return (FileErrorCode.FileSizeExceededLimit, null);
            }

            var ext = Path.GetExtension(uploadFile.FileName).ToLower();

            if (!FileExtensionTypes.ContainsKey(ext))
            {
                return (FileErrorCode.InvalidFileType, null);
            }

            //if (!ValidFileExtensions.Values.Any(v => v.Contains(ext))
            //{
            //    return FileErrorCode.InvalidFileExtension;
            //}

            return (GeneralCode.Success, FileExtensionTypes[ext]);
        }

        public async Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId)
        {
            var printConfig = await _masterDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .ProjectTo<PrintConfigExtract>(_mapper.ConfigurationProvider)
                .ProjectTo<PrintConfigModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (printConfig == null) throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            if (string.IsNullOrWhiteSpace(printConfig.TemplateFilePath))
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound, "Chưa có file template cấu hình phiếu in");

            if (!File.Exists(GetPhysicalFilePath(printConfig.TemplateFilePath)))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            return (File.OpenRead(GetPhysicalFilePath(printConfig.TemplateFilePath)), printConfig.ContentType, printConfig.TemplateFileName);
        }
    }
}

