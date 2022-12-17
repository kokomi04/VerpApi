using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;

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
        public IWorkbook GetWorkbook()
        {
            return hssfwb;
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
                    if (row[indx] == null || (row[indx]).Value == DBNull.Value) continue;
                    switch (row[indx].Type)
                    {
                        case EnumExcelType.String:
                            cell.SetCellValue(row[indx].Value.ToString());
                            break;
                        case EnumExcelType.Boolean:
                            cell.SetCellValue((bool)(row[indx]).Value);
                            break;
                        case EnumExcelType.DateTime:
                            cell.SetCellValue((DateTime)row[indx].Value);
                            cell.CellStyle = dateStyle;
                            break;
                        case EnumExcelType.Percentage:
                        case EnumExcelType.Number:
                            cell.SetCellValue(Convert.ToDouble(row[indx].Value));
                            break;
                        case EnumExcelType.Formula:
                            cell.SetCellFormula(row[indx].Value.ToString());
                            break;
                        default:
                            break;
                    }

                }
                addedRow++;
            }
            endRow = startRow + addedRow - 1;
        }
        public void WriteToSheet(ISheet sheet, ExcelData table, out int endRow, int startCollumn = 0, int startRow = 0)
        {
            int addedRow = 0;
            int columnLength = table.Columns.Count;
            var nullStyle = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true, isBorder: true);
            foreach (ExcelRow row in table.Rows)
            {
                int curRow = startRow + addedRow;
                IRow newRow = sheet.CreateRow(curRow);
                for (int indx = 0; indx < columnLength; indx++)
                {
                    int curCollumn = indx + startCollumn;
                    ICell cell = newRow.CreateCell(curCollumn);

                    WriteCellValue(cell, row[indx]);

                    /*
                    if (row[indx].Value.IsNullObject())
                    {
                        row[indx].Value = string.Empty;
                        row[indx].Type = EnumExcelType.String;
                        //row[indx].CellStyle = nullStyle;
                    }

                    switch (row[indx].Type)
                    {
                        case EnumExcelType.String:
                            cell.SetCellValue(row[indx].Value.ToString());
                            cell.SetCellType(CellType.String);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        case EnumExcelType.Boolean:
                            cell.SetCellValue((bool)row[indx].Value);
                            cell.SetCellType(CellType.Boolean);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        case EnumExcelType.DateTime:
                            cell.SetCellValue((DateTime)row[indx].Value);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        case EnumExcelType.Number:
                            cell.SetCellValue(Convert.ToDouble(row[indx].Value));
                            cell.SetCellType(CellType.Numeric);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        case EnumExcelType.Percentage:
                            cell.SetCellValue(Convert.ToDouble(row[indx].Value)/100);
                            cell.SetCellType(CellType.Numeric);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        case EnumExcelType.Formula:
                            cell.SetCellFormula(row[indx].Value.ToString());
                            cell.SetCellType(CellType.Formula);
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                        default:
                            cell.CellStyle = row[indx].CellStyle;
                            break;
                    }
                    */

                }
                addedRow++;
            }
            endRow = startRow + addedRow - 1;
        }

        public void WriteCellValue(ICell cell, ExcelCell data)
        {
            if (data.Value.IsNullOrEmptyObject())
            {
                data.Value = string.Empty;
                data.Type = EnumExcelType.String;
                //row[indx].CellStyle = nullStyle;
            }

            switch (data.Type)
            {
                case EnumExcelType.String:
                    cell.SetCellValue(data.Value.ToString());
                    cell.SetCellType(CellType.String);
                    cell.CellStyle = data.CellStyle;
                    break;
                case EnumExcelType.Boolean:
                    cell.SetCellValue((bool)data.Value);
                    cell.SetCellType(CellType.Boolean);
                    cell.CellStyle = data.CellStyle;
                    break;
                case EnumExcelType.DateTime:
                    if (data.Value.GetType().IsNumber())
                    {
                        var l = Convert.ToInt64(data.Value);

                        cell.SetCellValue(l.UnixToDateTime().Value);
                    }
                    else
                    {
                        cell.SetCellValue((DateTime)data.Value);
                    }
                    cell.CellStyle = data.CellStyle;
                    break;
                case EnumExcelType.Number:
                    cell.SetCellValue(Convert.ToDouble(data.Value));
                    cell.SetCellType(CellType.Numeric);
                    cell.CellStyle = data.CellStyle;
                    break;
                case EnumExcelType.Percentage:
                    cell.SetCellValue(Convert.ToDouble(data.Value) / 100);
                    cell.SetCellType(CellType.Numeric);
                    cell.CellStyle = data.CellStyle;
                    break;
                case EnumExcelType.Formula:
                    cell.SetCellFormula(data.Value.ToString());
                    cell.SetCellType(CellType.Formula);
                    cell.CellStyle = data.CellStyle;
                    break;
                default:
                    cell.CellStyle = data.CellStyle;
                    break;
            }
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


        public VerticalAlignment? GetVAlign(string vAlign)
        {
            if (string.IsNullOrWhiteSpace(vAlign)) return null;

            if (vAlign.Contains("top", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Top;
            if (vAlign.Contains("middle", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Center;
            if (vAlign.Contains("bottom", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Bottom;
            if (vAlign.Contains("baseline", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Justify;
            return null;
        }

        public HorizontalAlignment? GetHAlign(string hAlign)
        {
            if (string.IsNullOrWhiteSpace(hAlign)) return null;

            if (hAlign.Contains("left", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Left;
            if (hAlign.Contains("center", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Center;
            if (hAlign.Contains("right", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Right;
            if (hAlign.Contains("justify", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Justify;
            return null;
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
        public ICellStyle CellStyle { get; set; }
    }
}
