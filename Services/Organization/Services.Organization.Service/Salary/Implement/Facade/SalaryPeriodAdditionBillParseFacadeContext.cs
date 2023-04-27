using Microsoft.Data.SqlClient;
using System.Collections.Generic;
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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Constants;
using VErp.Services.Organization.Model.Salary;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{

    internal class SalaryPeriodAdditionBillParseFacadeContext : SalaryPeriodAdditionBillFieldAbstract
    {

        public List<NonCamelCaseDictionary> Employees;

        private SalaryPeriodAdditionType _typeFullInfo;
        private ImportExcelMapping _mapping;
        private ReferFieldModel _employeeRefField;
        private ImportExcelMappingField _employeeMappingField;
        private ImportExcelMappingField _descriptionMapping;
        private Dictionary<string, ImportExcelMappingField> _fieldMappings = new Dictionary<string, ImportExcelMappingField>();

        public SalaryPeriodAdditionBillParseFacadeContext(ImportExcelMapping mapping, ICategoryHelperService categoryHelperService, SalaryPeriodAdditionType typeFullInfo)
            : base(categoryHelperService)
        {
            this._typeFullInfo = typeFullInfo;
            this._mapping = mapping;
            _descriptionMapping = mapping.MappingFields.FirstOrDefault(m => m.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.Description));

            foreach (var field in _typeFullInfo.SalaryPeriodAdditionTypeField)
            {
                var mappingInfo = mapping.MappingFields.FirstOrDefault(f => f.FieldName == field.SalaryPeriodAdditionField.FieldName);
                if (mappingInfo != null)
                {
                    _fieldMappings.Add(field.SalaryPeriodAdditionField.FieldName, mappingInfo);
                }
            }
        }


        public async Task LoadEmployees(DbContext dbContext, IList<SalaryAdditionBilExcelRow> rows)
        {
            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            _employeeMappingField = _mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId));
            if (_employeeMappingField == null)
            {
                throw HrDataValidationMessage.FieldRequired.BadRequestFormat(employeeField.FieldTitle + " (" + employeeField.FieldName + ")");
            }

            _employeeRefField = null;

            _employeeRefField = refCategory.FirstOrDefault(rf => rf.CategoryFieldName == _employeeMappingField.RefFieldName);
            if (_employeeRefField == null)
            {
                throw HrDataValidationMessage.RefFieldNotExisted.BadRequestFormat(_employeeMappingField.RefFieldName, OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }
            var clause = new SingleClause()
            {
                DataType = (EnumDataType)_employeeRefField.DataTypeId,
                FieldName = _employeeRefField.CategoryFieldName,
                Operator = EnumOperator.InList,
                Value = rows.Select(r =>
                {
                    r.Data.TryGetValue(_employeeMappingField.Column, out var employeeData);
                    return employeeData;
                })
                .Where(b => !b.IsNullOrEmptyObject())
                .ToList()
            };
            var employeeView = $"v{OrganizationConstants.EMPLOYEE_CATEGORY_CODE}";
            var condition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            int prefix = 0;
            prefix = clause.FilterClauseProcess(employeeView, employeeView, condition, sqlParams, prefix, false, null, null);
            var employeeData = await dbContext.QueryDataTable($"SELECT * FROM {employeeView} WHERE {condition}", sqlParams.ToArray());
            Employees = employeeData.ConvertData();
        }


        public SalaryPeriodAdditionBillEmployeeParseInfo MapAndLoadRowToModel(SalaryAdditionBilExcelRow row, string billCode, IList<SalaryPeriodAdditionBillEmployeeModel> details)
        {
            var rowData = new SalaryPeriodAdditionBillEmployeeParseInfo();


            if (row.Data.TryGetValue(_descriptionMapping?.Column ?? "", out var des))
            {
                rowData.Description = des;
            }

            rowData.Values = new NonCamelCaseDictionary<decimal?>();

            var (employeeId, employeeInfo) = MatchEmployeeId(row);

            if (details.Any(d => d != rowData && d.EmployeeId == employeeId))
            {
                throw GeneralCode.InvalidParams.BadRequest($"Tồn tại nhiều nhân sự cùng 1 chứng từ {billCode}, dòng {row.Index}, cột {_employeeMappingField.Column}");
            }
            rowData.EmployeeId = employeeId;


            foreach (var field in _typeFullInfo.SalaryPeriodAdditionTypeField)
            {
                if (!_fieldMappings.TryGetValue(field.SalaryPeriodAdditionField.FieldName, out var mappingInfo))
                {
                    continue;
                }

                if (row.Data.TryGetValue(mappingInfo.Column, out var strValue) && !string.IsNullOrWhiteSpace(strValue))
                {
                    if (!decimal.TryParse(strValue, out var decimalValue))
                    {
                        throw GeneralCode.InvalidParams.BadRequest($"Không thể chuyển đổi giá trị {strValue} sang kiểu số, dòng {row.Index}, cột {mappingInfo.Column}");
                    }
                    else
                    {
                        rowData.Values.Add(field.SalaryPeriodAdditionField.FieldName, decimalValue);

                    }
                }
            }
            details.Add(rowData);
            return rowData;
        }


        private (long employeeId, NonCamelCaseDictionary employeeInfo) MatchEmployeeId(SalaryAdditionBilExcelRow row)
        {

            if (row.Data.TryGetValue(_employeeMappingField.Column, out var rowEmpoyee) && !string.IsNullOrWhiteSpace(rowEmpoyee))
            {

                var employeeRows = Employees.Where(e =>
                {
                    return e.TryGetValue(_employeeRefField.CategoryFieldName, out var employeeData) && employeeData?.ToString()?.NormalizeAsInternalName() == rowEmpoyee?.NormalizeAsInternalName();

                }).ToList();
                if (employeeRows.Count == 0)
                {
                    throw GeneralCode.ItemNotFound.BadRequest($"Không tìm thấy nhân sự có {_employeeRefField.CategoryFieldTitle} là {rowEmpoyee}, dòng {row.Index}, cột {_employeeMappingField.Column}");
                }

                if (employeeRows.Count > 1)
                {
                    employeeRows = Employees.Where(e =>
                    {
                        return e.TryGetValue(_employeeRefField.CategoryFieldName, out var employeeData) && employeeData?.ToString()?.ToLower() == rowEmpoyee?.ToLower();

                    }).ToList();
                    if (employeeRows.Count != 1)
                        throw GeneralCode.ItemNotFound.BadRequest($"Tìm thấy nhiều hơn 1 nhân sự có {_employeeRefField.CategoryFieldTitle} là {rowEmpoyee}, dòng {row.Index}, cột {_employeeMappingField.Column}");
                }

                long employeeId = 0;
                if (employeeRows[0].TryGetValue(CategoryFieldConstants.F_Id, out var strId))
                {
                    long.TryParse(strId + "", out employeeId);
                }

                if (employeeId == 0)
                {
                    throw GeneralCode.ItemNotFound.BadRequest($"ID nhân viên không hợp lệ, kiểm tra lại cấu hình danh mục {_employeeRefField.CategoryTitle} ({_employeeRefField.CategoryCode}), dòng {row.Index}, cột {_employeeMappingField.Column}");
                }

                return (employeeId, employeeRows[0]);
            }
            else
            {
                throw GeneralCode.ItemNotFound.BadRequest($"Không tìm thấy nhân sự, dòng {row.Index}, cột {_employeeMappingField.Column}");
            }

        }

    }
}
