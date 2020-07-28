using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
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

        public static ICell SetCellStyle(this ISheet sheet, int row, int column, int fontSize = 11, bool isBold = false, bool isItalic = false, VerticalAlignment? vAlign = null, HorizontalAlignment? hAlign = null, bool isBorder = false, bool isWrap = false)
        {
            var cell = EnsureCell(sheet, row, column);

            cell.CellStyle = GetCellStyle(sheet, fontSize, isBold, isItalic, vAlign, hAlign, isBorder, isWrap);

            return cell;
        }

        public static ICell EnsureCell(this ISheet sheet, int row, int column)
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

            return cell;
        }

        public static ICellStyle GetCellStyle(this ISheet sheet, int fontSize = 11, bool isBold = false, bool isItalic = false, VerticalAlignment? vAlign = null, HorizontalAlignment? hAlign = null, bool isBorder = false, bool isWrap = false)
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
            return style;
        }
    }
}
