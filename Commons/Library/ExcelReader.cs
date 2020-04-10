using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace VErp.Commons.Library
{
    public class ExcelReader
    {
        private XSSFWorkbook hssfwb;

        public ExcelReader(string filePath) : this(new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
        }

        public ExcelReader(Stream file)
        {
            hssfwb = new XSSFWorkbook(file);
            file.Close();
        }

        ~ExcelReader()
        {
            if (hssfwb != null)
            {
                hssfwb.Dispose();

            }
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

        public string[][] ReadFile(int sheetAt = 0, int startRow = 0)
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
                int collumIdx = 0;
                ICell cell;
                while ((cell = row.GetCell(collumIdx)) != null)
                {
                    if (cell != null)
                    {
                        info.Add(GetCellValue(cell));
                    }
                    else
                    {
                        info.Add(string.Empty);
                    }
                    collumIdx++;
                }
                data.Add(info.ToArray());
                rowIdx++;
            }
            return data.ToArray();
        }
    }
}
