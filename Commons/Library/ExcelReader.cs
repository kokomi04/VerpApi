using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;

namespace VErp.Commons.Library
{
    public class ExcelReader
    {
        private IWorkbook hssfwb;
        private DataFormatter dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);

        public ExcelReader(string filePath) : this(new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {

        }

        public ExcelReader(Stream file)
        {
            hssfwb = WorkbookFactory.Create(file);// new XSSFWorkbook(file);
            file.Close();
        }

        private string GetCellValue(ICell cell)
        {
            var dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);

            // If this is not part of a merge cell,
            // just get this cell's value like normal.
            if (!cell.IsMergedCell)
            {
                return dataFormatter.FormatCellValue(cell);
            }

            // Otherwise, we need to find the value of this merged cell.
            else
            {
                // Get current sheet.
                var currentSheet = cell.Sheet;

                // Loop through all merge regions in this sheet.
                for (int i = 0; i < currentSheet.NumMergedRegions; i++)
                {
                    var mergeRegion = currentSheet.GetMergedRegion(i);

                    // If this merged region contains this cell.
                    if (mergeRegion.FirstRow <= cell.RowIndex && cell.RowIndex <= mergeRegion.LastRow &&
                        mergeRegion.FirstColumn <= cell.ColumnIndex && cell.ColumnIndex <= mergeRegion.LastColumn)
                    {
                        // Find the top-most and left-most cell in this region.
                        var firstRegionCell = currentSheet.GetRow(mergeRegion.FirstRow)
                                                .GetCell(mergeRegion.FirstColumn);

                        // And return its value.
                        return dataFormatter.FormatCellValue(firstRegionCell);
                    }
                }
                // This should never happen.
                throw new Exception("Cannot find this cell in any merged region");
            }
        }

        public string[][] ReadFile(int collumnLength, int sheetAt = 0, int startRow = 0, int startCollumn = 0)
        {
            List<string[]> data = new List<string[]>();
            ISheet sheet = hssfwb.GetSheetAt(sheetAt);
            int rowIdx = startRow;
            IRow row;
            while ((row = sheet.GetRow(rowIdx)) != null)
            {
                if (row.GetCell(0) == null || string.IsNullOrEmpty(GetCellValue(row.GetCell(0))))
                {
                    break;
                }
                List<string> info = new List<string>();
                for (int collumnIdx = 0; collumnIdx < collumnLength; collumnIdx++)
                {
                    ICell cell = row.GetCell(collumnIdx + startCollumn);
                    if (cell != null)
                    {
                        info.Add(GetCellValue(cell));
                    }
                    else
                    {
                        info.Add(string.Empty);
                    }
                }
                data.Add(info.ToArray());
                rowIdx++;
            }
            return data.ToArray();
        }


        public IList<ExcelSheetDataModel> ReadSheets(string sheetName, int fromRow = 1, int? toRow = null, int? maxrows = null)
        {
            var sheetDatas = new List<ExcelSheetDataModel>();

            //hssfwb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
            try
            {
                BaseFormulaEvaluator.EvaluateAllFormulaCells(hssfwb);
            }
            catch (Exception e)
            {

            }


            //if (hssfwb is XSSFWorkbook)
            //{
            //    NPOI.SS.Formula.BaseFormulaEvaluator.EvaluateAllFormulaCells(hssfwb);
            //}
            //else
            //{
            //    HSSFFormulaEvaluator.EvaluateAllFormulaCells(hssfwb);
            //}

            var fromRowIndex = fromRow - 1;
            var toRowIndex = toRow.HasValue && toRow > 0 ? toRow - 1 : null;

            for (int i = 0; i < hssfwb.NumberOfSheets; i++)
            {

                var sheet = hssfwb.GetSheetAt(i);

                if (!string.IsNullOrWhiteSpace(sheetName) && sheet.SheetName != sheetName)
                    continue;

                if (!maxrows.HasValue)
                {
                    maxrows = sheet.LastRowNum + 1;
                }
                else
                {
                    if (maxrows > sheet.LastRowNum)
                    {
                        maxrows = sheet.LastRowNum + 1;
                    }
                }

                var sheetData = new List<NonCamelCaseDictionary>();

                var columns = new HashSet<string>();

                var mergeRegions = new CellRangeAddress[sheet.NumMergedRegions];

                var regionValues = new ICell[sheet.NumMergedRegions];

                for (var re = 0; re < sheet.NumMergedRegions; re++)
                {
                    var region = sheet.GetMergedRegion(re);

                    mergeRegions[re] = region;

                    var isFirstRowValue = false;

                    ICell cell = null;

                    var r = sheet.GetRow(region.FirstRow);

                    var c = r.Cells.FirstOrDefault(c => c.ColumnIndex == region.FirstColumn);

                    var v = GetCellString(c);
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        cell = c;
                        isFirstRowValue = true;
                    }


                    if (!isFirstRowValue)
                    {
                        r = sheet.GetRow(region.LastRow);

                        c = r.Cells.FirstOrDefault(c => c.ColumnIndex == region.LastColumn);

                        v = GetCellString(c);
                        if (!string.IsNullOrWhiteSpace(v))
                        {
                            cell = c;
                        }
                    }

                    regionValues[re] = cell;
                }

                var continuousEmpty = 0;
                for (int row = fromRowIndex; row < maxrows && (!toRowIndex.HasValue || row <= toRowIndex); row++)
                {
                    var rowData = new NonCamelCaseDictionary();
                    if (sheet.GetRow(row) == null) //null is when the row only contains empty cells 
                    {
                        continuousEmpty++;
                        //continue;
                    }
                    else
                    {
                        continuousEmpty = 0;

                        foreach (var col in sheet.GetRow(row).Cells)
                        {
                            var columnName = GetExcelColumnName(col.ColumnIndex + 1);
                            if (!columns.Contains(columnName))
                            {
                                columns.Add(columnName);
                            }

                            var cell = col;

                            if (cell.IsMergedCell)
                            {
                                for (var regionIdx = 0; regionIdx < mergeRegions.Length; regionIdx++)
                                {
                                    var region = mergeRegions[regionIdx];
                                    if (region.IsInRange(row, col.ColumnIndex))
                                    {
                                        var c = regionValues[regionIdx];
                                        var v = GetCellString(c);
                                        if (!string.IsNullOrWhiteSpace(v))
                                        {
                                            cell = c;
                                        }
                                    }
                                }
                            }


                            try
                            {
                                rowData.Add(columnName, GetCellString(cell));
                            }
                            catch
                            {
                                rowData.Add(columnName, cell.StringCellValue.ToString());

                            }

                        }
                    }

                    sheetData.Add(rowData);
                }

                //set default value for null column
                foreach (var column in columns)
                {
                    foreach (var row in sheetData)
                    {
                        if (!row.ContainsKey(column))
                        {
                            row.Add(column, null);
                        }
                    }
                }

                sheetDatas.Add(new ExcelSheetDataModel() { SheetName = sheet.SheetName, Rows = sheetData.ToArray() });
            }

            return sheetDatas;
        }

        private string GetCellString(ICell cell)
        {
            if (cell == null) return null;

            var type = cell.CellType;

            string formulaMessage = "";
            if (cell.CellType == CellType.Formula)
            {
                try
                {
                    //hssfwb.GetCreationHelper().CreateFormulaEvaluator().EvaluateFormulaCell(cell);
                    type = cell.CachedFormulaResultType;
                }
                catch (Exception ex)
                {
                    formulaMessage = cell.CellFormula + " => " + ex.Message;
                }


            }

            switch (type)
            {
                case CellType.String:
                    return cell.StringCellValue?.Trim();
                case CellType.Formula:
                    return formulaMessage;

                case CellType.Numeric:
                    return DateUtil.IsCellDateFormatted(cell) ? cell.DateCellValue.ToString() : cell.NumericCellValue.ToString();
            }

            return dataFormatter.FormatCellValue(cell);
            // return cell.StringCellValue?.Trim();
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
