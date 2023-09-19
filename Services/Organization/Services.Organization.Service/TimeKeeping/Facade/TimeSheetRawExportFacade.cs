using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using static NPOI.HSSF.UserModel.HeaderFooter;
using static VErp.Services.Organization.Service.HrConfig.HrDataService;

namespace VErp.Services.Organization.Service.HrConfig.Facade
{
    public class TimeSheetRawExportFacade
    {
        private ISheet _sheet = null;
        private int _currentRow = 0;
        private List<HrValidateField> _fields = new List<HrValidateField>();
        private IList<int> _columnMaxLineLength = new List<int>();
        private ICurrentContextService _currentContextService;

        public TimeSheetRawExportFacade(List<HrValidateField> fields, ICurrentContextService currentContextService)
        {
            _fields = fields;
            _currentContextService = currentContextService;
        }

        public (Stream stream, string fileName, string contentType) Export(IList<NonCamelCaseDictionary> dataExport)
        {

            var xssfwb = new XSSFWorkbook();
            _sheet = xssfwb.CreateSheet();


            WriteTable(dataExport);

            var currentRowTmp = _currentRow;

            for (var i = 0; i < _fields.Count + 1; i++)
            {
                _sheet.ManualResize(i, _columnMaxLineLength[i]);
            }


            _currentRow = currentRowTmp;


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"{"Dữ liệu chấm công".NormalizeAsInternalName()} {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx".RemoveDiacritics().Replace(" ", "#");
            return (stream, fileName, contentType);
        }

        private void WriteTable(IList<NonCamelCaseDictionary> dataExport)
        {
            _currentRow = 1;

            var fRow = _currentRow;
            var sRow = _currentRow;

            _sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            _sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;

            _columnMaxLineLength = new List<int>(_fields.Count + 1)
            {
                5
            };

            foreach (var f in _fields)
            {
                _sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.Title);
                _columnMaxLineLength.Add(f.Title?.Length + 5 ?? 10);

                _sheet.SetHeaderCellStyle(sRow, sColIndex);
                sColIndex++;
            }

            _currentRow = sRow + 1;

            WriteTableDetailData(dataExport);
        }


        private void WriteTableDetailData(IList<NonCamelCaseDictionary> dataExport)
        {
            var stt = 1;

            var textStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top);
            var intStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");
            var dateStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "dd/MM/yyyy");
            var timeStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "HH:mm");

            foreach (var detail in dataExport)
            {
                _sheet.EnsureCell(_currentRow, 0, intStyle).SetCellValue(stt);
                var sColIndex = 1;

                foreach (var f in _fields)
                {
                    var v = detail[f.FieldName];
                    var dataTypeId = f.DataTypeId;
                    if (!string.IsNullOrWhiteSpace(f.RefTableCode))
                    {
                        dataTypeId = EnumDataType.Text;
                        v = detail[f.FieldNameRefTitle];
                    }

                    switch (dataTypeId)
                    {
                        case EnumDataType.BigInt:
                        case EnumDataType.Int:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, intStyle)
                                    .SetCellValue(Convert.ToInt64(v));
                            }
                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, intStyle);
                            }
                            break;
                        case EnumDataType.Decimal:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, decimalStyle)
                                    .SetCellValue(Convert.ToDouble(v));
                            }
                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, decimalStyle);
                            }
                            break;
                        case EnumDataType.Date:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, dateStyle).SetCellValue(Convert.ToInt64(v).UnixToDateTime(_currentContextService.TimeZoneOffset));
                            }

                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, dateStyle);
                            }
                            break;
                        case EnumDataType.Boolean:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, textStyle).SetCellValue(dataTypeId.GetDataTypeValueTitleByLanguage(v));
                            }

                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, textStyle);
                            }
                            break;
                        case EnumDataType.Enum:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, textStyle).SetCellValue(EnumExtensions.GetEnumDescription(v as Enum));
                            }

                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, textStyle);
                            }
                            break;
                        case EnumDataType.Time:
                            if (!v.IsNullOrEmptyObject())
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, timeStyle).SetCellValue((new DateTime(1970, 1, 1).AddSeconds((double)v)).ToString("HH:mm"));
                            }

                            else
                            {
                                _sheet.EnsureCell(_currentRow, sColIndex, timeStyle);
                            }
                            break;


                        default:
                            _sheet.EnsureCell(_currentRow, sColIndex, textStyle).SetCellValue(v?.ToString());
                            break;
                    }
                    if (v?.ToString()?.Length + 5 > _columnMaxLineLength[sColIndex])
                    {
                        _columnMaxLineLength[sColIndex] = v?.ToString()?.Length + 5 ?? 10;
                    }

                    sColIndex++;
                }
                _currentRow++;
                stt++;
            }

        }

        private void MergeColumns(int row, int fromColumn, int toColumn)
        {
            MergeCells(row, row, fromColumn, toColumn);
        }

        private void MergeRows(int fromRow, int toRow, int column)
        {
            MergeCells(fromRow, toRow, column, column);
        }

        private void MergeCells(int fromRow, int toRow, int fromColumn, int toColumn)
        {
            var region = new CellRangeAddress(fromRow, toRow, fromColumn, toColumn);
            _sheet.AddMergedRegion(region);
            RegionUtil.SetBorderBottom(1, region, _sheet);
            RegionUtil.SetBorderLeft(1, region, _sheet);
            RegionUtil.SetBorderRight(1, region, _sheet);
            RegionUtil.SetBorderTop(1, region, _sheet);
        }

    }

}