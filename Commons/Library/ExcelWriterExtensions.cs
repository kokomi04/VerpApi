using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VErp.Commons.Library
{
    public static class ExcelWriterExtensions
    {
        public static ICell SetHeaderCellStyle(this ISheet sheet, int row, int column)
        {
            return SetCellStyle(sheet, row, column, 12, true, false, VerticalAlignment.Center, HorizontalAlignment.Center, true);
        }

        public static ICell SetSignatureCellStyle(this ISheet sheet, int row, int column)
        {
            return SetCellStyle(sheet, row, column, 12, true, false, VerticalAlignment.Top, HorizontalAlignment.Center, false, true);
        }

        public static ICell SetCellStyle(this ISheet sheet, int row, int column, int fontSize = 11, bool isBold = false, bool isItalic = false, VerticalAlignment? vAlign = null, HorizontalAlignment? hAlign = null, bool isBorder = false, bool isWrap = false, byte[] rgb = null)
        {
            var cell = EnsureCell(sheet, row, column);

            cell.CellStyle = GetCellStyle(sheet, fontSize, isBold, isItalic, vAlign, hAlign, isBorder, isWrap, rgb);

            return cell;
        }

        public static ICell SetCellStyle(this ISheet sheet, int row, int column, ICellStyle style)
        {
            var cell = EnsureCell(sheet, row, column);

            cell.CellStyle = style;

            return cell;
        }

        public static ICell EnsureCell(this ISheet sheet, int row, int column, ICellStyle style = null)
        {
            var excelRow = sheet.GetRow(row);
            if (excelRow == null)
            {
                excelRow = sheet.CreateRow(row);
            }

            var cell = excelRow.GetCell(column);
            if (cell == null)
            {
                cell = excelRow.CreateCell(column);
            }

            if (style != null)
            {
                cell.CellStyle = style;
            }
            return cell;
        }

        public static ICellStyle GetCellStyle(this ISheet sheet, int fontSize = 11, bool isBold = false, bool isItalic = false, VerticalAlignment? vAlign = null, HorizontalAlignment? hAlign = null, bool isBorder = false, bool isWrap = false, byte[] rgb = null, string dataFormat = "", byte[] color = null)
        {
            var style = sheet.Workbook.CreateCellStyle();
            if (vAlign.HasValue)
            {
                style.VerticalAlignment = vAlign.Value;
            }
            if (hAlign.HasValue)
            {
                style.Alignment = hAlign.Value;
            }

            var font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = fontSize;
            font.IsBold = isBold;
            font.IsItalic = isItalic;
            if (color != null)
            {
                font.Color = new XSSFColor(color).Index;
            }

            style.SetFont(font);

            if (isBorder)
            {
                style.BorderTop = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;                
            }

            if (isWrap)
                style.WrapText = true;

            if (rgb != null)
            {
                ((XSSFCellStyle)style).SetFillForegroundColor(new XSSFColor(rgb));
                style.FillPattern = FillPattern.SolidForeground;
            }
            

            if (!string.IsNullOrWhiteSpace(dataFormat))
            {
                ((XSSFCellStyle)style).SetDataFormat(sheet.Workbook.GetCreationHelper().CreateDataFormat().GetFormat(dataFormat));
            }



            return style;
        }

       
        public static VerticalAlignment? GetVerticalAlignment(this string vAlign)
        {
            if (string.IsNullOrWhiteSpace(vAlign)) return null;

            if (vAlign.Contains("top", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Top;
            if (vAlign.Contains("middle", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Center;
            if (vAlign.Contains("bottom", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Bottom;
            if (vAlign.Contains("baseline", StringComparison.OrdinalIgnoreCase)) return VerticalAlignment.Justify;
            return null;
        }

        public static HorizontalAlignment? GetHorizontalAlignment(this string hAlign)
        {
            if (string.IsNullOrWhiteSpace(hAlign)) return null;

            if (hAlign.Contains("left", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Left;
            if (hAlign.Contains("center", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Center;
            if (hAlign.Contains("right", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Right;
            if (hAlign.Contains("justify", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Justify;
            return null;
        }


        public static byte[] HexadecimalToRGB(this string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Remove(0, 1);
            }
            else
            {
                return new byte[] { 200, 200, 200 };
            }

            byte r = (byte)HexadecimalToDecimal(hex.Substring(0, 2));
            byte g = (byte)HexadecimalToDecimal(hex.Substring(2, 2));
            byte b = (byte)HexadecimalToDecimal(hex.Substring(4, 2));
            var cl = Color.FromArgb(r, g, b);
            var a = new XSSFColor(cl);

            return a.ARGB;
        }

        private static int HexadecimalToDecimal(string hex)
        {
            hex = hex.ToUpper();

            int hexLength = hex.Length;
            double dec = 0;

            for (int i = 0; i < hexLength; ++i)
            {
                byte b = (byte)hex[i];

                if (b >= 48 && b <= 57)
                    b -= 48;
                else if (b >= 65 && b <= 70)
                    b -= 55;

                dec += b * Math.Pow(16, ((hexLength - i) - 1));
            }

            return (int)dec;
        }

    }
}
