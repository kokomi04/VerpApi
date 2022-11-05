using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static Verp.Resources.Master.Print.PrintConfigStandardValidationMessage;


namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public abstract class PrintConfigServiceAbstract<TEntity, TModel> where TEntity : class, IPrintConfigEntity where TModel : PrintConfigBaseModel
    {
        protected readonly MasterDBContext _masterDBContext;
        private readonly IOptions<AppSetting> appSetting;
        private readonly AppSetting _appSetting;

        private readonly ILogger _logger;
        protected readonly IMapper _mapper;
        private readonly IDocOpenXmlService _docOpenXmlService;
        private readonly EnumObjectType objectTypeId;
        private readonly UploadTemplatePrintConfigFacade _uploadTemplate;

        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>()
        {
            { ".doc" , "application/msword" },
            { ".pdf" , "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".jpg", "image/jpg" },
            { ".jpeg", "image/jpg" },
            { ".bmp", "image/bmp" },
            { ".png", "image/png" },
        };

        public PrintConfigServiceAbstract(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger logger
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService
            , EnumObjectType objectTypeId
           )
        {
            _masterDBContext = masterDBContext;
            this.appSetting = appSetting;
            _docOpenXmlService = docOpenXmlService;
            this.objectTypeId = objectTypeId;
            _appSetting = appSetting.Value;
            _logger = logger;
            _mapper = mapper;

            _uploadTemplate = new UploadTemplatePrintConfigFacade().SetAppSetting(_appSetting);

            // this.objectTypeId = objectTypeId;

        }


        protected abstract Task LogAddPrintConfig(TModel model, TEntity entity);
        protected abstract Task LogUpdatePrintConfig(TModel model, TEntity entity);
        protected abstract Task LogDeletePrintConfig(TEntity entity);

        protected abstract int GetId(TEntity entity);

        public async Task<int> AddPrintConfig(TModel model, IFormFile template, IFormFile background)
        {
            if (long.TryParse(model.Background, out var v))
            {
                throw GeneralCode.InvalidParams.BadRequest("Background không hợp lệ");
            }

            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                await UploadFile(model, template, background);

                var config = _mapper.Map<TEntity>(model);
                await _masterDBContext.Set<TEntity>().AddAsync(config);
                await _masterDBContext.SaveChangesAsync();


                await trans.CommitAsync();

                await LogAddPrintConfig(model, config);

                return GetId(config);
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddPrintConfig");
                throw;
            }
        }

        public async Task<bool> DeletePrintConfig(int printConfigId)
        {
            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                //if (!string.IsNullOrWhiteSpace(config.TemplateFilePath))
                //    await _uploadTemplate.DeleteFile(config.TemplateFilePath);

                config.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await LogDeletePrintConfig(config);

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletePrintConfig");
                throw;
            }
        }

        public async Task<TModel> GetPrintConfig(int printConfigId)
        {
            var config = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            return _mapper.Map<TModel>(config);
        }

        public async Task<PageData<TModel>> Search(int moduleTypeId, string keyword, int page, int size, string orderByField, bool asc)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDBContext.Set<TEntity>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.PrintConfigName.Contains(keyword));

            if (moduleTypeId > 0)
                query = query.Where(x => x.ModuleTypeId.Equals(moduleTypeId));

            var total = await query.CountAsync();
            var lst = await (size > 0 ? (query.Skip((page - 1) * size)).Take(size) : query)
                .InternalOrderBy(orderByField, asc)
                .ProjectTo<TModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        private async Task UploadFile(TModel model, IFormFile template, IFormFile background)
        {

            if (template != null)
            {
                var fileInfo = await _uploadTemplate.Upload(objectTypeId, EnumFileType.Document, template);
                model.TemplateFileName = fileInfo.FileName;
                model.TemplateFilePath = fileInfo.FilePath;
                model.ContentType = fileInfo.ContentType;
            }

            if (background != null)
            {
                var fileInfo = await _uploadTemplate.Upload(objectTypeId, EnumFileType.Image, background);
                model.Background = fileInfo.FilePath;
            }
        }
        public async Task<bool> UpdatePrintConfig(int printConfigId, TModel model, IFormFile template, IFormFile background)
        {
            if (long.TryParse(model.Background, out var v))
            {
                throw GeneralCode.InvalidParams.BadRequest("Background không hợp lệ");
            }

            var trans = await _masterDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
                if (config == null)
                    throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

                await UploadFile(model, template, background);


                _mapper.Map(model, config);

                await _masterDBContext.SaveChangesAsync();

                await LogUpdatePrintConfig(model, config);

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePrintConfig");
                throw;
            }
        }

        public async Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId)
        {
            var config = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(config.TemplateFilePath))
                throw TemplateFilePathIsEmpty.BadRequest(InputErrorCode.PrintConfigNotFound);

            if (!_uploadTemplate.ExistsFile(config.TemplateFilePath))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            return (_uploadTemplate.GetFileStream(config.TemplateFilePath), config.ContentType, config.TemplateFileName);
        }

        public async Task<(Stream file, string contentType, string fileName)> GetPrintConfigBackgroundFile(int printConfigId)
        {
            var config = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
            if (config == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);

            if (string.IsNullOrWhiteSpace(config.Background))
                throw TemplateFilePathIsEmpty.BadRequest(InputErrorCode.PrintConfigNotFound);

            if (!_uploadTemplate.ExistsFile(config.Background))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            var fileName = Path.GetFileName(config.Background);
            var ext = Path.GetExtension(config.Background)?.ToLower();
            var contentType = "";
            if (ContentTypes.ContainsKey(ext))
            {
                contentType = ContentTypes[ext];
            }
            else
            {
                contentType = "application/octet-stream";
            }

            return (_uploadTemplate.GetFileStream(config.Background), contentType, fileName);
        }

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc)
        {
            var printConfig = await _masterDBContext.Set<TEntity>().FindAsync(printConfigId);
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
