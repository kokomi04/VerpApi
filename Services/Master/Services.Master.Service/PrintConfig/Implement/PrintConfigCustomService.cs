using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class PrintConfigCustomService : IPrintConfigCustomService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly IDocOpenXmlService _docOpenXmlService;

        private readonly UploadTemplatePrintConfigFacade _uploadTemplate;

        public PrintConfigCustomService(MasterDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigCustomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService)
        {
            _docOpenXmlService = docOpenXmlService;
            _masterDBContext = accountancyDBContext;
            _activityLogService = activityLogService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _mapper = mapper;

            _uploadTemplate = new UploadTemplatePrintConfigFacade().SetAppSetting(_appSetting);
        }

        public async Task<int> AddPrintConfigCustom(PrintConfigCustomModel model, IFormFile file)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                if (file != null)
                {
                    var fileInfo = await _uploadTemplate.Upload(EnumObjectType.PrintConfig, EnumFileType.Document, file);
                    model.TemplateFileName = fileInfo.FileName;
                    model.TemplateFilePath = fileInfo.FilePath;
                    model.ContentType = fileInfo.ContentType;
                }

                var config = _mapper.Map<PrintConfigCustom>(model);
                await _masterDBContext.PrintConfigCustom.AddAsync(config);
                await _masterDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigCustomId, $"Thêm cấu hình phiếu in chứng từ {config.PrintConfigName} ", model.JsonSerialize());

                await trans.CommitAsync();

                return config.PrintConfigCustomId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddPrintConfigCustom");
                throw;
            }
        }

        public async Task<bool> DeletePrintConfigCustom(int printConfigId)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.PrintConfigCustom.FirstOrDefaultAsync(p => p.PrintConfigCustomId == printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                //if (!string.IsNullOrWhiteSpace(config.TemplateFilePath))
                //    await _uploadTemplate.DeleteFile(config.TemplateFilePath);

                config.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, config.PrintConfigCustomId, $"Xóa cấu hình phiếu in chứng từ {config.PrintConfigName}", config.JsonSerialize());

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletePrintConfigCustom");
                throw;
            }
        }

        public async Task<PrintConfigCustomModel> GetPrintConfigCustom(int printConfigId)
        {
            var config = await _masterDBContext.PrintConfigCustom.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PrintConfigCustomId == printConfigId);

            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            return _mapper.Map<PrintConfigCustomModel>(config);
        }

        public async Task<PageData<PrintConfigCustomModel>> Search(int moduleTypeId, string keyword, int page, int size, string orderByField, bool asc)
        {
            keyword = (keyword ?? "").Trim();
            
            var query = _masterDBContext.PrintConfigCustom.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.PrintConfigName.Contains(keyword));

            if (moduleTypeId > 0)
                query = query.Where(x => x.ModuleTypeId.Equals(moduleTypeId));

            var total = await query.CountAsync();
            var lst = await (size > 0 ? (query.Skip((page - 1) * size)).Take(size) : query)
                .InternalOrderBy(orderByField, asc)
                .ProjectTo<PrintConfigCustomModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<bool> UpdatePrintConfigCustom(int printConfigId, PrintConfigCustomModel model, IFormFile file)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.PrintConfigCustom.FirstOrDefaultAsync(p => p.PrintConfigCustomId == printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                if (file != null)
                {
                    var fileInfo = await _uploadTemplate.Upload(EnumObjectType.PrintConfig, EnumFileType.Document, file);
                    model.TemplateFilePath = fileInfo.FilePath;
                    model.TemplateFileName = fileInfo.FileName;
                    model.ContentType = fileInfo.ContentType;
                }

                _mapper.Map(model, config);

                await _masterDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputType, printConfigId, $"Cập nhật cấu hình phiếu in chứng từ {model.PrintConfigName}", model.JsonSerialize());
                
                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePrintConfigCustom");
                throw;
            }
        }

        public async Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId)
        {
            var printConfig = await _masterDBContext.PrintConfigCustom
                .Where(p => p.PrintConfigCustomId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null) 
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(printConfig.TemplateFilePath))
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound, "Chưa có file template cấu hình phiếu in");

            if (!_uploadTemplate.ExistsFile(printConfig.TemplateFilePath))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            return (_uploadTemplate.GetFileStream(printConfig.TemplateFilePath), printConfig.ContentType, printConfig.TemplateFileName);
        }

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc)
        {
            var printConfig = await _masterDBContext.PrintConfigCustom
                .Where(p => p.PrintConfigCustomId == printConfigId)
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

                if(!isDoc)
                {
                    var newFile = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);

                    return (newFile, "application/pdf", Path.GetFileNameWithoutExtension(fileInfo.FileName) + ".pdf");
                }

                var filePath = await _docOpenXmlService.GenerateWordFromTemplate(fileInfo, templateModel.JsonSerialize(), _masterDBContext);
                return (File.OpenRead(filePath), "", fileInfo.FileName);
                
            }
            catch (Exception ex)
            {
                throw new BadRequestException(InputErrorCode.DoNotGeneratePrintTemplate, ex.Message);
            }
        }

        public async Task<bool> RollbackPrintConfigCustom(int printConfigId)
        {
            var printConfig = await _masterDBContext.PrintConfigCustom
                .Where(p => p.PrintConfigCustomId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            if (!printConfig.PrintConfigStandardId.HasValue || printConfig.PrintConfigStandardId.Value <= 0)
                throw new BadRequestException(GeneralCode.InternalError, "Phiếu in không có bản gốc");

            var source = await _masterDBContext.PrintConfigStandard
                .Where(x => x.PrintConfigStandardId == printConfig.PrintConfigStandardId)
                .ProjectTo<PrintConfigRollbackModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if(source == null)
                throw new BadRequestException(GeneralCode.InternalError, "Không tìm thấy bản gốc của phiếu in.");

            var destProperties = printConfig.GetType().GetProperties();
            foreach (var destProperty in destProperties)
            {
                var sourceProperty = source.GetType().GetProperty(destProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProperty != null && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    destProperty.SetValue(printConfig, sourceProperty.GetValue(source, new object[] { }), new object[] { });
                }
            }

            await _masterDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputType, printConfigId, $"Rollback cấu hình phiếu in chứng từ {printConfig.PrintConfigName}", printConfig.JsonSerialize());

            return true;
        }
    }
}
