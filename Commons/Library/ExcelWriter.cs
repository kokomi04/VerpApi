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
using VErp.Commons.Enums.MasterEnum;
using System.Collections;

namespace VErp.Commons.Library
{
    public class ExcelWriter
    {
        private XSSFWorkbook hssfwb;

        public ExcelWriter()
        {
            hssfwb = new XSSFWorkbook();
        }

        public ISheet GetSheet(string sheetName)
        {
            var sheet = hssfwb.GetSheet(sheetName);
            if (sheet == null)
            {
                sheet = hssfwb.CreateSheet(sheetName);
            }
            return sheet;
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

        public void WriteToSheet(ExcelData table, string sheetName, out int endRow, bool isHeader = false, byte[] headerRgb = null, int startCollumn = 0, int startRow = 0)
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
                        var cellStyle = (XSSFCellStyle)hssfwb.CreateCellStyle();
                        cellStyle.SetFillForegroundColor(new XSSFColor(headerRgb));
                        cellStyle.FillPattern = FillPattern.SolidForeground;
                        cell.CellStyle = cellStyle;
                    }
                    addCollumn++;
                }
                addedRow++;
            }
            int columnLength = table.Columns.Count;

            var dateStyle = (XSSFCellStyle)hssfwb.CreateCellStyle();
            var createHelper = hssfwb.GetCreationHelper();
            dateStyle.SetDataFormat(createHelper.CreateDataFormat().GetFormat("dd/mm/yyyy"));

            foreach (ExcelRow row in table.Rows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                for (int indx = 0; indx < columnLength; indx++)
                {
                    int curCollumn = indx + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);
                    if (row[indx] == null || (row[indx] as ExcelCell).Value == DBNull.Value) continue;
                    switch ((row[indx] as ExcelCell).Type)
                    {
                        case EnumExcelType.String:
                            cell.SetCellValue((row[indx] as ExcelCell).Value.ToString());
                            break;
                        case EnumExcelType.Boolean:
                            cell.SetCellValue((bool)(row[indx] as ExcelCell).Value);
                            break;
                        case EnumExcelType.DateTime:
                            cell.SetCellValue((DateTime)(row[indx] as ExcelCell).Value);
                            cell.CellStyle = dateStyle;
                            break;
                        case EnumExcelType.Number:
                            cell.SetCellValue(Convert.ToDouble((row[indx] as ExcelCell).Value));
                            break;
                        case EnumExcelType.Formula:
                            cell.SetCellFormula((row[indx] as ExcelCell).Value.ToString());
                            break;
                        default:
                            break;
                    }

                }
                addedRow++;
            }
            endRow = startRow + addedRow - 1;
        }
        public void WriteToSheet(ExcelData table, string sheetName, out int endRow, int startCollumn = 0, int startRow = 0)
        {
            var sheet = hssfwb.GetSheet(sheetName);
            if (sheet == null)
            {
                sheet = hssfwb.CreateSheet(sheetName);
            }

            int addedRow = 0;
            int columnLength = table.Columns.Count;

            foreach (ExcelRow row in table.Rows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                for (int indx = 0; indx < columnLength; indx++)
                {
                    int curCollumn = indx + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);
                    if (row[indx] == null || (row[indx] as ExcelCell).Value == DBNull.Value)
                    {
                        (row[indx] as ExcelCell).Value = string.Empty;
                        (row[indx] as ExcelCell).Type = EnumExcelType.String;
                    }
                    switch ((row[indx] as ExcelCell).Type)
                    {
                        case EnumExcelType.String:
                            cell.SetCellValue((row[indx] as ExcelCell).Value.ToString());
                            cell.SetCellType(CellType.String);
                            cell.CellStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true, isBorder: true);
                            break;
                        case EnumExcelType.Boolean:
                            cell.SetCellValue((bool)(row[indx] as ExcelCell).Value);
                            cell.SetCellType(CellType.Boolean);
                            cell.CellStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true, isBorder: true);
                            break;
                        case EnumExcelType.DateTime:
                            cell.SetCellValue((DateTime)(row[indx] as ExcelCell).Value);
                            cell.CellStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: "dd/mm/yyyy");
                            break;
                        case EnumExcelType.Number:
                            cell.SetCellValue(Convert.ToDouble((row[indx] as ExcelCell).Value));
                            cell.SetCellType(CellType.Numeric);
                            cell.CellStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: "#,##0.00");
                            break;
                        case EnumExcelType.Formula:
                            cell.SetCellFormula((row[indx] as ExcelCell).Value.ToString());
                            cell.SetCellType(CellType.Formula);
                            cell.CellStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, isWrap: true, isBorder: true, dataFormat: "#,##0.00", isBold: true);
                            break;
                        default:
                            break;
                    }

                }
                addedRow++;
            }
            endRow = startRow + addedRow - 1;
        }

        public ICreationHelper GetCreationHelper()
        {
            return hssfwb.GetCreationHelper();
        }

        public int AddPicture(byte[] bytes, PictureType pictureType)
        {
            return hssfwb.AddPicture(bytes, pictureType);
        }

        public async Task<MemoryStream> WriteToStream()
        {
            string fileName = @"TempFile";
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                hssfwb.Write(file);
                file.Close();
            }
            var memory = new MemoryStream();
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
                stream.Close();
            }
            File.Delete(fileName);
            memory.Position = 0;
            return memory;
        }
    }

    public class ExcelData
    {
        public HashSet<string> Columns { get; }
        public List<ExcelRow> Rows { get; }

        public ExcelData()
        {
            Columns = new HashSet<string>();
            Rows = new List<ExcelRow>();
        }

        public void AddColumn(string columnName = null)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                int indx = 0;
                do
                {
                    indx++;
                    columnName = $"column{indx}";
                }
                while (Columns.Contains(columnName));
            }
            Columns.Add(columnName);
            foreach (var row in Rows)
            {
                row.Add();
            }
        }

        public ExcelRow NewRow()
        {
            ExcelRow row = new ExcelRow();
            for (int indx = 0; indx < Columns.Count; indx++)
            {
                row.Add();
            }
            return row;
        }
    }

    public class ExcelRow
    {
        private List<ExcelCell> row;

        public int Count => row.Count;

        public ExcelCell this[int index] { get => row[index]; set => row[index] = value; }

        public ExcelRow()
        {
            row = new List<ExcelCell>();
        }

        public void Add(ExcelCell value = null)
        {
            row.Add(value);
        }

        public void FillAllRow()
        {
            for (int r = 0; r < Count; r++)
            {
                if (this[r] == null)
                    this[r] = new ExcelCell { Value = string.Empty, Type = EnumExcelType.String };
            }
        }
    }

    public class ExcelCell
    {
        public object Value { get; set; }
        public EnumExcelType Type { get; set; }
    }
}
