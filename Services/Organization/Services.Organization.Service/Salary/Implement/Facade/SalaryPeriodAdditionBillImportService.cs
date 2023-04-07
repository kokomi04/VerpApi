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
using Verp.Resources.Organization.Salary.Validation;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{

    public class SalaryPeriodAdditionBillImportService : ISalaryPeriodAdditionBillImportService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _billActivityLog;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly ILongTaskResourceLockService _longTaskResourceLockService;
        private readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;

        private static IList<CategoryFieldNameModel> detailFields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillEmployeeModel>();
        private readonly CategoryFieldNameModel employeeField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId));
        private readonly CategoryFieldNameModel desField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.Description));


        public SalaryPeriodAdditionBillImportService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IActivityLogService activityLogService, ICustomGenCodeHelperService customGenCodeHelperService, ICategoryHelperService categoryHelperService, ILongTaskResourceLockService longTaskResourceLockService, ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService, ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _billActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionBill);
            _customGenCodeHelperService = customGenCodeHelperService;
            _categoryHelperService = categoryHelperService;
            _longTaskResourceLockService = longTaskResourceLockService;
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping(int salaryPeriodAdditionTypeId)
        {
            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField)
                 .ThenInclude(tf => tf.SalaryPeriodAdditionField)
                 .Where(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId)
                 .FirstOrDefaultAsync();

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }


            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.HrTypeId,
                CategoryCode = typeInfo.SalaryPeriodAdditionTypeId + "",
                CategoryTitle = typeInfo.Title,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await _categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            result.Fields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillBase>();

            var detailFields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillEmployeeModel>();
            var employeeField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId));
            var desField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.Description));

            const string detailGroupName = "Chi tiết";
            result.Fields.Add(new CategoryFieldNameModel()
            {
                //CategoryFieldId = field.HrAreaFieldId,
                FieldName = employeeField.FieldName,
                FieldTitle = employeeField.FieldTitle,
                IsRequired = employeeField.IsRequired,
                GroupName = detailGroupName,
                RefCategory = new CategoryNameModel()
                {
                    //CategoryId = 0,
                    CategoryCode = refCategory.FirstOrDefault()?.CategoryCode,
                    CategoryTitle = refCategory.FirstOrDefault()?.CategoryTitle,
                    IsTreeView = false,
                    Fields = refCategory
                        .Select(f => new CategoryFieldNameModel()
                        {
                            //CategoryFieldId = f.id,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = f.GetTitleCategoryField(),
                            RefCategory = null,
                            IsRequired = false
                        }).ToList()
                }
            });

            foreach (var field in typeInfo.SalaryPeriodAdditionTypeField)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    //CategoryFieldId = field.HrAreaFieldId,
                    FieldName = field.SalaryPeriodAdditionField.FieldName,
                    FieldTitle = field.SalaryPeriodAdditionField.Title,
                    RefCategory = null,
                    IsRequired = false,
                    GroupName = detailGroupName
                };

                result.Fields.Add(fileData);
            }


            result.Fields.Add(new CategoryFieldNameModel()
            {
                //CategoryFieldId = field.HrAreaFieldId,
                FieldName = desField.FieldName,
                FieldTitle = desField.FieldTitle,
                IsRequired = true,
                GroupName = detailGroupName,
                RefCategory = null
            });

            result.Fields.Add(new CategoryFieldNameModel
            {
                FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                FieldTitle = "Cột kiểm tra",
            });

            return result;
        }

        public async Task<bool> Import(int salaryPeriodAdditionTypeId, ImportExcelMapping mapping, Stream stream)
        {
            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (!typeInfo.IsActived)
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInActived.BadRequestFormat(typeInfo.Title);
            }


            using (var longTask = await _longTaskResourceLockService.Accquire($"Nhập dữ liệu thưởng/phụ cấp và khấu trừ \"{typeInfo.Title}\" từ excel"))
            {
                var reader = new ExcelReader(stream);
                reader.RegisterLongTaskEvent(longTask);

                var dataExcel = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

                var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();
                var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(SalaryPeriodAdditionBillBase.BillCode));
                if (columnKey == null)
                {
                    throw HrDataValidationMessage.BillCodeError.BadRequest();
                }

                var billsByCode = dataExcel.Rows.Select((r, i) => new BillDataRow
                {
                    Data = r,
                    Index = i + mapping.FromRow
                })
                    .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                    .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                    .GroupBy(r => r.Data[columnKey.Column])
                    .ToDictionary(r => r.Key, r => r.ToIList());



                longTask.SetCurrentStep("Kiểm tra dữ liệu", billsByCode.Count());


                var (employees, employeeRefField, employeeMappingField) = await GetEmployees(mapping, billsByCode);


                var bills = new Dictionary<SalaryPeriodAdditionBillModel, IList<BillDataRow>>();

                foreach (var bill in billsByCode)
                {
                    var modelBill = new SalaryPeriodAdditionBillModel();
                    bills.Add(modelBill, bill.Value);

                    modelBill.BillCode = bill.Key;
                    modelBill.Details = new List<SalaryPeriodAdditionBillEmployeeModel>();
                    int count = bill.Value.Count();
                    for (int rowIndex = 0; rowIndex < count; rowIndex++)
                    {
                        var mapRow = new NonCamelCaseDictionary();
                        var row = bill.Value.ElementAt(rowIndex);

                        if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillModel.Content), out var content) && !string.IsNullOrWhiteSpace(content))
                        {
                            modelBill.Content = content;
                        }

                        if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillModel.Year), out var strYear) && int.TryParse(strYear, out var year))
                        {
                            modelBill.Year = year;
                        }

                        if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillModel.Month), out var strMonth) && int.TryParse(strMonth, out var month))
                        {
                            modelBill.Month = month;
                        }

                        if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillModel.Date), out var strDate) && DateTime.TryParse(strDate, out var date))
                        {
                            modelBill.Date = date.GetUnixUtc(_currentContextService.TimeZoneOffset);
                        }


                        var rowData = new SalaryPeriodAdditionBillEmployeeModel();

                        if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillEmployeeModel.Description), out var des))
                        {
                            rowData.Description = des;
                        }

                        rowData.Values = new NonCamelCaseDictionary<decimal?>();

                        modelBill.Details.Add(rowData);
                        var employeeId = MatchEmployeeId(employees, row, employeeRefField, employeeMappingField);

                        if (modelBill.Details.Any(d => d != rowData && d.EmployeeId == employeeId))
                        {
                            throw GeneralCode.InvalidParams.BadRequest($"Tồn tại nhiều nhân sự cùng 1 chứng từ {modelBill.BillCode}, dòng {row.Index}, cột {employeeMappingField.Column}");
                        }
                        rowData.EmployeeId = employeeId;


                        foreach (var field in typeInfo.SalaryPeriodAdditionTypeField)
                        {
                            var mappingInfo = mapping.MappingFields.FirstOrDefault(f => f.FieldName == field.SalaryPeriodAdditionField.FieldName);
                            if (mappingInfo == null)
                            {
                                continue;
                            }

                            if (row.Data.TryGetValue(field.SalaryPeriodAdditionField.FieldName, out var strValue) && !string.IsNullOrWhiteSpace(strValue))
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
                    }


                    longTask.IncProcessedRows();

                }

                var codes = billsByCode.Select(c => c.Key).Where(c => !c.IsNullOrEmptyObject()).ToList();

                var existedBills = (await _salaryPeriodAdditionBillService.QueryFullInfo()
                    .Where(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && codes.Contains(b.BillCode))
                    .ToListAsync()
                    ).ToDictionary(b => b.BillCode?.ToLower(), b => b);

                var existedCodes = existedBills.Keys.ToHashSet();

                var createBills = bills.Where(b => existedCodes.Contains(b.Key.BillCode?.ToLower()));

                var updateBills = bills.Where(b => !existedCodes.Contains(b.Key.BillCode?.ToLower()));
                if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied && updateBills.Count() > 0)
                {
                    var firstDuplicate = updateBills.First();
                    var firstRow = firstDuplicate.Value.First();
                    throw GeneralCode.ItemCodeExisted.BadRequest($"Chứng từ {firstDuplicate.Key.BillCode} đã tồn tại, dòng {firstRow.Index}, cột {columnKey.Column}");
                }

                var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
                try
                {
                    longTask.SetCurrentStep("Lưu vào cơ sở dữ liệu", bills.Count());
                    var baseValueChains = new Dictionary<string, int>();

                    using (var batchLog = _billActivityLog.BeginBatchLog())
                    {
                        var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();


                        foreach (var model in createBills)
                        {
                            var date = new DateTime(model.Key.Year ?? 0, model.Key.Month ?? 0, 1);

                            var code = await ctx
                                .SetConfig(EnumObjectType.SalaryPeriodAdditionBill, EnumObjectType.SalaryPeriodAdditionType, salaryPeriodAdditionTypeId)
                                .SetConfigData(0, date.GetUnixUtc(_currentContextService.TimeZoneOffset))
                                .TryValidateAndGenerateCode(_organizationDBContext.SalaryPeriodAdditionBill, model.Key.BillCode, (s, code) => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && s.BillCode == code);

                            model.Key.BillCode = code;

                            var info = await _salaryPeriodAdditionBillService.CreateToDb(typeInfo, model.Key, employees);

                            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.CreateFromExcel)
                                 .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
                                 .ObjectId(info.SalaryPeriodAdditionBillId)
                                 .JsonData(model.JsonSerialize())
                                 .CreateLog();

                            longTask.IncProcessedRows();
                        }

                        foreach (var model in updateBills)
                        {
                            var entity = existedBills[model.Key.BillCode?.ToLower()];
                            var updateModel = _salaryPeriodAdditionBillService.MapInfo(entity);

                            updateModel.UpdateIfAvaiable(u => u.BillCode, model.Key.BillCode);
                            updateModel.UpdateIfAvaiable(u => u.Year, model.Key.Year);
                            updateModel.UpdateIfAvaiable(u => u.Year, model.Key.Year);
                            updateModel.UpdateIfAvaiable(u => u.Month, model.Key.Month);
                            updateModel.UpdateIfAvaiable(u => u.Date, model.Key.Date);
                            updateModel.UpdateIfAvaiable(u => u.Content, model.Key.Content);

                            foreach (var newDetail in model.Key.Details)
                            {
                                var existedDetail = updateModel.Details.FirstOrDefault(e => e.EmployeeId == newDetail.EmployeeId);
                                if (existedDetail == null)
                                {
                                    updateModel.Details.Add(newDetail);
                                }
                                else
                                {
                                    existedDetail.UpdateIfAvaiable(v => v.Description, newDetail.Description);
                                    foreach (var newValue in newDetail.Values.Where(v => v.Value.HasValue))
                                    {
                                        if (existedDetail.Values.ContainsKey(newValue.Key))
                                        {
                                            existedDetail.Values[newValue.Key] = newValue.Value;
                                        }
                                        else
                                        {
                                            existedDetail.Values.Add(newValue.Key, newValue.Value);
                                        }
                                    }
                                }
                            }


                            var info = await _salaryPeriodAdditionBillService.UpdateToDb(typeInfo, updateModel.SalaryPeriodAdditionBillId, model.Key, employees);

                            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.UpdateFromExcel)
                               .MessageResourceFormatDatas(model.Key.BillCode, typeInfo.Title)
                               .ObjectId(entity.SalaryPeriodAdditionBillId)
                               .JsonData(model.Key.JsonSerialize())
                               .CreateLog();

                            longTask.IncProcessedRows();
                        }

                        await @trans.CommitAsync();
                        await batchLog.CommitAsync();
                        await ctx.ConfirmCode();
                    }
                }
                catch
                {

                    await @trans.TryRollbackTransactionAsync();
                    throw;
                }
                return true;
            }


        }

        private async Task<(List<NonCamelCaseDictionary> emplyees, ReferFieldModel employeeRefField, ImportExcelMappingField employeeMappingField)> GetEmployees(ImportExcelMapping mapping, IDictionary<string, IList<BillDataRow>> billsByCode)
        {
            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await _categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            var employeeMappingField = mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId));
            if (employeeMappingField == null)
            {
                throw HrDataValidationMessage.FieldRequired.BadRequestFormat(employeeField.FieldTitle + " (" + employeeField.FieldName + ")");
            }

            ReferFieldModel employeeRefField = null;

            employeeRefField = refCategory.FirstOrDefault(rf => rf.CategoryFieldName == employeeMappingField.RefFieldName);
            if (employeeRefField == null)
            {
                throw HrDataValidationMessage.RefFieldNotExisted.BadRequestFormat(employeeMappingField.RefFieldName, OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }
            var clause = new SingleClause()
            {
                DataType = (EnumDataType)employeeRefField.DataTypeId,
                FieldName = employeeRefField.CategoryFieldName,
                Operator = EnumOperator.InList,
                Value = billsByCode.SelectMany(b => b.Value.Select(r =>
                {
                    r.Data.TryGetValue(employeeMappingField.FieldName, out var employeeData);
                    return employeeData;
                }))
                .Where(b => !b.IsNullOrEmptyObject())
                .ToList()
            };
            var employeeView = $"v{OrganizationConstants.EMPLOYEE_CATEGORY_CODE}";
            var condition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            int prefix = 0;
            prefix = clause.FilterClauseProcess(employeeView, employeeView, condition, sqlParams, prefix, false, null, null);
            var employeeData = await _organizationDBContext.QueryDataTable($"SELECT * FROM {employeeView} WHERE {condition}", sqlParams.ToArray());
            return (employeeData.ConvertData(), employeeRefField, employeeMappingField);


        }


        private long MatchEmployeeId(List<NonCamelCaseDictionary> employees, BillDataRow row, ReferFieldModel employeeRefField, ImportExcelMappingField employeeMappingField)
        {

            if (row.Data.TryGetValue(nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId), out var rowEmpoyee) && !string.IsNullOrWhiteSpace(rowEmpoyee))
            {

                var employeeRow = employees.Where(e =>
                {
                    return e.TryGetValue(employeeRefField.CategoryFieldName, out var employeeData) && employeeData?.ToString()?.NormalizeAsInternalName() == rowEmpoyee?.NormalizeAsInternalName();

                }).ToList();
                if (employeeRow.Count == 0)
                {
                    throw GeneralCode.ItemNotFound.BadRequest($"Không tìm thấy nhân sự có {employeeRefField.CategoryFieldTitle} là {rowEmpoyee}, dòng {row.Index}, cột {employeeMappingField.Column}");
                }

                if (employeeRow.Count > 1)
                {
                    employeeRow = employees.Where(e =>
                    {
                        return e.TryGetValue(employeeRefField.CategoryFieldName, out var employeeData) && employeeData?.ToString()?.ToLower() == rowEmpoyee?.ToLower();

                    }).ToList();
                    if (employeeRow.Count != 1)
                        throw GeneralCode.ItemNotFound.BadRequest($"Tìm thấy nhiều hơn 1 nhân sự có {employeeRefField.CategoryFieldTitle} là {rowEmpoyee}, dòng {row.Index}, cột {employeeMappingField.Column}");
                }

                long employeeId = 0;
                if (employeeRow[0].TryGetValue(CategoryFieldConstants.F_Id, out var strId))
                {
                    long.TryParse(strId + "", out employeeId);
                }

                if (employeeId == 0)
                {
                    throw GeneralCode.ItemNotFound.BadRequest($"ID nhân viên không hợp lệ, kiểm tra lại cấu hình danh mục {employeeRefField.CategoryTitle} ({employeeRefField.CategoryCode}), dòng {row.Index}, cột {employeeMappingField.Column}");
                }


                return employeeId;
            }
            else
            {
                throw GeneralCode.ItemNotFound.BadRequest($"Không tìm thấy nhân sự, dòng {row.Index}, cột {employeeMappingField.Column}");
            }

        }
        private class BillDataRow
        {
            public int Index { get; set; }
            public NonCamelCaseDictionary<string> Data { get; set; }
        }
    }
}
