using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using VErp.Infrastructure.AppSettings.Model;
using System.Data;

namespace VErp.Commons.Library
{
    public class ExcelWriter
    {
        private XSSFWorkbook hssfwb;

        public ExcelWriter()
        {
            hssfwb = new XSSFWorkbook();
        }

        public void WriteToSheet((string, byte[])[][] dataInRows, string sheetName, int startCollumn = 0, int startRow = 0)
        {
            var sheet = hssfwb.GetSheet(sheetName);
            if (sheet == null)
            {
                sheet = hssfwb.CreateSheet(sheetName);
            }

            int addedRow = 0;
            foreach ((string, byte[])[] row in dataInRows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                int addCollumn = 0;
                foreach ((string Value, byte[] Style) in row)
                {
                    int curCollumn = addCollumn + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);
                    cell.SetCellValue(Value);
                    if (Style != null)
                    {
                        XSSFCellStyle cellStyle = (XSSFCellStyle)hssfwb.CreateCellStyle();
                        cellStyle.SetFillForegroundColor(new XSSFColor(Style));
                        cellStyle.FillPattern = FillPattern.SolidForeground;
                        cell.CellStyle = cellStyle;
                    }


                    addCollumn++;
                }
                addedRow++;
            }
        }

        public void WriteToSheet(DataTable table, string sheetName, out int endRow, bool isHeader = false, byte[] headerRgb = null, int startCollumn = 0, int startRow = 0)
        {
            var sheet = hssfwb.GetSheet(sheetName);
            if (sheet == null)
            {
                sheet = hssfwb.CreateSheet(sheetName);
            }

            int addedRow = 0;
            if (isHeader)
            {
                // Write header
                IRow newRow = sheet.CreateRow(startRow);
                int addCollumn = 0;
                foreach (var collumn in table.Columns)
                {
                    int curCollumn = addCollumn + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);
                    cell.SetCellValue(collumn.ToString());
                    if (headerRgb != null)
                    {
                        XSSFCellStyle cellStyle = (XSSFCellStyle)hssfwb.CreateCellStyle();
                        cellStyle.SetFillForegroundColor(new XSSFColor(headerRgb));
                        cellStyle.FillPattern = FillPattern.SolidForeground;
                        cell.CellStyle = cellStyle;
                    }
                    addCollumn++;
                }
                addedRow++;
            }
            int columnLength = table.Columns.Count;

            foreach (DataRow row in table.Rows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                for (int indx = 0; indx < columnLength; indx++)
                {
                    int curCollumn = indx + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);
                    cell.SetCellValue(row[indx]?.ToString() ?? null);
                }
                addedRow++;
            }
            endRow = startRow + addedRow - 1;
        }

        public async Task<MemoryStream> WriteToStream()
        {
            string tempFilePath = @"TempFile";

            using (FileStream file = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                hssfwb.Write(file);
                file.Close();
            }
            var memory = new MemoryStream();
            using (var stream = new FileStream(tempFilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
                stream.Close();
            }
            File.Delete(tempFilePath);
            memory.Position = 0;
            return memory;
        }
    }
}
