using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class DataReportExcelFacade
    {
        private ISheet sheet = null;
        private ExcelWriter xssfwb = null;
        private int currentRow = 0;
        private int maxCloumn = 15;
        private ReportFacadeModel _model;
        private IList<ReportColumnModel> columns = null;

        private readonly string sheetName = "Data";

        private AppSetting _appSetting;
        private IPhysicalFileService _physicalFileService;
        private ReportConfigDBContext _contextData;

        private readonly Dictionary<string, PictureType> drImageType = new Dictionary<string, PictureType>
        {
            {".jpeg", PictureType.JPEG },
            {".png", PictureType.PNG },
            {".gif", PictureType.GIF },
            {".emf", PictureType.EMF },
            {".wmf", PictureType.WMF },
            {".pict", PictureType.PICT },
            {".dib", PictureType.DIB },
            {".tiff", PictureType.TIFF },
            {".eps", PictureType.EPS },
            {".bmp", PictureType.BMP },
            {".wpg", PictureType.WPG },
        };
        private readonly Dictionary<string, HorizontalAlignment> drTextAlign = new Dictionary<string, HorizontalAlignment>
        {
            {"center", HorizontalAlignment.Center },
            {"left", HorizontalAlignment.Left },
            {"right", HorizontalAlignment.Right }
        };

        public async Task<(Stream stream, string fileName, string contentType)> AccountancyReportExport(int reportId, ReportFacadeModel model)
        {
            var reportInfo = await _contextData.ReportType.AsNoTracking().FirstOrDefaultAsync(r => r.ReportTypeId == reportId);

            if (reportInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>().Where(col => !col.IsHidden).OrderBy(col => col.SortOrder).ToList();

            _model = model;
            xssfwb = new ExcelWriter();
            sheet = xssfwb.GetSheet(sheetName);

            maxCloumn = columns.Count;

            await WriteHeader();

            WriteBody(reportInfo);
            WriteFooter();

            for (var i = 0; i <= maxCloumn; i++)
            {
                sheet.AutoSizeColumn(i, true);
            }

            for (var i = 0; i <= maxCloumn; i++)
            {
                var c = sheet.GetColumnWidth(i);
                if (c < 2600)
                {
                    sheet.SetColumnWidth(i, 2600);
                }
                else if (c > 10000)
                {
                    sheet.SetColumnWidth(i, 10000);
                }
            }

            var stream = await xssfwb.WriteToStream();
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = Utils.RemoveDiacritics($"{reportInfo.ReportTypeName} {DateTime.UtcNow.ToString("dd_MM_yyyy")}.xlsx").Replace(" ", "_");
            return (stream, fileName, contentType);
        }

        private async Task WriteHeader()
        {
            if (_model.Header != null)
            {
                int fRow, sRow;
                fRow = currentRow;
                sRow = fRow + 3;

                sheet.CreateRow(0).Height = 400;
                sheet.CreateRow(1).Height = 400;
                sheet.CreateRow(2).Height = 400;
                sheet.CreateRow(3).Height = 400;

                var fCol = 0;
                var lCol = maxCloumn - 1;

                if (_model.Header.fLogoId > 0)
                {
#if !DEBUG
                    var fileInfo = await _physicalFileService.GetSimpleFileInfo(_model.Header.fLogoId);
                    if (fileInfo != null)
                    {
                        fCol = 2;
                        var fileStream = File.OpenRead(GetPhysicalFilePath(fileInfo.FilePath));
                        byte[] bytes = IOUtils.ToByteArray(fileStream);

                        int pictureIdx = xssfwb.AddPicture(bytes, GetPictureType(Path.GetExtension(fileInfo.FileName)));
                        fileStream.Close();

                        var helper = xssfwb.GetCreationHelper();
                        var drawing = sheet.CreateDrawingPatriarch();
                        var anchor = helper.CreateClientAnchor();

                        anchor.Col1 = 0;
                        anchor.Row1 = 0;
                        anchor.Col2 = 2;
                        anchor.Row2 = 4;

                        drawing.CreatePicture(anchor, pictureIdx);
                    }
#endif
                }
                if (!string.IsNullOrEmpty(_model.Header.FormBreif) && !_model.Header.FormBreif.Contains("null"))
                {
                    int l = lCol;
                    int f = lCol - 4;
                    lCol = f - 1;
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, f, l));
                    sheet.EnsureCell(fRow, f).SetCellValue(_model.Header.FormBreif);
                    sheet.SetCellStyle(fRow, f, 12, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Center, isWrap: true);
                }
                if (!string.IsNullOrEmpty(_model.Header.CompanyBreif) && !_model.Header.CompanyBreif.Contains("null"))
                {
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, fCol, lCol));
                    sheet.EnsureCell(fRow, fCol).SetCellValue(_model.Header.CompanyBreif);
                    sheet.SetCellStyle(fRow, fCol, 12, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true);
                }

                currentRow = sRow;
            }

        }

        private void WriteBody(ReportType reportInfo)
        {
            GenerateBodyInfo();
            currentRow += 2;
            int startRow = currentRow;
            GenerateHeadTable(reportInfo);
            GenerateDataTable(reportInfo);
            int endRow = currentRow;
        }

        private void GenerateBodyInfo()
        {
            if (_model.Body != null)
            {
                //Tiêu đề báo cáo
                currentRow += 2;
                if (!string.IsNullOrEmpty(_model.Body.Title))
                {
                    int fRow = currentRow;
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 0, maxCloumn - 1));
                    sheet.EnsureCell(fRow, 0).SetCellValue(_model.Body.Title);
                    sheet.SetCellStyle(fRow, 0, vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center, fontSize: 18, isBold: true);
                }
                if (_model.Body.HeadDetails.Count > 0)
                {
                    int i = 1;
                    foreach (var info in _model.Body.HeadDetails)
                    {
                        int row = currentRow + i;
                        sheet.AddMergedRegion(new CellRangeAddress(row, row, 0, maxCloumn - 1));
                        sheet.EnsureCell(row, 0).SetCellValue(info.Value);
                        sheet.SetCellStyle(row, 0, vAlign: VerticalAlignment.Top, hAlign: drTextAlign[info.TextAlign]);
                        i++;
                    }

                    currentRow += _model.Body.HeadDetails.Count;
                }
            }
        }

        private void GenerateHeadTable(ReportType reportInfo)
        {
            int fRow, sRow;
            fRow = sRow = 0;
            byte[] headerRgb = new byte[3] { 107, 150, 207 };

            if (!string.IsNullOrEmpty(reportInfo.GroupColumns)) sRow = 1;
            fRow = currentRow;
            sRow = fRow + sRow;
            if (!string.IsNullOrEmpty(reportInfo.GroupColumns))
            {
                var gColumns = reportInfo.GroupColumns.Split("@").Select(x =>
                {
                    var value = x.Substring(0, x.IndexOf("("));
                    var pivot = x.Substring(x.IndexOf("(") + 1, x.IndexOf(")") - x.IndexOf("(") - 1).Split(",");

                    int.TryParse(pivot[0], out int fCol);
                    int.TryParse(pivot[pivot.Length - 1], out int lCol);

                    return new { value, fCol, lCol };
                });

                foreach (var col in gColumns)
                {
                    var fCol = columns.IndexOf(columns.FirstOrDefault(x => x.SortOrder == col.fCol));
                    var lCol = columns.IndexOf(columns.FirstOrDefault(x => x.SortOrder == col.lCol));
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, fCol, lCol));
                    sheet.EnsureCell(fRow, fCol).SetCellValue(col.value);
                    for (int i = fCol; i <= lCol; i++)
                    {
                        sheet.SetCellStyle(fRow, i,
                            vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                            rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                    }
                }

                for (int i = 0; i < columns.Count; i++)
                {
                    if (gColumns.Any(x => x.fCol == columns[i].SortOrder
                        || x.lCol == columns[i].SortOrder
                        || (x.lCol > columns[i].SortOrder && x.fCol < columns[i].SortOrder)))
                    {
                        sheet.EnsureCell(sRow, i).SetCellValue(columns[i].Name);
                        sheet.SetCellStyle(sRow, i,
                            vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                            rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                    }
                    else
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, i, i));
                        sheet.EnsureCell(fRow, i).SetCellValue(columns[i].Name);
                        sheet.SetCellStyle(fRow, i,
                            vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                            rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                        sheet.SetCellStyle(sRow, i,
                            vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                            rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    sheet.EnsureCell(fRow, i).SetCellValue(columns[i].Name);
                    sheet.SetCellStyle(fRow, i,
                        vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                        rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                }
            }

            currentRow = sRow;
        }

        private void GenerateDataTable(ReportType reportInfo)
        {
            currentRow += 1;
            ExcelData table = new ExcelData();

            for (var index = 1; index <= columns.Count; index++)
            {
                table.Columns.Add($"Col-{index}");
            }
            var sumCalc = new List<int>();
            foreach (var row in _model.Body.TableData)
            {
                ExcelRow tbRow = table.NewRow();
                int columnIndx = 0;
                foreach (var field in columns)
                {
                    if (field.IsCalcSum) sumCalc.Add(columnIndx);
                    var dataType = field.DataTypeId.HasValue ? (EnumDataType)field.DataTypeId : EnumDataType.Text;
                    if (row.ContainsKey(field.Alias))
                        tbRow[columnIndx] = new ExcelCell
                        {
                            Value = dataType.GetSqlValue(row[field.Alias]),
                            Type = dataType.GetExcelType()
                        };
                    columnIndx++;
                }
                tbRow.FillAllRow();
                table.Rows.Add(tbRow);
            }
            if (sumCalc.Count > 0)
            {
                ExcelRow sumRow = table.NewRow();
                foreach (int columnIndx in sumCalc)
                {
                    var columnName = (columnIndx + 1).GetExcelColumnName();
                    sumRow[columnIndx] = new ExcelCell
                    {
                        Value = $"SUM({columnName}{currentRow + 1}:{columnName}{currentRow + _model.Body.TableData.Count()})",
                        Type = EnumExcelType.Formula
                    };
                }
                sumRow.FillAllRow();
                table.Rows.Add(sumRow);
            }

            xssfwb.WriteToSheet(table, sheetName, out currentRow, startCollumn: 0, startRow: currentRow);
        }

        private void WriteFooter()
        {
            var signs = _model.Footer?.SignText?.Split("@");

            if (signs != null && signs.Length > 0)
            {
                int fRow = currentRow + 3;

                var rCol = (int)maxCloumn / signs.Length;
                for (int i = 0; i < signs.Length; i++)
                {
                    var fCol = i > 0 ? (i * rCol) : 0;
                    var lCol = (i + 1) * rCol - 1;
                    if (i == signs.Length - 1 && maxCloumn > (signs.Length * rCol)) lCol += (maxCloumn - (signs.Length * rCol));
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, fCol, lCol));
                    sheet.EnsureCell(fRow, fCol).SetCellValue(signs[i].Replace("/", $"\r\n"));
                    sheet.SetSignatureCellStyle(fRow, fCol);

                    if (i == signs.Length - 1 && !string.IsNullOrEmpty(_model.Footer.RDateText))
                    {
                        var rIndex = fRow - 1;
                        sheet.AddMergedRegion(new CellRangeAddress(rIndex, rIndex, fCol, lCol));
                        sheet.EnsureCell(rIndex, fCol).SetCellValue(_model.Footer.RDateText);
                        sheet.SetCellStyle(rIndex, fCol, vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center);
                    }
                }

                sheet.GetRow(fRow).Height = 1700;
            }
            else if (!string.IsNullOrEmpty(_model.Footer.RDateText))
            {
                int fRow = currentRow + 2;
                sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 0, maxCloumn));
                sheet.EnsureCell(fRow, 0).SetCellValue(_model.Footer.RDateText);
                sheet.SetCellStyle(fRow, 0, vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center);
            }

            currentRow++;
        }

        private PictureType GetPictureType(string extension)
        {
            if (drImageType.ContainsKey(extension))
                return drImageType[extension];
            return PictureType.None;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return Utils.GetPhysicalFilePath(filePath, _appSetting);
        }

        internal void SetAppSetting(AppSetting appSetting) => _appSetting = appSetting;

        internal void SetPhysicalFileService(IPhysicalFileService physicalFileService) => _physicalFileService = physicalFileService;

        internal void SetContextData(ReportConfigDBContext reportConfigDB) => _contextData = reportConfigDB;
    }
}
