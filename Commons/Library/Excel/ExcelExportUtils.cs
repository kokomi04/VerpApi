using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Commons.Library.Excel
{
    public class ExcelExportUtils
    {
        private const int CONDITION_VALUE = 1;
        private ISheet sheet = null;
        private ExcelWriter xssfwb = null;
        private int currentRow = 0;
        private int numberOfColumns = 15;
        private IList<ExelExportColumn> columns = null;
        private readonly NonCamelCaseDictionary headerData;
        private readonly int numberOfHeaderTitleCell;

        private readonly string sheetName = "Data";

        private IList<NonCamelCaseDictionary> data;


        private ICurrentContextService currentContextService;

        private string title;
        public ExcelExportUtils(string title, ICurrentContextService currentContextService, IList<NonCamelCaseDictionary> data, IList<ExelExportColumn> columns, NonCamelCaseDictionary headerData, int numberOfHeaderTitleCell)
        {
            this.title = title;
            this.currentContextService = currentContextService;
            this.data = data;

            foreach (var col in columns)
            {
                if (string.IsNullOrWhiteSpace(col.GroupName))
                {
                    col.GroupName = col.FieldName;
                }
            }
            this.columns = columns;

            this.headerData = headerData;
            this.numberOfHeaderTitleCell = numberOfHeaderTitleCell;

            xssfwb = new ExcelWriter();
            sheet = xssfwb.GetSheet(sheetName);

            numberOfColumns = columns.Count;
        }

        public async Task<(Stream stream, string fileName, string contentType)> WriteExcel()
        {

            WriteHeader();

            WriteBody();


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

            var stream = await xssfwb.WriteToStream();
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fName = Utils.RemoveDiacritics($"{title} {DateTime.UtcNow.ToString("dd_MM_yyyy")}.xlsx").Replace(" ", "_");
            return (stream, fName, contentType);

        }


        public void WriteHeader()
        {
            currentRow++;


            var titleCell = sheet.EnsureCell(currentRow, 0);
            xssfwb.WriteCellValue(titleCell, new ExcelCell()
            {
                Value = title,
                CellStyle = null,
                Type = EnumExcelType.String
            });

            sheet.SetCellStyle(currentRow, 0, 14, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Center, isWrap: true, isBold: true);

            sheet.AddMergedRegionUnsafe(new CellRangeAddress(currentRow, currentRow, 0, columns.Count));

            currentRow += 2;

            foreach (var h in headerData)
            {
                for (var c = 0; c < numberOfHeaderTitleCell; c++)
                {
                    var cell = sheet.EnsureCell(currentRow, c);
                    if (c == 0)
                    {
                        cell.SetCellValue(h.Key);
                    }
                }
                if (numberOfHeaderTitleCell > 1)
                {
                    sheet.AddMergedRegionUnsafe(new CellRangeAddress(currentRow, currentRow, 0, numberOfHeaderTitleCell - 1));
                }

                xssfwb.WriteCellValue(sheet.EnsureCell(currentRow, numberOfHeaderTitleCell), new ExcelCell()
                {
                    Value = h.Value,
                    CellStyle = null,
                    Type = h.Value?.GetType()?.GetDataType().GetExcelType() ?? EnumExcelType.String
                });

                currentRow++;
            }
        }


        private void WriteBody()
        {
            currentRow += 2;
            GenerateHeadTable();
            GenerateDataTable();
        }


        Dictionary<int, int> maxColumnLineLengths = new Dictionary<int, int>();
        readonly byte[] headerRgb = new byte[3] { 221, 229, 239 };
        private void GenerateHeadTable()
        {
            int fRow, sRow;
            fRow = sRow = 0;

            var groupColumns = columns
                .GroupBy(c => c.GroupName)
                .ToList();

            var isGroup = groupColumns.Any(g => g.Count() > 1);

            if (isGroup) sRow = 1;

            fRow = currentRow;
            sRow = fRow + sRow;

            if (isGroup)
            {

                var columnIndex = 0;
                var headStyle = sheet.GetCellStyle(12, true, false, VerticalAlignment.Center, HorizontalAlignment.Center, true, true, headerRgb);

                foreach (var group in groupColumns)
                {
                    if (group.Count() == 1)
                    {
                        var cell = sheet.EnsureCell(fRow, columnIndex);
                        cell.SetCellValue(group.FirstOrDefault()?.GroupName);
                        cell.CellStyle = headStyle;

                        var mergeRegion = new CellRangeAddress(fRow, fRow + 1, columnIndex, columnIndex);
                        sheet.AddMergedRegion(mergeRegion);

                        RegionUtil.SetBorderBottom(1, mergeRegion, sheet);
                        RegionUtil.SetBorderLeft(1, mergeRegion, sheet);
                        RegionUtil.SetBorderRight(1, mergeRegion, sheet);
                        RegionUtil.SetBorderTop(1, mergeRegion, sheet);

                        columnIndex++;
                    }
                    else
                    {
                        var cols = group.ToList();

                        var cell0 = sheet.EnsureCell(fRow, columnIndex);
                        cell0.SetCellValue(group.FirstOrDefault()?.GroupName);
                        cell0.CellStyle = headStyle;

                        var mergeRegion = new CellRangeAddress(fRow, fRow, columnIndex, columnIndex + cols.Count() - 1);
                        sheet.AddMergedRegion(mergeRegion);
                        RegionUtil.SetBorderBottom(1, mergeRegion, sheet);
                        RegionUtil.SetBorderLeft(1, mergeRegion, sheet);
                        RegionUtil.SetBorderRight(1, mergeRegion, sheet);
                        RegionUtil.SetBorderTop(1, mergeRegion, sheet);

                        foreach (var child in cols)
                        {
                            var cell1 = sheet.EnsureCell(fRow + 1, columnIndex);
                            cell1.SetCellValue(child.Title);
                            cell1.CellStyle = headStyle;
                            columnIndex++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    sheet.EnsureCell(fRow, i).SetCellValue(columns[i].Title);
                    sheet.SetCellStyle(fRow, i,
                        vAlign: VerticalAlignment.Center, hAlign: HorizontalAlignment.Center,
                        rgb: headerRgb, isBold: true, fontSize: 12, isBorder: true);
                }
            }

            for (int i = 0; i < columns.Count; i++)
            {
                var nameLineLength = columns[i].Title?.Split('\n')?.Select(l => l.Length)?.Max() ?? 0;
                var groupLineLength = columns[i].GroupName?.Split('\n')?.Select(l => l.Length)?.Max() ?? 0;
                maxColumnLineLengths.Add(i, Math.Max(nameLineLength, groupLineLength));
            }
            currentRow = sRow;
        }


        private bool[][] _mergeRows = null;
        private ICellStyle[][] cellStyles = null;
        private void GenerateDataTable()
        {
            var sheet = xssfwb.GetSheet(sheetName);
            currentRow += 1;
            ExcelData table = new ExcelData();

            for (var index = 1; index <= columns.Count; index++)
            {
                table.Columns.Add($"Col-{index}");
            }


            var conditionHiddenColumns = columns.Select(c => c.CalcSumConditionCol).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToArray();
            for (var conditionIndex = 0; conditionIndex < conditionHiddenColumns.Length; conditionIndex++)
            {
                var col = conditionHiddenColumns[conditionIndex];
                sheet.SetColumnHidden(columns.Count + conditionIndex, true);
                table.Columns.Add($"Col-{columns.Count + conditionIndex + 1}");
            }


            var sumCalc = new Dictionary<int, ExelExportColumn>();
            //var sumValues = new Dictionary<int, decimal>();


            _mergeRows = new bool[data.Count][];
            cellStyles = new ICellStyle[data.Count + currentRow][];


            var mergeRanges = new List<CellRangeAddress>();
            for (var i = 0; i < data.Count; i++)
            {
                var row = data[i];

                ExcelRow tbRow = table.NewRow();
                int columnIndx = 0;

                _mergeRows[i] = new bool[columns.Count];
                Array.Fill(_mergeRows[i], false);

                //customCellStyles
                //ICellStyle rowStyle = null;
                //var rowStyleStr = "";
                //if (row.ContainsKey(ReportSpecialColumnConstants.ROW_CSS_STYLE_ALIAS))
                //{
                //    rowStyleStr = row[ReportSpecialColumnConstants.ROW_CSS_STYLE_ALIAS]?.ToString();
                //    rowStyle = ParseCellStyle(sheet, null, rowStyleStr);
                //}
                cellStyles[i + currentRow] = new ICellStyle[columns.Count];
                //Array.Fill(cellStyles[i + currentRow], rowStyle);

                foreach (var field in columns)
                {
                    var charLengths = row[field.FieldName]?.ToString()?.Length;
                    if (charLengths > maxColumnLineLengths[columnIndx])
                    {
                        maxColumnLineLengths[columnIndx] = charLengths.Value;
                    }

                    //var cellStyleStr = "";
                    //var cellStyleAlias = string.Format(ReportSpecialColumnConstants.ROW_COLUMN_CSS_STYLE_ALIAS_FORMAT, field.Alias);
                    //if (row.ContainsKey(cellStyleAlias))
                    //{
                    //    cellStyleStr = row[cellStyleAlias]?.ToString();
                    //}

                    //ICellStyle cellStyle = ParseCellStyle(sheet, field, rowStyleStr, cellStyleStr); ;

                    if (field.IsCalcSum && !sumCalc.ContainsKey(columnIndx))
                    {
                        sumCalc.Add(columnIndx, field);
                        //sumValues.Add(columnIndx, 0);
                    }
                    var dataType = field.DataTypeId.HasValue ? (EnumDataType)field.DataTypeId : EnumDataType.Text;

                    // cellStyles[i + currentRow][columnIndx] = cellStyle;

                    if (row.ContainsKey(field.FieldName))
                    {
                        var value = dataType.GetSqlValue(row[field.FieldName], currentContextService.TimeZoneOffset);
                        tbRow[columnIndx] = new ExcelCell
                        {
                            Value = value,
                            Type = dataType.GetExcelType(),
                            //CellStyle = cellStyle
                        };

                    }

                    columnIndx++;
                }






                for (var conditionIndex = 0; conditionIndex < conditionHiddenColumns.Length; conditionIndex++)
                {
                    var col = conditionHiddenColumns[conditionIndex];
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
            }


            if (sumCalc.Count > 0)
            {
                ExcelRow sumRow = table.NewRow();
                foreach (var (index, column) in sumCalc)
                {

                    var dataType = column.DataTypeId.HasValue ? (EnumDataType)column.DataTypeId : EnumDataType.Text;

                    var columnName = (index + 1).GetExcelColumnName();

                    var conditionColum = conditionHiddenColumns.FirstOrDefault(c => c == column.CalcSumConditionCol);

                    var sumRange = $"{columnName}{currentRow + 1}:{columnName}{currentRow + data.Count()}";
                    if (!string.IsNullOrWhiteSpace(column.CalcSumConditionCol) && conditionColum != null)
                    {
                        var aliasIndex = columns.Count + Array.IndexOf(conditionHiddenColumns, conditionColum);
                        var aliasName = (aliasIndex + 1).GetExcelColumnName();

                        var conditionRange = $"{aliasName}{currentRow + 1}:{aliasName}{currentRow + data.Count()}";
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


                }
                var columnIndx = 0;
                foreach (var field in columns)
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



        Dictionary<string, ICellStyle> dataTypeStyles = new Dictionary<string, ICellStyle>();
        private ICellStyle GetCellStyle(ISheet sheet, ExelExportColumn column, bool isHeader = false)
        {
            var keyCached = column.FieldName + "|" + isHeader;
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
    }

    public class ExelExportColumn
    {
        public string GroupName { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public EnumDataType? DataTypeId { get; set; }
        public string CalcSumConditionCol { get; set; }
        public bool IsCalcSum { get; set; }
        public string VAlign { get; set; }
        public string HAlign { get; set; }
        public int? DecimalPlace { get; set; }
    }
}
