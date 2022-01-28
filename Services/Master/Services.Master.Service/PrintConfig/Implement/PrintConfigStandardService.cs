using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.PrintConfig;
using static Verp.Resources.Master.Print.PrintConfigStandardValidationMessage;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class PrintConfigStandardService : IPrintConfigStandardService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;

        private readonly ILogger _logger;
        private readonly ObjectActivityLogFacade _printConfigStandardActivityLog;
        private readonly IMapper _mapper;
        private readonly IDocOpenXmlService _docOpenXmlService;

        private readonly UploadTemplatePrintConfigFacade _uploadTemplate;

        public PrintConfigStandardService(MasterDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigStandardService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService)
        {
            _masterDBContext = accountancyDBContext;
            _docOpenXmlService = docOpenXmlService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _mapper = mapper;

            _uploadTemplate = new UploadTemplatePrintConfigFacade().SetAppSetting(_appSetting);

            _printConfigStandardActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigStandard);
        }

        public async Task<int> AddPrintConfigStandard(PrintConfigStandardModel model, IFormFile file)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                if (file != null)
                {
                    var fileInfo = await _uploadTemplate.Upload(EnumObjectType.PrintConfigStandard, EnumFileType.Document, file);
                    model.TemplateFileName = fileInfo.FileName;
                    model.TemplateFilePath = fileInfo.FilePath;
                    model.ContentType = fileInfo.ContentType;
                }

                var config = _mapper.Map<PrintConfigStandard>(model);
                await _masterDBContext.PrintConfigStandard.AddAsync(config);
                await _masterDBContext.SaveChangesAsync();


                await trans.CommitAsync();

                await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Create)
                  .MessageResourceFormatDatas(config.PrintConfigName)
                  .ObjectId(config.PrintConfigStandardId)
                  .JsonData(model.JsonSerialize())
                  .CreateLog();

                return config.PrintConfigStandardId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddPrintConfigStandard");
                throw;
            }
        }

        public async Task<bool> DeletePrintConfigStandard(int printConfigId)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.PrintConfigStandard.FirstOrDefaultAsync(p => p.PrintConfigStandardId == printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                //if (!string.IsNullOrWhiteSpace(config.TemplateFilePath))
                //    await _uploadTemplate.DeleteFile(config.TemplateFilePath);

                config.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();
             
                await trans.CommitAsync();

                await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(config.PrintConfigName)
                   .ObjectId(config.PrintConfigStandardId)
                   .JsonData(config.JsonSerialize())
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletePrintConfigStandard");
                throw;
            }
        }

        public async Task<PrintConfigStandardModel> GetPrintConfigStandard(int printConfigId)
        {
            var config = await _masterDBContext.PrintConfigStandard.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PrintConfigStandardId == printConfigId);

            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            return _mapper.Map<PrintConfigStandardModel>(config);
        }

        public async Task<PageData<PrintConfigStandardModel>> Search(int moduleTypeId, string keyword, int page, int size, string orderByField, bool asc)
        {
            keyword = (keyword ?? "").Trim();
            
            var query = _masterDBContext.PrintConfigStandard.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.PrintConfigName.Contains(keyword));

            if(moduleTypeId > 0)
                query = query.Where(x => x.ModuleTypeId.Equals(moduleTypeId));

            var total = await query.CountAsync();
            var lst = await (size > 0 ? (query.Skip((page - 1) * size)).Take(size) : query)
                .InternalOrderBy(orderByField, asc)
                .ProjectTo<PrintConfigStandardModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<bool> UpdatePrintConfigStandard(int printConfigId, PrintConfigStandardModel model, IFormFile file)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.PrintConfigStandard.FirstOrDefaultAsync(p => p.PrintConfigStandardId == printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                if (file != null)
                {
                    var fileInfo = await _uploadTemplate.Upload(EnumObjectType.PrintConfigStandard, EnumFileType.Document, file);
                    model.TemplateFilePath = fileInfo.FilePath;
                    model.TemplateFileName = fileInfo.FileName;
                    model.ContentType = fileInfo.ContentType;
                }

                _mapper.Map(model, config);

                await _masterDBContext.SaveChangesAsync();

              
                await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Update)
                   .MessageResourceFormatDatas(config.PrintConfigName)
                   .ObjectId(config.PrintConfigStandardId)
                   .JsonData(model.JsonSerialize())
                   .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePrintConfigStandard");
                throw;
            }
        }

        public async Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId)
        {
            var printConfig = await _masterDBContext.PrintConfigStandard
                .Where(p => p.PrintConfigStandardId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(printConfig.TemplateFilePath))
                throw TemplateFilePathIsEmpty.BadRequest(InputErrorCode.PrintConfigNotFound);

            if (!_uploadTemplate.ExistsFile(printConfig.TemplateFilePath))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            return (_uploadTemplate.GetFileStream(printConfig.TemplateFilePath), printConfig.ContentType, printConfig.TemplateFileName);
        }

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc)
        {
            var printConfig = await _masterDBContext.PrintConfigStandard
                .Where(p => p.PrintConfigStandardId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(printConfig.TemplateFilePath))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            try
            {
                var fileInfo = new SimpleFileInfo
                {
                    ContentType = printConfig.ContentType,
                    FileName = printConfig.TemplateFileName,
                    FilePath = printConfig.TemplateFilePath
                };

                if (!isDoc)
                {
                    var newFile = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);

                    return (newFile, "application/pdf", Path.GetFileNameWithoutExtension(fileInfo.FileName) + ".pdf");
                }

                var filePath = await _docOpenXmlService.GenerateWordFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);
                return (File.OpenRead(filePath), "", fileInfo.FileName);

            }
            catch (Exception ex)
            {
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }
    }
}
