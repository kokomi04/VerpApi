using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private int numberOfColumns = 15;
        private ReportFacadeModel _model;
        private IList<ReportColumnModel> columns = null;

        private readonly string sheetName = "Data";

        private AppSetting _appSetting;
        private IPhysicalFileService _physicalFileService;
        private ReportConfigDBContext _contextData;
        private ICurrentContextService _currentContextService;
        private IDataReportService _dataReportService;

        private readonly Dictionary<string, PictureType> drImageType = new Dictionary<string, PictureType>
        {
            {".jpeg", PictureType.JPEG },
            {".jpg", PictureType.JPEG },
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

            numberOfColumns = columns.Count;

            await WriteHeader();

            WriteBody(reportInfo);
            WriteFooter();

            if (sheet.LastRowNum < 1000)
            {
                for (var i = 0; i < numberOfColumns; i++)
                {
                    sheet.AutoSizeColumn(i, false);
                }
            }
            else
            {
                for (var i = 0; i < numberOfColumns; i++)
                {
                    sheet.ManualResize(i, maxColumnCharLengths[i]);
                }
            }


            for (var i = 0; i < numberOfColumns; i++)
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
                var lCol = numberOfColumns - 1;

                if (_model.Header.fLogoId > 0)
                {
#if !DEBUG
                    var fileInfo = await _physicalFileService.GetSimpleFileInfo(_model.Header.fLogoId);
                    var pictureType = GetPictureType(Path.GetExtension(fileInfo.FileName).ToLower());

                    if (fileInfo != null && pictureType != PictureType.None)
                    {
                        fCol = 2;
                        var filePath = GetPhysicalFilePath(fileInfo.FilePath);
                        if (!File.Exists(filePath))
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, $"Logo file {fileInfo.FilePath} was not found!");
                        }
                        var fileStream = File.OpenRead(filePath);
                        byte[] bytes = IOUtils.ToByteArray(fileStream);
                        int pictureIdx = xssfwb.AddPicture(bytes, pictureType);
                        fileStream.Close();

                        var helper = xssfwb.GetCreationHelper();
                        var drawing = sheet.CreateDrawingPatriarch();
                        var anchor = helper.CreateClientAnchor();

                        anchor.Col1 = 0;
                        anchor.Row1 = 0;
                        anchor.Col2 = 2;
                        anchor.Row2 = 4;
                        anchor.AnchorType = AnchorType.MoveDontResize;
                        var picture = drawing.CreatePicture(anchor, pictureIdx);
                        picture.Resize(1, 1);
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
                    sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 0, numberOfColumns - 1));
                    sheet.EnsureCell(fRow, 0).SetCellValue(_model.Body.Title);
                    sheet.SetCellStyle(fRow, 0, vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center, fontSize: 18, isBold: true);
                }
                if (_model.Body.HeadDetails.Count > 0)
                {
                    int i = 1;
                    foreach (var info in _model.Body.HeadDetails)
                    {
                        int row = currentRow + i;
                        sheet.AddMergedRegion(new CellRangeAddress(row, row, 0, numberOfColumns - 1));
                        sheet.EnsureCell(row, 0).SetCellValue(info.Value);
                        sheet.SetCellStyle(row, 0, vAlign: VerticalAlignment.Top, hAlign: drTextAlign[info.TextAlign]);
                        i++;
                    }

                    currentRow += _model.Body.HeadDetails.Count;
                }
            }
        }

        Dictionary<int, int> maxColumnCharLengths = new Dictionary<int, int>();
        private void GenerateHeadTable(ReportType reportInfo)
        {
            int fRow, sRow;
            fRow = sRow = 0;
            byte[] headerRgb = new byte[3] { 221, 229, 239 };

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
                    maxColumnCharLengths.Add(i, columns[i].Name?.Length ?? 0);

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
                    maxColumnCharLengths.Add(i, columns[i].Name?.Length ?? 0);

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
            var sheet = xssfwb.GetSheet(sheetName);
            currentRow += 1;
            ExcelData table = new ExcelData();

            for (var index = 1; index <= columns.Count; index++)
            {
                table.Columns.Add($"Col-{index}");
            }
            var sumCalc = new Dictionary<int, ReportColumnModel>();
            var dataTable = _dataReportService.Report(reportInfo.ReportTypeId, _model.Body.FilterData, 1, 0).Result;

            int? firstGroupDataRow = null;
            int? lastGroupDataRow = null;
            string currentMergeValue = null;

            _mergeRows = new bool[dataTable.Rows.List.Count][];
            cellStyles = new ICellStyle[dataTable.Rows.List.Count + currentRow][];

            var mergeRanges = new List<CellRangeAddress>();
            for (var i = 0; i < dataTable.Rows.List.Count; i++)
            {
                var row = dataTable.Rows.List[i];

                ExcelRow tbRow = table.NewRow();
                int columnIndx = 0;
                var mergeValue = "";
                var isMergeRow = false;

                _mergeRows[i] = new bool[columns.Count];
                Array.Fill(_mergeRows[i], false);

                //customCellStyles
                ICellStyle rowStyle = null;
                var rowStyleStr = "";
                if (row.ContainsKey("$ROW_CSS_STYLE"))
                {
                    rowStyleStr = row["$ROW_CSS_STYLE"]?.ToString();
                    rowStyle = ParseCellStyle(sheet, null, rowStyleStr);
                }
                cellStyles[i + currentRow] = new ICellStyle[columns.Count];
                Array.Fill(cellStyles[i + currentRow], rowStyle);

                foreach (var field in columns)
                {
                    var charLengths = row[field.Alias]?.ToString()?.Length;
                    if (charLengths > maxColumnCharLengths[columnIndx])
                    {
                        maxColumnCharLengths[columnIndx] = charLengths.Value;
                    }

                    var cellStyleStr = "";
                    if (row.ContainsKey("$" + field.Alias + "_CSS_STYLE"))
                    {
                        cellStyleStr = row["$" + field.Alias + "_CSS_STYLE"]?.ToString();
                    }

                    ICellStyle cellStyle = ParseCellStyle(sheet, field, rowStyleStr, cellStyleStr); ;

                    if (field.IsCalcSum && !sumCalc.ContainsKey(columnIndx)) sumCalc.Add(columnIndx, field);
                    var dataType = field.DataTypeId.HasValue ? (EnumDataType)field.DataTypeId : EnumDataType.Text;

                    cellStyles[i + currentRow][columnIndx] = cellStyle;

                    if (row.ContainsKey(field.Alias))
                    {
                        tbRow[columnIndx] = new ExcelCell
                        {
                            Value = dataType.GetSqlValue(row[field.Alias], _currentContextService.TimeZoneOffset),
                            Type = dataType.GetExcelType(),
                            CellStyle = cellStyle
                        };
                    }

                    if (field.IsGroupRow)
                    {
                        mergeValue += "|" + row[field.Alias];
                        isMergeRow = true;

                    }
                    columnIndx++;
                }


                if (isMergeRow)
                {
                    if (currentMergeValue == mergeValue)
                    {
                        lastGroupDataRow = i;
                    }
                    else
                    {
                        if (firstGroupDataRow.HasValue)
                        {
                            columnIndx = 0;
                            foreach (var field in columns)
                            {
                                if (field.IsGroupRow && lastGroupDataRow > firstGroupDataRow)
                                {
                                    for (var s = firstGroupDataRow.Value; s <= lastGroupDataRow.Value; s++)
                                    {
                                        _mergeRows[s][columnIndx] = true;
                                    }
                                    mergeRanges.Add(new CellRangeAddress(firstGroupDataRow.Value + currentRow, lastGroupDataRow.Value + currentRow, columnIndx, columnIndx));
                                }

                                columnIndx++;
                            }
                        }

                        firstGroupDataRow = i;
                        lastGroupDataRow = i;
                        currentMergeValue = mergeValue;
                    }
                }
                tbRow.FillAllRow();
                table.Rows.Add(tbRow);
            }
            if (firstGroupDataRow.HasValue)
            {
                var columnIndx = 0;
                foreach (var field in columns)
                {
                    if (field.IsGroupRow && lastGroupDataRow > firstGroupDataRow)
                    {
                        for (var s = firstGroupDataRow.Value; s <= lastGroupDataRow.Value; s++)
                        {
                            _mergeRows[s][columnIndx] = true;
                        }
                        mergeRanges.Add(new CellRangeAddress(firstGroupDataRow.Value + currentRow, lastGroupDataRow.Value + currentRow, columnIndx, columnIndx));
                    }
                    columnIndx++;
                }
            }

            mergeRanges.AddRange(MergeColumn(table, dataTable));

            if (sumCalc.Count > 0)
            {
                ExcelRow sumRow = table.NewRow();
                foreach (var (index, column) in sumCalc)
                {
                    var dataType = column.DataTypeId.HasValue ? (EnumDataType)column.DataTypeId : EnumDataType.Text;
                    var columnName = (index + 1).GetExcelColumnName();
                    sumRow[index] = new ExcelCell
                    {
                        Value = $"SUM({columnName}{currentRow + 1}:{columnName}{currentRow + dataTable.Rows.List.Count()})",
                        Type = EnumExcelType.Formula,
                        CellStyle = GetCellStyle(sheet, column)
                    };
                }
                sumRow.FillAllRow();
                table.Rows.Add(sumRow);
            }

            xssfwb.WriteToSheet(sheet, table, out currentRow, startCollumn: 0, startRow: currentRow);
            var wb = xssfwb.GetWorkbook();
            mergeRanges.ForEach(m =>
            {
                sheet.AddMergedRegionUnsafe(m);
                RegionUtil.SetBorderBottom(1, m, sheet);
                RegionUtil.SetBorderLeft(1, m, sheet);
                RegionUtil.SetBorderRight(1, m, sheet);
                RegionUtil.SetBorderTop(1, m, sheet);
                if (cellStyles.Length > m.FirstRow && cellStyles[m.FirstRow] != null)
                    sheet.SetCellStyle(m.FirstRow, m.FirstColumn, cellStyles[m.FirstRow][m.FirstColumn]);
            });
        }

        private bool[][] _mergeRows = null;
        private ICellStyle[][] cellStyles = null;
        private List<CellRangeAddress> MergeColumn(ExcelData table, ReportDataModel dataTable)
        {
            var mergeRanges = new List<CellRangeAddress>();
            for (var i = 0; i < _mergeRows.Length; i++)
            {
                var row = dataTable.Rows.List[i];

                int columnIndx = 0;

                var isGroupColumn = false;
                var groupColumns = new Dictionary<int, (string[] columns, string value)>();
                if (row.ContainsKey("$GROUP_COLUMN"))
                {
                    isGroupColumn = !string.IsNullOrWhiteSpace(row["$GROUP_COLUMN"]?.ToString());
                    if (isGroupColumn)
                    {
                        var groups = row["$GROUP_COLUMN"]?.ToString().Split('|');
                        for (var j = 0; j < groups.Length; j++)
                        {
                            var groupConfig = groups[j].Split('=');
                            var columns = groupConfig[0].Split(',').Select(c => c.Trim()).ToArray();
                            var value = groupConfig.Length > 1 ? groups[j].Split('=')[1] : "";
                            groupColumns.Add(j, (columns, value));
                        }
                    }
                }
                int? firstGroupDataColumn = null;
                int? lastGroupDataColumn = null;
                int? currentMergeColumnValue = null;
                string dataValue = "";

                foreach (var field in columns)
                {
                    if (!_mergeRows[i][columnIndx])
                    {
                        int? mergeColumnValue = null;
                        foreach (var g in groupColumns)
                        {
                            foreach (var c in g.Value.columns)
                            {
                                if (c == field.Alias || c == "*")
                                {
                                    mergeColumnValue = g.Key;
                                    dataValue = g.Value.value;
                                }
                            }
                        }

                        if (mergeColumnValue == currentMergeColumnValue)
                        {
                            lastGroupDataColumn = columnIndx;
                        }
                        else
                        {
                            if (currentMergeColumnValue != null && firstGroupDataColumn.HasValue && lastGroupDataColumn > firstGroupDataColumn)
                            {
                                mergeRanges.Add(new CellRangeAddress(i + currentRow, i + currentRow, firstGroupDataColumn.Value, lastGroupDataColumn.Value));
                                if (row.ContainsKey(dataValue))
                                {
                                    var rowCell = table.Rows[i];
                                    rowCell[firstGroupDataColumn.Value] = new ExcelCell() { Value = row[dataValue]?.ToString() };

                                }
                            }

                            currentMergeColumnValue = mergeColumnValue;
                            firstGroupDataColumn = columnIndx;
                            lastGroupDataColumn = columnIndx;

                        }
                    }

                    columnIndx++;
                }
                if (currentMergeColumnValue != null && firstGroupDataColumn.HasValue && lastGroupDataColumn > firstGroupDataColumn)
                {
                    mergeRanges.Add(new CellRangeAddress(i + currentRow, i + currentRow, firstGroupDataColumn.Value, lastGroupDataColumn.Value));
                    if (row.ContainsKey(dataValue))
                    {
                        var rowCell = table.Rows[i];
                        rowCell[firstGroupDataColumn.Value] = new ExcelCell() { Value = row[dataValue]?.ToString() };
                    }
                }
            }

            return mergeRanges;
        }

        Dictionary<string, ICellStyle> dataTypeStyles = new Dictionary<string, ICellStyle>();

        private ICellStyle GetCellStyle(ISheet sheet, ReportColumnModel column)
        {
            if (dataTypeStyles.ContainsKey(column.Alias))
            {
                return dataTypeStyles[column.Alias];
            }
            var type = column.DataTypeId.HasValue ? (EnumDataType)column.DataTypeId : EnumDataType.Text;

            var vAlign = column.VAlign?.GetVerticalAlignment();
            var hAlign = column.HAlign?.GetHorizontalAlignment();

            ICellStyle style;
            switch (type)
            {
                case EnumDataType.Boolean:
                    style = sheet.GetCellStyle(vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Left, isWrap: true, isBorder: true);
                    dataTypeStyles.Add(column.Alias, style);
                    return style;
                case EnumDataType.Int:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:

                case EnumDataType.BigInt:
                case EnumDataType.Decimal:
                    {
                        var format = new StringBuilder("#,##0");
                        if (column.DecimalPlace.GetValueOrDefault() > 0)
                        {
                            format.Append(".0");
                            for (int i = 1; i < column.DecimalPlace; i++)
                            {
                                format.Append("#");
                            }
                        }
                        style = sheet.GetCellStyle(vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: format.ToString());
                        dataTypeStyles.Add(column.Alias, style);
                        return style;
                    }
                case EnumDataType.Date:
                    style = sheet.GetCellStyle(vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: "dd/mm/yyyy");
                    dataTypeStyles.Add(column.Alias, style);
                    return style;
                case EnumDataType.Percentage:
                    {
                        var format = new StringBuilder("0");
                        if (column.DecimalPlace.GetValueOrDefault() > 0)
                        {
                            format.Append(".0");
                            for (int i = 1; i < column.DecimalPlace; i++)
                            {
                                format.Append("#");
                            }
                        }
                        format.Append(" %");
                        style = sheet.GetCellStyle(vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: format.ToString());
                        dataTypeStyles.Add(column.Alias, style);
                        return style;
                    }
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    style = sheet.GetCellStyle(vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Left, isWrap: true, isBorder: true);
                    dataTypeStyles.Add(column.Alias, style);
                    return style;
            }
        }

        Dictionary<string, ICellStyle> customCellStyles = new Dictionary<string, ICellStyle>();
        private ICellStyle ParseCellStyle(ISheet sheet, ReportColumnModel column, params string[] styleConfigs)
        {
            ICellStyle defaultColumnStyle = null;
            if (column != null)
            {
                defaultColumnStyle = GetCellStyle(sheet, column);
            }

            if (styleConfigs == null || styleConfigs.All(s => string.IsNullOrWhiteSpace(s)))
            {
                return defaultColumnStyle;
            }

            var cached = string.Join("", styleConfigs) + "|" + column?.Alias;
            if (customCellStyles.ContainsKey(cached))
            {
                return customCellStyles[cached];
            }

            var defaultFont = defaultColumnStyle?.GetFont(sheet.Workbook);

            int fontSize = (int?)defaultFont?.FontHeightInPoints ?? 11;
            bool isBold = defaultFont?.IsBold ?? false;
            bool isItalic = defaultFont?.IsItalic ?? false;
            byte[] bgColor = null;
            byte[] color = null;
            VerticalAlignment? vAlign = defaultColumnStyle?.VerticalAlignment;
            HorizontalAlignment? hAlign = defaultColumnStyle?.Alignment;
            string format = defaultColumnStyle?.GetDataFormatString();
            foreach (var styleConfig in styleConfigs)
            {
                if (string.IsNullOrWhiteSpace(styleConfig)) continue;

                var cfgs = JObject.Parse(styleConfig);

                if (cfgs.ContainsKey("fontSize"))
                {
                    var value = cfgs.Value<string>("fontSize");

                    if (value.Contains("px") || value.Contains("pt"))
                    {
                        var v = new Regex("[^0-9]");
                        var nunber = v.Replace(value, "");
                        int.TryParse(nunber, out fontSize);
                    }
                }
                if (cfgs.ContainsKey("fontWeight"))
                {
                    var value = cfgs.Value<string>("fontWeight");
                    isBold = value.Contains("bold", StringComparison.OrdinalIgnoreCase);
                }
                if (cfgs.ContainsKey("fontStyle"))
                {
                    var value = cfgs.Value<string>("fontWeight");
                    isBold = value.Contains("italic", StringComparison.OrdinalIgnoreCase);
                }

                if (cfgs.ContainsKey("verticalAlign"))
                {
                    var value = cfgs.Value<string>("verticalAlign");
                    vAlign = value?.GetVerticalAlignment();
                }

                if (cfgs.ContainsKey("textAlign"))
                {
                    var value = cfgs.Value<string>("textAlign");

                    hAlign = value?.GetHorizontalAlignment();
                }

                if (cfgs.ContainsKey("background"))
                {
                    var value = cfgs.Value<string>("background");
                    bgColor = value?.HexadecimalToRGB();
                }

                if (cfgs.ContainsKey("color"))
                {
                    var value = cfgs.Value<string>("color");
                    color = value?.HexadecimalToRGB();
                }

                if (cfgs.ContainsKey("format"))
                {
                    format = cfgs.Value<string>("format");
                }

            }

            var style = sheet.GetCellStyle(fontSize, isBold, isItalic, vAlign, hAlign, isBorder: true, rgb: bgColor, color: color, dataFormat: format);
            customCellStyles.Add(cached, style);
            return style;
        }



        private void WriteFooter()
        {
            var signs = _model.Footer?.SignText?.Split("@");

            if (signs != null && signs.Length > 0)
            {
                int fRow = currentRow + 3;

                var rCol = (int)numberOfColumns / signs.Length;
                for (int i = 0; i < signs.Length; i++)
                {
                    var fCol = i > 0 ? (i * rCol) : 0;
                    var lCol = (i + 1) * rCol - 1;
                    if (i == signs.Length - 1 && numberOfColumns > (signs.Length * rCol)) lCol += (numberOfColumns - (signs.Length * rCol));
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
                sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 0, numberOfColumns - 1));
                sheet.EnsureCell(fRow, 0).SetCellValue(_model.Footer.RDateText);
                sheet.SetCellStyle(fRow, 0, vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center);
            }

            currentRow++;
        }

        private PictureType GetPictureType(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && drImageType.ContainsKey(extension))
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
        internal void SetCurrentContextService(ICurrentContextService currentContextService) => _currentContextService = currentContextService;
        internal void SetDataReportService(IDataReportService dataReportService) => _dataReportService = dataReportService;
    }
}
