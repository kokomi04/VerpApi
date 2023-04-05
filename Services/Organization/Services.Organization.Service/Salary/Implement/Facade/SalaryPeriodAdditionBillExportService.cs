using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using Verp.Resources.Organization;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Commons.Constants;
using VErp.Services.Organization.Model.Salary;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Verp.Resources.Organization.Salary;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.Util;
using static NPOI.HSSF.UserModel.HeaderFooter;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{

    public class SalaryPeriodAdditionBillExportService : ISalaryPeriodAdditionBillExportService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;



        public SalaryPeriodAdditionBillExportService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, ICategoryHelperService categoryHelperService, ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _categoryHelperService = categoryHelperService;
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(int salaryPeriodAdditionTypeId, int? year, int? month, string keyword)
        {
            var bills = await _salaryPeriodAdditionBillService.GetListQuery(salaryPeriodAdditionTypeId, year, month, keyword)
                .Include(b => b.SalaryPeriodAdditionBillEmployee)
                .ThenInclude(e => e.SalaryPeriodAdditionBillEmployeeValue)
                .AsNoTracking()
                .ToListAsync();

            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId);

            var xssfwb = new XSSFWorkbook();
            var sheet = xssfwb.CreateSheet();



            var employeeIds = bills.SelectMany(b => b.SalaryPeriodAdditionBillEmployee.Select(e => e.EmployeeId)).ToList(); ;

            var (employees, employeeFields) = await GetEmployees(employeeIds);


            var TITLE_ROW_INDEX = 3;

            var fRow = TITLE_ROW_INDEX;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);

            sheet.EnsureCell(fRow, 1).SetCellValue($"Năm");
            sheet.SetHeaderCellStyle(fRow, 1);

            sheet.EnsureCell(fRow, 2).SetCellValue($"Tháng");
            sheet.SetHeaderCellStyle(fRow, 2);

            sheet.EnsureCell(fRow, 3).SetCellValue($"Số chứng từ");
            sheet.SetHeaderCellStyle(fRow, 3);

            sheet.EnsureCell(fRow, 4).SetCellValue($"Ngày chứng từ");
            sheet.SetHeaderCellStyle(fRow, 4);

            sheet.EnsureCell(fRow, 5).SetCellValue($"Nội dung");
            sheet.SetHeaderCellStyle(fRow, 5);

            var currentColumnIndex = 5;

            employeeFields = employeeFields.Where(f => f.CategoryFieldName != CategoryFieldConstants.F_Id && !f.IsHidden).ToList();

            for (var i = 0; i < employeeFields.Count; i++)
            {
                currentColumnIndex++;
                var field = employeeFields.ElementAt(i);
                sheet.EnsureCell(fRow, currentColumnIndex).SetCellValue(field.CategoryFieldName);
                sheet.SetHeaderCellStyle(fRow, currentColumnIndex);
            }


            for (var i = 0; i < typeInfo.SalaryPeriodAdditionTypeField.Count; i++)
            {
                currentColumnIndex++;
                var field = typeInfo.SalaryPeriodAdditionTypeField.ElementAt(i).SalaryPeriodAdditionField;
                sheet.EnsureCell(fRow, currentColumnIndex).SetCellValue(field.Title);
                sheet.SetHeaderCellStyle(fRow, currentColumnIndex);
            }

            sheet.EnsureCell(1, currentColumnIndex).SetCellValue("Danh sách " + typeInfo.Title);


            var stt = 0;
            var textStyle = sheet.GetCellStyle(isBorder: true);
            var dateStyle = sheet.GetCellStyle(isBorder: true, dataFormat: "dd/MM/yyyy");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");


            foreach (var bill in bills)
            {
                foreach (var detail in bill.SalaryPeriodAdditionBillEmployee)
                {
                    fRow++;
                    stt++;
                    sheet.EnsureCell(fRow, 0, textStyle).SetCellValue(stt);

                    sheet.EnsureCell(fRow, 1, textStyle).SetCellValue(bill.Year);

                    sheet.EnsureCell(fRow, 2, textStyle).SetCellValue(bill.Month);

                    sheet.EnsureCell(fRow, 3, textStyle).SetCellValue(bill.BillCode);

                    sheet.EnsureCell(fRow, 4, dateStyle).SetCellValue(bill.Date.AddMinutes(_currentContextService.TimeZoneOffset ?? -420));

                    sheet.EnsureCell(fRow, 5, textStyle).SetCellValue(bill.Content);

                    currentColumnIndex = 5;

                    var employeeInfo = employees.FirstOrDefault(e =>
                         {
                             return e.TryGetValue(CategoryFieldConstants.F_Id, out var employeeData) && Convert.ToInt64(employeeData) == detail.EmployeeId;
                         });

                    for (var i = 0; i < employeeFields.Count; i++)
                    {
                        currentColumnIndex++;
                        var cell = sheet.EnsureCell(fRow, currentColumnIndex, textStyle);
                        if (employeeInfo != null)
                        {
                            var field = employeeFields.ElementAt(i);
                            employeeInfo.TryGetValue(field.CategoryFieldName, out var employeeData);
                            cell.SetCellValue(employeeData?.ToString());
                        }
                    }


                    for (var i = 0; i < typeInfo.SalaryPeriodAdditionTypeField.Count; i++)
                    {
                        currentColumnIndex++;
                        var field = typeInfo.SalaryPeriodAdditionTypeField.ElementAt(i).SalaryPeriodAdditionField;
                        var value = detail.SalaryPeriodAdditionBillEmployeeValue.FirstOrDefault(v => v.SalaryPeriodAdditionFieldId == field.SalaryPeriodAdditionFieldId);
                        var cell = sheet.EnsureCell(fRow, currentColumnIndex, decimalStyle);
                        if (value != null && value.Value.HasValue)
                        {
                            var doubleValue = Convert.ToDouble(value);
                            cell.SetCellValue(doubleValue);
                        }

                    }
                }
            }



            if (sheet.LastRowNum < 1000)
            {
                for (var i = 0; i < currentColumnIndex + 1; i++)
                {
                    sheet.AutoSizeColumn(i, false);
                }
            }
            else
            {
                for (var i = 0; i < currentColumnIndex + 1; i++)
                {
                    sheet.ManualResize(i, sheet.EnsureCell(TITLE_ROW_INDEX, i).StringCellValue?.Length ?? 10);
                }
            }


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"customer-list-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }

        private async Task<(List<NonCamelCaseDictionary> employees, List<ReferFieldModel> employeeFields)> GetEmployees(IList<long> employeeIds)
        {
            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await _categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            var clause = new SingleClause()
            {
                DataType = EnumDataType.BigInt,
                FieldName = CategoryFieldConstants.F_Id,
                Operator = EnumOperator.InList,
                Value = employeeIds
            };
            var employeeView = $"v{OrganizationConstants.EMPLOYEE_CATEGORY_CODE}";
            var condition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            int prefix = 0;
            prefix = clause.FilterClauseProcess(employeeView, employeeView, condition, sqlParams, prefix, false, null, null);
            var employeeData = await _organizationDBContext.QueryDataTable($"SELECT * FROM {employeeView} WHERE {condition}", sqlParams.ToArray());
            return (employeeData.ConvertData(), refCategory);
        }
    }
}
