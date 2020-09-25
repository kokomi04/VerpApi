using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
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
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
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
        private readonly StockDBContext _stockDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly AppSetting _appSetting;

        public PrintConfigService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            , StockDBContext stockDBContext
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _stockDBContext = stockDBContext;
            _appSetting = appSetting.Value;
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
            var query = _accountancyDBContext.PrintConfig.AsQueryable();
            if (inputTypeId > 0)
            {
                query = query.Where(p => p.InputTypeId == inputTypeId);
            }
            var lst = await query.OrderBy(p => p.Title)
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
                config.TemplateFileId = data.TemplateFileId;
                config.GenerateToString = data.GenerateToString;

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

        public async Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, int fileId, PrintTemplateInput templateModel)
        {
            var printConfig = await _accountancyDBContext.PrintConfig
                .Where(p => p.PrintConfigId == printConfigId)
                .FirstOrDefaultAsync();
            if (printConfig == null)
            {
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            }
            var fileInfo = await _stockDBContext.File.FirstOrDefaultAsync(f => f.FileId == printConfig.TemplateFileId);
            if (fileInfo == null)
            {
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            string file = Path.GetFileNameWithoutExtension(fileInfo.FileName);
            string outDirectory = GeneratePhysicalFolder();

            var physicalFilePath = GetPhysicalFilePath(fileInfo.FilePath);
            try
            {
                using (var document = WordprocessingDocument.CreateFromTemplate(physicalFilePath))
                {
                    var body = document.MainDocumentPart.Document.Body;

                    #region generate row data into table
                    var mainTable = (Table)(body.Descendants<TableProperties>()
                                                .Where(x => x.TableCaption?.Val == RegexDocExpression.DetectMainTable)
                                                .FirstOrDefault()?.Parent);
                    if (mainTable != null)
                    {
                        mainTable.AutoFitContents();

                        var rows = mainTable.Descendants<TableRow>();
                        if (rows.Count() > 1)
                        {
                            TableRow row = rows.ElementAt(1);
                            templateModel.data.Reverse();
                            foreach (var data in templateModel.data)
                            {
                                var tableRow = (TableRow)row.Clone();
                                foreach (var cell in tableRow.Descendants<TableCell>())
                                {
                                    var lsDocMatch = cell.Descendants<Paragraph>()
                                                            .MatchingLinesWithRegex(RegexDocExpression.PrintTemplatePattern);

                                    foreach (var docMatch in lsDocMatch)
                                    {
                                        var paragraph = docMatch.paragraph;
                                        foreach (var (fullText, field) in docMatch.matchs)
                                        {
                                            string rs = string.Empty;
                                            if (field.StartsWith(RegexDocExpression.StartWithFuntion))
                                            {
                                                var sqlParam = new SqlParameter("@data", (new[] { data }).JsonSerialize());
                                                var tbl = await _accountancyDBContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam });
                                                rs = tbl.Rows[0][0].ToString();
                                            }
                                            else
                                                data.TryGetValue(field, out rs);

                                            WordOpenXmlTools.ReplaceText(paragraph, fullText, rs ?? string.Empty);
                                        }
                                    }
                                }
                                mainTable.InsertAfter(tableRow, row);
                            }
                            row.Remove();
                        }
                        mainTable.AutoFitWindow();
                    }
                    #endregion

                    #region find and replace string with regex
                    var docMatchs = body.Descendants<Paragraph>()
                                            .MatchingLinesWithRegex(RegexDocExpression.PrintTemplatePattern);

                    foreach (var docMatch in docMatchs)
                    {
                        var paragraph = docMatch.paragraph;
                        foreach (var (fullText, field) in docMatch.matchs)
                        {
                            string rs = string.Empty;
                            if (templateModel.data.Count > 0)
                            {
                                if (templateModel.data.Count > 0 && field.StartsWith(RegexDocExpression.StartWithFuntion))
                                {
                                    var sqlParam = new SqlParameter("@data", templateModel.data.JsonSerialize());
                                    var tbl = await _accountancyDBContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam });
                                    rs = tbl.Rows[0][0].ToString();
                                }
                                else
                                    templateModel.data[0].TryGetValue(field, out rs);
                            }

                            WordOpenXmlTools.ReplaceText(paragraph, fullText, rs ?? string.Empty);
                        }
                    }
                    #endregion

                    document.SaveAs($"{outDirectory}/{file}.docx").Close();
                    document.Close();
                    WordOpenXmlTools.ConvertToPdf($"{outDirectory}/{file}.docx", $"{outDirectory}/{file}.pdf");
                }
                return (System.IO.File.OpenRead($"{outDirectory}/{file}.pdf"), "application/pdf", $"{file}.pdf");
            }
            catch (Exception ex)
            {
                throw new BadRequestException(InputErrorCode.DoNotGeneratePrintTemplate, ex.Message);
            }
        }

        private string GeneratePhysicalFolder()
        {
            var relativeFolder = $"/_tmp_/{Guid.NewGuid().ToString()}";
            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);
            return obsoluteFolder;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }
    }
}

