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

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{
    public class SalaryGroupEmployeeExportFacade
    {
        private readonly ISalaryFieldService _salaryFieldService;
        private readonly IList<string> _fieldsName;

        private ISheet sheet = null;
        private int currentRow = 0;
        private IList<SalaryFieldModel> _salaryFields;
        private IList<int> columnMaxLineLength = new List<int>();
        private const string EMPLOYEE_F_ID = "F_Id";
        private const string EMPLOYEE_FIELD_NAME = "ho_ten";

        public SalaryGroupEmployeeExportFacade(IList<string> fieldsName, ISalaryFieldService salaryFieldService)
        {
            _fieldsName = fieldsName;
            _salaryFieldService = salaryFieldService;
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields)
        {

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();
            await GetSalaryField();

            var employees = WriteTable(groupSalaryEmployees, groupFields);
            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = StringUtils.RemoveDiacritics($"group salary employee {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx").Replace(" ", "#");

            return (stream, fileName, contentType);
        }

        private async Task GetSalaryField()
        {
            var salaryFields = await _salaryFieldService.GetList();
            _salaryFields = new List<SalaryFieldModel>();
            foreach (var field in _fieldsName)
            {
                var salaryField = salaryFields.FirstOrDefault(x => x.SalaryFieldName == field);
                if (salaryField == null)
                    throw new BadRequestException($"Không tìm thấy trường {field} trong bảng lương");
                else
                    _salaryFields.Add(salaryField);
            }
        }
        private string WriteTable(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields)
        {

            if (groupSalaryEmployees.Count == 0)
                throw new BadRequestException("Không tìm thấy nhân sự trong bảng lương");
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;
            columnMaxLineLength = new List<int>(_salaryFields.Count + 1);
            columnMaxLineLength.Add(5);
            foreach (var g in _salaryFields)
            {
                sheet.EnsureCell(fRow, sColIndex).SetCellValue(g.Title);
                sheet.SetHeaderCellStyle(fRow, sColIndex);
                sColIndex++;
                columnMaxLineLength.Add(g.Title?.Length ?? 10);
            }

            currentRow = sRow + 1;

            return WriteTableDetailData(groupSalaryEmployees, groupFields);
        }
        private string WriteTableDetailData(IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> groupSalaryEmployees, IList<string> groupFields)
        {
            var stt = 1;
            var textStyle = sheet.GetCellStyle(isBorder: true);
            var intStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");
            int column = 0;

            if (groupFields!=null && groupFields.Count > 0)
            {
                if (!groupSalaryEmployees.Any(x => groupFields.Any(g => x.ContainsKey(g))))
                {
                    throw new BadRequestException($"Không tìm thấy trường dữ liệu {groupFields} để thực hiện gom nhóm");
                }

                var groups = from f in groupSalaryEmployees
                             group f by groupFields.Select(x=>  f[x].Value).JsonSerialize()
                             into g
                             select g;
                foreach (var g in groups)
                {
                    var group = g.OrderBy(x => x[EMPLOYEE_FIELD_NAME].Value.ToString().Split(' ').Last());
                    foreach (var p in group)
                    {
                        WriteDataInCell(textStyle, intStyle, decimalStyle, ref stt, ref column, p);
                    }

                }
            } else
            {
                foreach (var p in groupSalaryEmployees)
                {
                    WriteDataInCell(textStyle, intStyle, decimalStyle, ref stt, ref column, p);
                }
            }
            for (int i = 0; i < column; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            return "";
        }
        private void WriteDataInCell(ICellStyle textStyle, ICellStyle intStyle, ICellStyle decimalStyle, ref int stt, ref int column, NonCamelCaseDictionary<SalaryEmployeeValueModel> p)
        {
            var sColIndex = 1;
            sheet.EnsureCell(currentRow, 0, intStyle).SetCellValue(stt);
            foreach (var f in _salaryFields)
            {
                p.TryGetValue(f.SalaryFieldName, out var value);
                switch (f.DataTypeId)
                {
                    case EnumDataType.BigInt:
                    case EnumDataType.Int:
                        if (!value.IsNullOrEmptyObject())
                            sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                .SetCellFormula(value?.Value?.ToString());
                        else
                            sheet.EnsureCell(currentRow, sColIndex, intStyle);
                        break;
                    case EnumDataType.Decimal:
                        if (!value.IsNullOrEmptyObject())
                            sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                .SetCellFormula(value?.Value?.ToString());
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
                sColIndex++;
            }
            column = sColIndex;
            currentRow++;
            stt++;
        }
    }
}
