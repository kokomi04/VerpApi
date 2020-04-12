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

namespace VErp.Commons.Library
{
    public class ExcelWriter
    {
        private XSSFWorkbook hssfwb;

        public ExcelWriter()
        {
            hssfwb = new XSSFWorkbook();
        }

        public void WriteToSheet(List<(string, byte[])[]> dataInRows, string sheetName, int startCollumn = 0, int startRow = 0)
        {
            ISheet sheet = hssfwb.CreateSheet(sheetName);

            int addedRow = 0;
            foreach ((string, byte[])[] row in dataInRows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                int addCollumn = 0;
                foreach ((string Value, byte[] Style) in row)
                {
                    if (Style != null)
                    {
                        XSSFCellStyle cellStyle = (XSSFCellStyle)hssfwb.CreateCellStyle();
                        cellStyle.SetFillForegroundColor(new XSSFColor(Style));
                        cellStyle.FillPattern = FillPattern.SolidForeground;
                        int curCollumn = addCollumn + startCollumn;
                        ICell cell = newRow.CreateCell(curCollumn);
                        cell.SetCellValue(Value);

                        cell.CellStyle = cellStyle;
                    }
                    addCollumn++;
                }
                addedRow++;
            }
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
