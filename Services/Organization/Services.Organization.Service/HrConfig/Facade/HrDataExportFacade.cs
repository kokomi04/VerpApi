using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using static VErp.Services.Organization.Service.HrConfig.HrDataService;

namespace VErp.Services.Organization.Service.HrConfig.Facade
{
    public class HrDataExportFacade
    {
        private HrType _hrType;
        private ISheet _sheet = null;
        private int _currentRow = 0;
        private Dictionary<int, List<HrValidateField>> _fieldsByArea = new Dictionary<int, List<HrValidateField>>();
        private IList<int> _columnMaxLineLength = new List<int>();

        public HrDataExportFacade(HrType hrType, List<HrValidateField> fields, IList<string> fieldNames)
        {
            _hrType = hrType;
            _fieldsByArea = fields.Where(f => fieldNames.Contains(f.FieldName)).GroupBy(f => f.HrAreaId).ToDictionary(g => g.Key, g => g.ToList());
        }

        public (Stream stream, string fileName, string contentType) Export(IList<NonCamelCaseDictionary> hrDetails)
        {

            var xssfwb = new XSSFWorkbook();
            _sheet = xssfwb.CreateSheet();


            WriteTable(hrDetails);

            var currentRowTmp = _currentRow;

            if (_sheet.LastRowNum < 100)
            {
                for (var i = 0; i < _fieldsByArea.Sum(a => a.Value.Count) + 1; i++)
                {
                    _sheet.AutoSizeColumn(i, false);
                }
            }
            else
            {
                for (var i = 0; i < _fieldsByArea.Sum(a => a.Value.Count) + 1; i++)
                {
                    _sheet.ManualResize(i, _columnMaxLineLength[i]);
                }
            }

            _currentRow = currentRowTmp;


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"{_hrType.Title.NormalizeAsInternalName()} {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx".RemoveDiacritics().Replace(" ", "#");
            return (stream, fileName, contentType);
        }

        private void WriteTable(IList<NonCamelCaseDictionary> hrDetails)
        {
            _currentRow = 1;

            var fRow = _currentRow;
            var sRow = _currentRow;

            _sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            _sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;
            if (_fieldsByArea.Count > 0)
            {
                sRow = fRow + 1;
            }

            _columnMaxLineLength = new List<int>(_fieldsByArea.Count + 1)
            {
                5
            };

            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                _sheet.EnsureCell(fRow, sColIndex).SetCellValue(areaFields.First().HrAreaTitle);
                _sheet.SetHeaderCellStyle(fRow, sColIndex);

                if (areaFields.Count() > 1)
                {
                    MergeColumns(fRow, sColIndex, sColIndex + areaFields.Count() - 1);
                }

                foreach (var f in areaFields)
                {
                    _sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.Title);
                    _columnMaxLineLength.Add(f.Title?.Length ?? 10);

                    _sheet.SetHeaderCellStyle(sRow, sColIndex);
                    sColIndex++;
                }
            }

            _currentRow = sRow + 1;

            WriteTableDetailData(hrDetails);
        }


        private void WriteTableDetailData(IList<NonCamelCaseDictionary> hrDetails)
        {
            var stt = 1;

            var textStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top);
            var intStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = _sheet.GetCellStyle(isBorder: true, vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");

            var groupByBills = hrDetails.GroupBy(d => d[OrganizationConstants.HR_TABLE_F_IDENTITY]).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (billId, details) in groupByBills)
            {
                _sheet.EnsureCell(_currentRow, 0, intStyle).SetCellValue(stt);
                var mergeRowsEnd = _currentRow + details.Count - 1;
                if (details.Count > 1)
                {
                    MergeRows(_currentRow, mergeRowsEnd, 0);
                }

                for (var i = 0; i < details.Count; i++)
                {
                    var detail = details[i];
                    var sColIndex = 1;
                    foreach (var (areaId, areaFields) in _fieldsByArea)
                    {
                        if (!areaFields.First().IsMultiRow && i > 0)
                        {
                            sColIndex += areaFields.Count;
                            continue;
                        }

                        foreach (var f in areaFields)
                        {
                            var v = detail[f.FieldName];
                            var dataTypeId = f.DataTypeId;
                            if (!string.IsNullOrWhiteSpace(f.RefTableCode))
                            {
                                dataTypeId = EnumDataType.Text;
                                v = detail[f.RefTitle];
                            }

                            if (!f.IsMultiRow && details.Count > 1)
                            {
                                MergeRows(_currentRow, mergeRowsEnd, sColIndex);
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
                                default:
                                    _sheet.EnsureCell(_currentRow, sColIndex, textStyle).SetCellValue(v?.ToString());
                                    break;
                            }
                            if (v?.ToString()?.Length > _columnMaxLineLength[sColIndex])
                            {
                                _columnMaxLineLength[sColIndex] = v?.ToString()?.Length ?? 10;
                            }

                            sColIndex++;
                        }
                    }
                    _currentRow++;
                }
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