using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IDocOpenXmlService
    {
        Task<Stream> GenerateWordAsPdfFromTemplate(SimpleFileInfo fileInfo, string jsonString, DbContext dbContext);
        Task<string> GenerateWordFromTemplate((string file, string outDirectory, string fileTemplate) fileInfo, string jsonString, DbContext dbContext);
    }
    public class DocOpenXmlService : IDocOpenXmlService
    {
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContextService;

        public DocOpenXmlService(ILogger<DocOpenXmlService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContextService)
        {
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContextService = currentContextService;
        }

        public async Task<Stream> GenerateWordAsPdfFromTemplate(SimpleFileInfo fileInfo, string data, DbContext dbContext)
        {
            string outDirectory = GeneratePhysicalFolder();
            var fileTempatePath = GetPhysicalFilePath(fileInfo.FilePath);

            await GenerateWordFromTemplate(fileInfo: (fileInfo.FileName, outDirectory, fileTempatePath), data, dbContext);

            return await WordOpenXmlTools.ConvertToPdf(fileDocPath: $"{outDirectory}/{fileInfo.FileName}", _appSetting);
        }

        public async Task<string> GenerateWordFromTemplate((string file, string outDirectory, string fileTemplate) fileInfo,
            string jsonData, DbContext dbContext)
        {
            string fileName = $"{fileInfo.outDirectory}/{fileInfo.file}";
            JObject jObject = JObject.Parse(jsonData);

            using (var document = WordprocessingDocument.CreateFromTemplate(fileInfo.fileTemplate))
            {
                var body = document.MainDocumentPart.Document.Body;

                #region generate row data into table
                var tRowDetects = body.Descendants<TableRow>()
                                            .Where(x => x.InnerText.StartsWith(RegexDocExpression.DetectMainTable));
                foreach(var tRowDetect in tRowDetects)
                {
                    var mainTable = (Table)(tRowDetect?.Parent);

                    if (mainTable != null)
                    {
                        mainTable.AutoFitContents();

                        var rows = mainTable.Descendants<TableRow>();
                        var index = rows.IndexOf(tRowDetect);

                        if (rows.Count() > 1)
                        {
                            TableRow row = rows.ElementAt(index + 1);

                            var dataPath = tRowDetect.InnerText.Replace(RegexDocExpression.DetectMainTable, "").Trim();
                            foreach (var data in jObject.SelectTokens(dataPath).Reverse())
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
                                                var timeZone = new SqlParameter("@timeZone", Math.Abs(_currentContextService.TimeZoneOffset.GetValueOrDefault() * 60));
                                                var tbl = await dbContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam, timeZone });
                                                rs = tbl.Rows[0][0].ToString();
                                            }
                                            else
                                                rs = (string)data.SelectToken(field);

                                            WordOpenXmlTools.ReplaceText(paragraph, fullText, rs ?? string.Empty);
                                        }
                                    }
                                }
                                mainTable.InsertAfter(tableRow, row);
                            }
                            row.Remove();
                            tRowDetect.Remove();
                        }
                        mainTable.AutoFitWindow();
                    }
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
                        if (field.StartsWith(RegexDocExpression.StartWithFuntion))
                        {
                            var sqlParam = new SqlParameter("@data", jsonData);
                            var timeZone = new SqlParameter("@timeZone", Math.Abs(_currentContextService.TimeZoneOffset.GetValueOrDefault() * 60));
                            var tbl = await dbContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam, timeZone });
                            rs = tbl.Rows[0][0].ToString();
                        }
                        else
                            rs = (string)jObject.SelectToken(field);

                        WordOpenXmlTools.ReplaceText(paragraph, fullText, rs ?? string.Empty);
                    }
                }
                #endregion

                
                document.SaveAs(fileName).Close();
                document.Close();
            }
            return fileName;
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
