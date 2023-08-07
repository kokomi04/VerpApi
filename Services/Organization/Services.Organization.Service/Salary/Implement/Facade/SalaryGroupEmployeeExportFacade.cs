using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NPOI.HSSF.UserModel.HeaderFooter;
using VErp.Commons.Library;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using VErp.Services.Organization.Model.Salary;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Services.Organization.Model.Employee;
using OpenXmlPowerTools;
using VErp.Infrastructure.EF.OrganizationDB;
using System.Linq.Expressions;
using System.Dynamic;
using NPOI.SS.Formula.Functions;
using DocumentFormat.OpenXml.Drawing;
using Google.Protobuf.WellKnownTypes;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{
    public class SalaryGroupEmployeeExportFacade
    {
        private readonly ISalaryFieldService _salaryFieldService;
        private readonly ISalaryEmployeeService _salaryEmployeeService;
        private readonly ISalaryGroupService _salaryGroupService;
        private readonly IList<string> _fieldsName;

        private ISheet sheet = null;
        private int currentRow = 0;
        private IList<SalaryFieldModel> _salaryFields;
        private IList<int> columnMaxLineLength = new List<int>();
        private const string EMPLOYEE_F_ID = "F_Id";
        private const string EMPLOYEE_FIELD_NAME = "ho_ten";
        private IList<string> groups;

        public SalaryGroupEmployeeExportFacade(IList<string> fieldsName, ISalaryFieldService salaryFieldService, ISalaryEmployeeService salaryEmployeeService, ISalaryGroupService salaryGroupService)
        {
            _fieldsName = fieldsName;
            _salaryFieldService = salaryFieldService;
            _salaryEmployeeService = salaryEmployeeService;
            _salaryGroupService = salaryGroupService;
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields, string titleName, int salaryGroupId)
        {

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();
            await GetSalaryField(salaryGroupId);

            var employees = WriteTable(groupSalaryEmployees, groupFields);
            if (sheet.LastRowNum < 100)
            {
                for (var i = 0; i < _salaryFields.Count + 1; i++)
                {
                    sheet.AutoSizeColumn(i, true);
                }
            }
            else
            {
                for (var i = 0; i < _salaryFields.Count + 1; i++)
                {
                    sheet.ManualResize(i, columnMaxLineLength[i]);
                }
            }
            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = StringUtils.RemoveDiacritics($"{titleName} {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx").Replace(" ", "#");

            return (stream, fileName, contentType);
        }

        private async Task GetSalaryField(int salaryGroupId)
        {
            var fieldsGroup = salaryGroupId !=0 ? await _salaryGroupService.GetInfo(salaryGroupId) : null;
            var fieldsData = await _salaryFieldService.GetList();
            foreach (var field in fieldsData)
            {
                if (string.IsNullOrEmpty(field.GroupName))
                {
                    field.GroupName = string.Empty;
                }
            }
            _salaryFields = new List<SalaryFieldModel>();
            foreach (var field in _fieldsName)
            {
                var salaryField = fieldsData.FirstOrDefault(x => x.SalaryFieldName == field);
                if (salaryField == null)
                    throw new BadRequestException($"Không tìm thấy trường {field} trong bảng lương");
                else
                {
                    if (fieldsGroup != null)
                    {
                        var fieldGroup = fieldsGroup.TableFields.FirstOrDefault(x => x.SalaryFieldId == salaryField.SalaryFieldId);
                        salaryField.GroupName = string.IsNullOrEmpty( fieldGroup.GroupName ) ? "" : fieldGroup.GroupName;
                        salaryField.SortOrder = fieldGroup.SortOrder;
                    }
                    _salaryFields.Add(salaryField);
                }
                    
            }
        }
        private string WriteTable(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields)
        {
            if (groupSalaryEmployees.Count == 0)
                throw new BadRequestException("Không tìm thấy nhân sự trong bảng lương");
            currentRow = 1;
            groups = _salaryFields.Select(g => g.GroupName).Distinct().ToList();
            var fRow = currentRow;
            var sRow = currentRow;
            _salaryFields = _salaryFields.OrderBy(x=> x.SortOrder).ToList();


            var sColIndex = 1;
            if (groups.Count > 0)
            {
                sRow = fRow + 1;
            }

            sheet.EnsureCell(sRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);
            columnMaxLineLength = new List<int>(_salaryFields.Count + 1);
            columnMaxLineLength.Add(5);
            foreach (var g in groups)
            {
                var groupCols = _salaryFields.Where(f => f.GroupName == g);

                sheet.EnsureCell(fRow, sColIndex).SetCellValue(g);
                sheet.SetHeaderCellStyle(fRow, sColIndex);

                if (groupCols.Count() > 1)
                            {
                    var region = new CellRangeAddress(fRow, fRow, sColIndex, sColIndex + groupCols.Count() - 1);
                                sheet.AddMergedRegion(region);
                                RegionUtil.SetBorderBottom(1, region, sheet);
                                RegionUtil.SetBorderLeft(1, region, sheet);
                                RegionUtil.SetBorderRight(1, region, sheet);
                                RegionUtil.SetBorderTop(1, region, sheet);
                            }
                       

                foreach (var f in groupCols)
                    {

                    sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.Title);
                    sheet.SetHeaderCellStyle(sRow, sColIndex);

                    columnMaxLineLength.Add(f.Title?.Length ?? 10);
                    sColIndex++;
                }
            }
            currentRow = sRow + 1;

            return WriteTableDetailData(groupSalaryEmployees, groupFields);
        }
        private string WriteTableDetailData(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields)
        {
           
            var textStyle = sheet.GetCellStyle(isBorder: true);
            var intStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");
            int column = 0;
            var sumColumns = new Dictionary<int, SalaryFieldModel>();
            if (groupFields != null && groupFields.Count > 0)
            {
                if (!groupSalaryEmployees.Any(x => groupFields.Any(g => x.ContainsKey(g))))
                {
                    throw new BadRequestException($"Không tìm thấy trường dữ liệu {groupFields} để thực hiện gom nhóm");
                }

                var groups = from f in groupSalaryEmployees
                             group f by IListToString( groupFields.Select(x => f[x].Value?.ToString() ?? "").ToList())
                             into g
                             select g;
                foreach (var g in groups)
                {
                    var stt = 1;
                    var group = g.OrderBy(x => x[EMPLOYEE_FIELD_NAME].Value.ToString().Split(' ').Last());
                    var region = new CellRangeAddress(currentRow, currentRow, 0, _salaryFields.Count);
                    sheet.AddMergedRegion(region);
                    RegionUtil.SetBorderBottom(1, region, sheet);
                    RegionUtil.SetBorderLeft(1, region, sheet);
                    RegionUtil.SetBorderRight(1, region, sheet);
                    RegionUtil.SetBorderTop(1, region, sheet);
                    sheet.EnsureCell(currentRow, 0).SetCellValue(g.Key);
                    currentRow++;
                    var startGroupRow = currentRow;
                    foreach (var p in group)
                    {
                        WriteDataInCell(textStyle, intStyle, decimalStyle, ref stt, ref column, p,sumColumns);
                    }
                    var fields = group.Select(s => s).FirstOrDefault();

                    for (int i = 0; i <= sumColumns.Count; i++)
                    {
                        sheet.EnsureCell(currentRow, i, textStyle);
                        if (sumColumns.ContainsKey(i))
                        {
                            if (sumColumns[i].IsCalcSum)
                            {
                                var cell = sheet.EnsureCell(currentRow, i, sumColumns[i].DataTypeId == EnumDataType.Decimal ? decimalStyle : intStyle);
                                cell.SetCellFormula($"SUM({sheet.EnsureCell(startGroupRow, i).Address}:{sheet.EnsureCell(currentRow -1, i).Address})");
                            }
                        }
                    }
                    currentRow++;
                }
            }
            else
            {
                var stt = 1;
                var startGroupRow = currentRow;
                foreach (var p in groupSalaryEmployees)
                {
                    WriteDataInCell(textStyle, intStyle, decimalStyle, ref stt, ref column, p, sumColumns);
                }
                for (int i = 0; i <= sumColumns.Count; i++)
                {
                    sheet.EnsureCell(currentRow, i, textStyle);
                    if (sumColumns.ContainsKey(i))
                    {
                        if (sumColumns[i].IsCalcSum)
                        {
                            var cell = sheet.EnsureCell(currentRow, i, sumColumns[i].DataTypeId == EnumDataType.Decimal ? decimalStyle : intStyle);
                            cell.SetCellFormula($"SUM({sheet.EnsureCell(startGroupRow, i).Address}:{sheet.EnsureCell(currentRow -1, i).Address})");
                        }
                    }
                }
               
                currentRow++;
            }
            return "";
        }
        private string IListToString(IList<string> strs)
        {
            var str = new StringBuilder();
            for (int i = 0; i < strs.Count; i++)
            {
                if (!string.IsNullOrEmpty(strs[i]))
                    str.Append(str.Length == 0 ? strs[i] : $", {strs[i]}");

            }
            return str.ToString();
        }
        private void WriteDataInCell(ICellStyle textStyle, ICellStyle intStyle, ICellStyle decimalStyle, ref int stt, ref int column, NonCamelCaseDictionary<SalaryEmployeeValueModel> p, Dictionary<int, SalaryFieldModel> sumColumns)
        {
            var sColIndex = 1;
            sheet.EnsureCell(currentRow, 0, intStyle).SetCellValue(stt);
            foreach (var g in groups)
            {
                var groupCols = _salaryFields.Where(f => f.GroupName == g);

                foreach (var f in groupCols)
                {
                    p.TryGetValue(f.SalaryFieldName, out var value);
                    switch (f.DataTypeId)
                    {
                        case EnumDataType.BigInt:
                        case EnumDataType.Int:
                            if (!value.IsNullOrEmptyObject())
                                sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                    .SetCellFormula(value.Value?.ToString());
                            else
                                sheet.EnsureCell(currentRow, sColIndex, intStyle);
                            break;
                        case EnumDataType.Decimal:
                            if (!value.IsNullOrEmptyObject())
                                sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                    .SetCellFormula(value.Value?.ToString());
                            else
                                sheet.EnsureCell(currentRow, sColIndex, decimalStyle);
                            break;
                        default:
                            sheet.EnsureCell(currentRow, sColIndex, textStyle).SetCellValue(value?.Value?.ToString());
                            break;
                    }
                    if (value?.Value?.ToString()?.Length > columnMaxLineLength[sColIndex])
                    {
                        columnMaxLineLength[sColIndex] = value.Value?.ToString()?.Length ?? 10;
                    }
                    if (!sumColumns.ContainsKey(sColIndex))
                        sumColumns.Add(sColIndex, f);
                    sColIndex++;
                }
            }

            column = sColIndex;
            currentRow++;
            stt++;
        }
    }
}
