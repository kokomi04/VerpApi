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
        Task<(string filePath, string contentType, string fileName)> GenerateWordAsPdfFromTemplate(SimpleFileInfo fileInfo, string jsonString, DbContext dbContext);
        Task<bool> GenerateWordFromTemplate((string file, string outDirectory, string fileTemplate) fileInfo, string jsonString, DbContext dbContext);
    }
    public class DocOpenXmlService : IDocOpenXmlService
    {
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;

        public DocOpenXmlService(ILogger<DocOpenXmlService> logger, IOptionsSnapshot<AppSetting> appSetting)
        {
            _logger = logger;
            _appSetting = appSetting.Value;
        }

        public async Task<(string filePath, string contentType, string fileName)> GenerateWordAsPdfFromTemplate(SimpleFileInfo fileInfo, string jsonString, DbContext dbContext)
        {
            string file = Path.GetFileNameWithoutExtension(fileInfo.FileName);
            string outDirectory = GeneratePhysicalFolder();

            var physicalFilePath = GetPhysicalFilePath(fileInfo.FilePath);

            await GenerateWordFromTemplate((fileInfo.FileName, outDirectory, physicalFilePath), jsonString, dbContext);

            WordOpenXmlTools.ConvertToPdf($"{outDirectory}/{fileInfo.FileName}", $"{outDirectory}/{file}.pdf");

            return ($"{outDirectory}/{file}.pdf", "application/pdf", $"{file}.pdf");
        }

        public async Task<bool> GenerateWordFromTemplate((string file, string outDirectory, string fileTemplate) fileInfo,
            string jsonString, DbContext dbContext)
        {
            JObject jObject = JObject.Parse(jsonString);

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
                                                var tbl = await dbContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam });
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
                            var sqlParam = new SqlParameter("@data", jsonString);
                            var tbl = await dbContext.QueryDataTable($"SELECT {field.Substring(1)}", new[] { sqlParam });
                            rs = tbl.Rows[0][0].ToString();
                        }
                        else
                            rs = (string)jObject.SelectToken(field);

                        WordOpenXmlTools.ReplaceText(paragraph, fullText, rs ?? string.Empty);
                    }
                }
                #endregion

                document.SaveAs($"{fileInfo.outDirectory}/{fileInfo.file}").Close();
                document.Close();
            }
            return true;
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
