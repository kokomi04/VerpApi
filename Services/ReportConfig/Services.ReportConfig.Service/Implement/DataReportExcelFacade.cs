﻿using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    internal class DataReportExcelFacade
    {
        private const int CONDITION_VALUE = 1;
        private ISheet sheet = null;
        private ExcelWriter xssfwb = null;
        private int currentRow = 0;
        private int numberOfColumns = 15;
        private ReportFacadeModel _model;
        private IList<ReportColumnModel> originalColumns = null;
        private IList<ReportColumnModel> groupRowColumns = null;
        private bool isMergeRow = false;
        private IList<ReportColumnModel> visibleColumns = null;
        private IList<ReportColumnModel> allValueColumns = null;
        private ReportDataModel dataTable = null;

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

        public async Task<(Stream stream, string fileName, string contentType)> ReportExport(int reportId, ReportFacadeModel model)
        {
            var reportInfo = await _contextData.ReportType.AsNoTracking().FirstOrDefaultAsync(r => r.ReportTypeId == reportId);

            if (reportInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            xssfwb = new ExcelWriter();
            sheet = xssfwb.GetSheet(sheetName);


            _model = model;

            originalColumns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();
            foreach (var c in originalColumns.OrderBy(s => s.SortOrder))
            {
                c.ColGroupName = originalColumns.FirstOrDefault(c1 => c1.ColGroupId == c.ColGroupId)?.ColGroupName;
            }

            groupRowColumns = originalColumns.Where(c => c.IsGroupRow).ToList();

            isMergeRow = groupRowColumns.Any(c => !c.IsHidden);


            var size = 0;
            if (reportInfo.IsDbPaging == true)
                size = int.MaxValue;
            dataTable = _dataReportService.Report(reportInfo.ReportTypeId, _model.Body.FilterData, 1, size).Result;

            var firstRow = RepeatColumnUtils.GetFistRow(dataTable.Rows.List);

            allValueColumns = RepeatColumnUtils.RepeatColumnAndSortProcess(originalColumns, firstRow);

            groupRowColumns = allValueColumns.Where(c => c.IsGroupRow).ToList();

            if (firstRow != null && firstRow.Count > 0)
            {
                foreach (var c in allValueColumns)
                {
                    if (firstRow.TryGetStringValue($"${c.Alias}_CONFIG", out var strCfg))
                    {
                        var style = ParseCellStyle(sheet, c, strCfg);
                        if (style.IsHidden)
                        {
                            c.IsHidden = true;
                        }
                    }

                }
            }


            visibleColumns = allValueColumns.Where(col => !col.IsHidden).ToList();



            numberOfColumns = visibleColumns.Count;

            await WriteHeader();

            WriteBody();

            WriteFooter();



            if (sheet.LastRowNum < 100)
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
                    sheet.ManualResize(i, maxColumnLineLengths[i]);
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

            var stream = xssfwb.WriteToStream();
            stream.Seek(0, SeekOrigin.Begin);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = GetFileName(model.Body.FilterData, reportInfo.ReportTypeName);
            fileName = StringUtils.RemoveDiacritics($"{fileName}.xlsx").Replace(" ", "#");
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
                    var fileInfo = await _physicalFileService.GetSimpleFileInfo(_model.Header.fLogoId);
#if !DEBUG
                   
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

        private void WriteBody()
        {
            GenerateBodyInfo();
            currentRow += 2;
            GenerateHeadTable();
            GenerateDataTable();
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

        Dictionary<int, int> maxColumnLineLengths = new Dictionary<int, int>();
        readonly byte[] headerRgb = new byte[3] { 221, 229, 239 };
        private void GenerateHeadTable()
        {
            int fRow, sRow;
            fRow = sRow = 0;

            var groupColumns = visibleColumns
                .GroupBy(c => new { c.ColGroupId, c.SuffixKey })
                .OrderBy(g => g.Key.ColGroupId)
                .ThenBy(g => g.Key.SuffixKey)
                .ToList();
            var isGroup = groupColumns.Any(g => g.Count() > 1 || g.First().IsColGroup);

            if (isGroup) sRow = 1;

            fRow = currentRow;
            sRow = fRow + sRow;

            if (isGroup)
            {

                var columnIndex = 0;
                var headStyle = sheet.GetCellStyle(12, true, false, VerticalAlignment.Center, HorizontalAlignment.Center, true, true, headerRgb);

                foreach (var group in groupColumns)
                {
                    if (group.Count() == 1 && !group.First().IsColGroup)
                    {
                        var cell = sheet.EnsureCell(fRow, columnIndex);
                        cell.SetCellValue(group.FirstOrDefault()?.Name);
                        cell.CellStyle = headStyle;

                        var mergeRegion = new CellRangeAddress(fRow, fRow + 1, columnIndex, columnIndex);
                        if (mergeRegion.FirstRow != mergeRegion.LastRow || mergeRegion.FirstColumn != mergeRegion.LastColumn)
                        {
                            sheet.AddMergedRegion(mergeRegion);

                            RegionUtil.SetBorderBottom(1, mergeRegion, sheet);
                            RegionUtil.SetBorderLeft(1, mergeRegion, sheet);
                            RegionUtil.SetBorderRight(1, mergeRegion, sheet);
                            RegionUtil.SetBorderTop(1, mergeRegion, sheet);
                        }

                        columnIndex++;
                    }
                    else
                    {
                        var cols = group.OrderBy(c => c.SortOrder);

                        var cell0 = sheet.EnsureCell(fRow, columnIndex);
                        cell0.SetCellValue(group.FirstOrDefault()?.ColGroupName);
                        cell0.CellStyle = headStyle;

                        var mergeRegion = new CellRangeAddress(fRow, fRow, columnIndex, columnIndex + cols.Count() - 1);
                        if (mergeRegion.FirstRow != mergeRegion.LastRow || mergeRegion.FirstColumn != mergeRegion.LastColumn)
                        {
                            sheet.AddMergedRegion(mergeRegion);
                            RegionUtil.SetBorderBottom(1, mergeRegion, sheet);
                            RegionUtil.SetBorderLeft(1, mergeRegion, sheet);
                            RegionUtil.SetBorderRight(1, mergeRegion, sheet);
                            RegionUtil.SetBorderTop(1, mergeRegion, sheet);
                        }

                        foreach (var child in cols)
                        {
                            var cell1 = sheet.EnsureCell(fRow + 1, columnIndex);
                            cell1.SetCellValue(child.Name);
                            cell1.CellStyle = headStyle;
                            columnIndex++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < visibleColumns.Count; i++)
                {
                    sheet.EnsureCell(fRow, i).SetCellValue(visibleColumns[i].Name);
                    sheet.SetCellStyle(fRow, i,
                        vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                        rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                }
            }

            for (int i = 0; i < visibleColumns.Count; i++)
            {
                var nameLineLength = visibleColumns[i].Name?.Split('\n')?.Select(l => l.Length)?.Max() ?? 0;
                var groupLineLength = visibleColumns[i].ColGroupName?.Split('\n')?.Select(l => l.Length)?.Max() ?? 0;
                maxColumnLineLengths.Add(i, Math.Max(nameLineLength, groupLineLength));
            }
            currentRow = sRow;
        }



        private void GenerateDataTable()
        {
            var sheet = xssfwb.GetSheet(sheetName);
            currentRow += 1;
            ExcelData table = new ExcelData();

            for (var index = 1; index <= visibleColumns.Count; index++)
            {
                table.Columns.Add($"Col-{index}");
            }


            var conditionSumColumns = originalColumns.Select(c => c.CalcSumConditionCol).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToArray();
            for (var conditionIndex = 0; conditionIndex < conditionSumColumns.Length; conditionIndex++)
            {
                var col = conditionSumColumns[conditionIndex];
                sheet.SetColumnHidden(visibleColumns.Count + conditionIndex, true);
                table.Columns.Add($"Col-{visibleColumns.Count + conditionIndex + 1}");
            }


            var sumCalc = new Dictionary<int, ReportColumnModel>();
            //var sumValues = new Dictionary<int, decimal>();

            int? firstGroupDataRow = null;
            int? lastGroupDataRow = null;
            string currentMergeValue = null;

            _mergeRows = new bool[dataTable.Rows.List.Count][];
            cellStyles = new ICellStyle[dataTable.Rows.List.Count + currentRow][];


            var mergeRanges = new List<CellRangeAddress>();

            var groups = dataTable.Rows.List.GroupBy(row => string.Join('|', groupRowColumns.Select(c => row[c.Alias])));
            var i = 0;
            foreach (var groupRow in groups)
            {
                foreach (var row in groupRow)
                {
                    ExcelRow tbRow = table.NewRow();
                    int columnIndx = 0;

                    _mergeRows[i] = new bool[visibleColumns.Count];
                    Array.Fill(_mergeRows[i], false);

                    //customCellStyles
                    ICellStyle rowStyle = null;
                    var rowStyleStr = "";
                    if (row.ContainsKey(ReportSpecialColumnConstants.ROW_CSS_STYLE_ALIAS))
                    {
                        rowStyleStr = row[ReportSpecialColumnConstants.ROW_CSS_STYLE_ALIAS]?.ToString();
                        rowStyle = ParseCellStyle(sheet, null, rowStyleStr);
                    }
                    cellStyles[i + currentRow] = new ICellStyle[visibleColumns.Count];
                    Array.Fill(cellStyles[i + currentRow], rowStyle);

                    foreach (var field in visibleColumns)
                    {
                        var charLengths = row[field.Alias]?.ToString()?.Length;
                        if (charLengths > maxColumnLineLengths[columnIndx])
                        {
                            maxColumnLineLengths[columnIndx] = charLengths.Value;
                        }

                        var cellStyleStr = "";
                        var cellStyleAlias = string.Format(ReportSpecialColumnConstants.ROW_COLUMN_CSS_STYLE_ALIAS_FORMAT, field.Alias);
                        if (row.ContainsKey(cellStyleAlias))
                        {
                            cellStyleStr = row[cellStyleAlias]?.ToString();
                        }
                        ICellStyle cellStyle = ParseCellStyle(sheet, field, rowStyleStr, cellStyleStr);

                        if (field.IsCalcSum && !sumCalc.ContainsKey(columnIndx))
                        {
                            sumCalc.Add(columnIndx, field);
                            //sumValues.Add(columnIndx, 0);
                        }
                        var dataType = field.DataTypeId.HasValue ? (EnumDataType)field.DataTypeId : EnumDataType.Text;
                        
                        cellStyles[i + currentRow][columnIndx] = cellStyle;

                        if (row.ContainsKey(field.Alias))
                        {
                            var value = dataType.GetSqlValueAtTimezone(row[field.Alias], _currentContextService.TimeZoneOffset);
                            tbRow[columnIndx] = new ExcelCell
                            {
                                Value = value,
                                Type = dataType.GetExcelType(),
                                CellStyle = cellStyle
                            };

                        }

                        columnIndx++;
                    }



                    if (isMergeRow)
                    {
                        var mergeValue = string.Join('|', groupRowColumns.Select(c => row[c.Alias]));

                        if (currentMergeValue == mergeValue)
                        {
                            lastGroupDataRow = i;
                        }
                        else
                        {
                            if (firstGroupDataRow.HasValue)
                            {
                                columnIndx = 0;
                                foreach (var field in visibleColumns)
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


                    for (var conditionIndex = 0; conditionIndex < conditionSumColumns.Length; conditionIndex++)
                    {
                        var col = conditionSumColumns[conditionIndex];
                        if (row.ContainsKey(col))
                        {
                            var v = row[col];
                            long.TryParse(v?.ToString(), out var vInNumber);
                            if (v == (object)true || vInNumber > 0)
                            {
                                tbRow[columnIndx] = new ExcelCell
                                {
                                    Value = CONDITION_VALUE,
                                    Type = EnumDataType.Int.GetExcelType()
                                };
                            }

                        }

                        columnIndx++;

                    }


                    tbRow.FillAllRow();
                    table.Rows.Add(tbRow);
                    i++;
                }
            }


            if (firstGroupDataRow.HasValue)
            {
                var columnIndx = 0;
                foreach (var field in visibleColumns)
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

                    var conditionColum = conditionSumColumns.FirstOrDefault(c => c == column.CalcSumConditionCol);

                    var sumRange = $"{columnName}{currentRow + 1}:{columnName}{currentRow + dataTable.Rows.List.Count()}";
                    if (!string.IsNullOrWhiteSpace(column.CalcSumConditionCol) && conditionColum != null)
                    {
                        var aliasIndex = visibleColumns.Count + Array.IndexOf(conditionSumColumns, conditionColum);
                        var aliasName = (aliasIndex + 1).GetExcelColumnName();

                        var conditionRange = $"{aliasName}{currentRow + 1}:{aliasName}{currentRow + dataTable.Rows.List.Count()}";
                        sumRow[index] = new ExcelCell
                        {
                            Value = $"SUMIF({conditionRange},{CONDITION_VALUE},{sumRange})",
                            Type = EnumExcelType.Formula,
                            CellStyle = GetCellStyle(sheet, column, true)
                        };
                    }
                    else
                    {
                        sumRow[index] = new ExcelCell
                        {
                            Value = $"SUM({sumRange})",
                            Type = EnumExcelType.Formula,
                            CellStyle = GetCellStyle(sheet, column, true)
                        };
                    }

                    //sumRow[index] = new ExcelCell
                    //{
                    //    Value = sumValues[index],
                    //    Type = EnumExcelType.Number,
                    //    CellStyle = GetCellStyle(sheet, column, true)
                    //};

                }
                var columnIndx = 0;
                foreach (var field in visibleColumns)
                {
                    if (!sumCalc.ContainsKey(columnIndx))
                    {
                        sumRow[columnIndx] = new ExcelCell
                        {
                            Value = $"",
                            Type = EnumExcelType.String,
                            CellStyle = GetCellStyle(sheet, field, true)
                        };
                    }
                    columnIndx++;

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
        private int GetDecimalPlace(string value)
        {
            var decimalPlace = value.Split('.').Count() > 1 && value.Split('.').Count() < 3 ? value.Split('.').LastOrDefault() : null;
            return decimalPlace == null ? 0 : decimalPlace.Length;
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

                foreach (var field in visibleColumns)
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

        private ICellStyle GetCellStyle(ISheet sheet, ReportColumnModel column, bool isHeader = false)
        {
            var keyCached = column.Alias + "|" + isHeader;
            if (dataTypeStyles.ContainsKey(keyCached))
            {
                return dataTypeStyles[keyCached];
            }
            byte[] bgColor = null;
            bool isBold = false;
            if (isHeader)
            {
                bgColor = headerRgb;
                isBold = true;
            }
            var type = column.DataTypeId.HasValue ? (EnumDataType)column.DataTypeId : EnumDataType.Text;

            var vAlign = column.VAlign?.GetVerticalAlignment();
            var hAlign = column.HAlign?.GetHorizontalAlignment();

            ICellStyle style;
            switch (type)
            {
                case EnumDataType.Boolean:
                    style = sheet.GetCellStyle(isBold: isBold, vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Left, isWrap: true, isBorder: true);
                    dataTypeStyles.Add(keyCached, style);
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
                        style = sheet.GetCellStyle(isBold: isBold, vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, rgb: bgColor, dataFormat: format.ToString());
                        dataTypeStyles.Add(keyCached, style);
                        return style;
                    }
                case EnumDataType.Date:
                    style = sheet.GetCellStyle(isBold: isBold, vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, rgb: bgColor, dataFormat: "dd/mm/yyyy");
                    dataTypeStyles.Add(keyCached, style);
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
                        style = sheet.GetCellStyle(isBold: isBold, vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Right, isWrap: true, isBorder: true, rgb: bgColor, dataFormat: format.ToString());
                        dataTypeStyles.Add(keyCached, style);
                        return style;
                    }
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    style = sheet.GetCellStyle(isBold: isBold, vAlign: vAlign ?? VerticalAlignment.Top, hAlign: hAlign ?? HorizontalAlignment.Left, isWrap: true, isBorder: true, rgb: bgColor);
                    dataTypeStyles.Add(keyCached, style);
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
            short? indention = null;
            bool? isHidden = null;

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
                    var value = cfgs.Value<string>("fontStyle");
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



                if (cfgs.ContainsKey("paddingLeft"))
                {
                    var value = cfgs.Value<string>("paddingLeft");
                    if (value.Contains("px") || value.Contains("pt"))
                    {
                        var v = new Regex("[^0-9]");
                        var nunber = v.Replace(value, "");
                        int.TryParse(nunber, out var paddingLeft);

                        indention = Convert.ToInt16(paddingLeft / 10);
                    }

                }

                if (cfgs.ContainsKey("isHidden"))
                {
                    isHidden = cfgs.Value<bool>("isHidden");
                }

            }

            var style = sheet.GetCellStyle(fontSize, isBold, isItalic, vAlign, hAlign, isBorder: true, rgb: bgColor, color: color, dataFormat: format, indention: indention, isHidden: isHidden);
            customCellStyles.Add(cached, style);
            return style;
        }



        private void WriteFooter()
        {
            var signs = _model.Footer?.SignText?.Split("@");

            if (signs != null && signs.Length > 0)
            {
                if (signs.Length > numberOfColumns) return;

                int fRow = currentRow + 3;

                var rCol = (int)numberOfColumns / signs.Length;
                for (int i = 0; i < signs.Length; i++)
                {
                    var fCol = i > 0 ? (i * rCol) : 0;
                    var lCol = (i + 1) * rCol - 1;
                    if (i == signs.Length - 1 && numberOfColumns > (signs.Length * rCol)) lCol += (numberOfColumns - (signs.Length * rCol));
                    if (lCol > fCol)
                        sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, fCol, lCol));
                    sheet.EnsureCell(fRow, fCol).SetCellValue(signs[i].Replace("/", $"\r\n"));
                    sheet.SetSignatureCellStyle(fRow, fCol);

                    if (i == signs.Length - 1 && !string.IsNullOrEmpty(_model.Footer.RDateText))
                    {
                        var rIndex = fRow - 1;
                        if (lCol > fCol)
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
        private string GetFileName(ReportFilterDataModel filters, string fileName)
        {
            var fromDate = "";
            var toDate = "";
            foreach (var key in filters.Filters.Keys)
            {
                if (key.ToLower().Contains("fromdate") && !filters.Filters[key].IsNullOrEmptyObject())
                {
                    fromDate = Convert.ToInt64(filters.Filters[key]).UnixToDateTime(_currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy");
                }
                if (key.ToLower().Contains("todate") && !filters.Filters[key].IsNullOrEmptyObject())
                {
                    toDate = Convert.ToInt64(filters.Filters[key]).UnixToDateTime(_currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy");
                }
            }
            if (!"".Equals(fromDate)) fileName = $"{fileName} {fromDate}";
            if (!"".Equals(toDate)) fileName = $"{fileName} {toDate}";
            return fileName;
        }
    }
}
