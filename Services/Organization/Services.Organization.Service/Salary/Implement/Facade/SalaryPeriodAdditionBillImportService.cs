using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using Verp.Resources.Organization;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.Facade;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Services.Organization.Model.Salary;
using VErp.Infrastructure.EF.EFExtensions;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Library.Utilities;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{

    public class SalaryPeriodAdditionBillImportService : SalaryPeriodAdditionBillFieldAbstract, ISalaryPeriodAdditionBillImportService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _billActivityLog;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ILongTaskResourceLockService _longTaskResourceLockService;
        private readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;


        public SalaryPeriodAdditionBillImportService(
            OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IActivityLogService activityLogService,
            ICustomGenCodeHelperService customGenCodeHelperService, ICategoryHelperService categoryHelperService,
            ILongTaskResourceLockService longTaskResourceLockService,
            ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService,
            ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService)
            : base(categoryHelperService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _billActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionBill);
            _customGenCodeHelperService = customGenCodeHelperService;
            _longTaskResourceLockService = longTaskResourceLockService;
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping(int salaryPeriodAdditionTypeId)
        {
            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

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

            result.Fields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillBase>();

            foreach (var d in await GetFieldDetailsForMapping(typeInfo))
            {
                result.Fields.Add(d);
            }

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

            var parseFacade = new SalaryPeriodAdditionBillParseFacadeContext(mapping, categoryHelperService, typeInfo);

            using (var longTask = await _longTaskResourceLockService.Accquire($"Nhập dữ liệu thưởng/phụ cấp và khấu trừ \"{typeInfo.Title}\" từ excel"))
            {
                var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(SalaryPeriodAdditionBillBase.BillCode));
                if (columnKey == null)
                {
                    throw HrDataValidationMessage.BillCodeError.BadRequest();
                }

                var reader = new ExcelReader(stream);
                reader.RegisterLongTaskEvent(longTask);
                
                var excelRows = ReadExcelData(mapping, reader);

                var billsByCode = excelRows.Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                    .GroupBy(r => r.Data[columnKey.Column])
                    .ToDictionary(r => r.Key, r => r.ToIList());

                longTask.SetCurrentStep("Kiểm tra dữ liệu", billsByCode.Count());

                await parseFacade.LoadEmployees(_organizationDBContext, billsByCode.SelectMany(c => c.Value).ToList());

                var bills = new Dictionary<SalaryPeriodAdditionBillModel, IList<SalaryAdditionBilExcelRow>>();

                var contentMapping = mapping.MappingFields.FirstOrDefault(m => m.FieldName == nameof(SalaryPeriodAdditionBillModel.Content));
                var yearMapping = mapping.MappingFields.FirstOrDefault(m => m.FieldName == nameof(SalaryPeriodAdditionBillModel.Year));
                var monthMapping = mapping.MappingFields.FirstOrDefault(m => m.FieldName == nameof(SalaryPeriodAdditionBillModel.Month));
                var dateMapping = mapping.MappingFields.FirstOrDefault(m => m.FieldName == nameof(SalaryPeriodAdditionBillModel.Date));

                foreach (var bill in billsByCode)
                {
                    var modelBill = new SalaryPeriodAdditionBillModel();
                    bills.Add(modelBill, bill.Value);

                    modelBill.BillCode = bill.Key;
                    modelBill.Details = new List<SalaryPeriodAdditionBillEmployeeModel>();
                    int count = bill.Value.Count();
                    for (int rowIndex = 0; rowIndex < count; rowIndex++)
                    {
                        var row = bill.Value.ElementAt(rowIndex);

                        if (row.Data.TryGetValue(contentMapping?.Column ?? "", out var content) && !string.IsNullOrWhiteSpace(content))
                        {
                            modelBill.Content = content;
                        }

                        if (row.Data.TryGetValue(yearMapping?.Column ?? "", out var strYear) && int.TryParse(strYear, out var year))
                        {
                            modelBill.Year = year;
                        }

                        if (row.Data.TryGetValue(monthMapping?.Column ?? "", out var strMonth) && int.TryParse(strMonth, out var month))
                        {
                            modelBill.Month = month;
                        }

                        if (row.Data.TryGetValue(dateMapping?.Column ?? "", out var strDate) && DateTime.TryParse(strDate, out var date))
                        {
                            modelBill.Date = date.GetUnixUtc(_currentContextService.TimeZoneOffset);
                        }

                        parseFacade.MapAndLoadRowToModel(row, modelBill.BillCode, modelBill.Details);
                    }


                    longTask.IncProcessedRows();

                }

                var codes = billsByCode.Select(c => c.Key).Where(c => !c.IsNullOrEmptyObject()).ToList();

                var existedBills = (await _salaryPeriodAdditionBillService.QueryFullInfo()
                    .Where(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && codes.Contains(b.BillCode))
                    .ToListAsync()
                    ).ToDictionary(b => b.BillCode?.ToLower(), b => b);

                var existedCodes = existedBills.Keys.ToHashSet();


                var opt = EnumCustomNormalizeAndValidateOption.All | EnumCustomNormalizeAndValidateOption.IgnoreRequired;

                ICollection<CustomValidationResult> results = new List<CustomValidationResult>();

                foreach (var (model, rows) in bills)
                {
                    var rowIndex = rows.First().Index;

                    if (!CustomValidator.TryNormalizeAndValidateObject(model, results, opt))
                    {
                        var firstError = results.FirstOrDefault();
                        var propName = firstError?.MemberNames?.LastOrDefault();
                        var mappingProp = mapping.MappingFields.FirstOrDefault(m => m.FieldName == propName);

                        throw new BadRequestException(GeneralCode.InvalidParams, $"Lỗi dữ liệu {firstError.DisplayName} dòng {rowIndex}, cột {mappingProp?.Column} không hợp lệ, " + string.Join(", ", firstError?.MemberNames) + ": " + firstError?.ErrorMessage);
                    }
                }

                var createBills = bills.Where(b => !existedCodes.Contains(b.Key.BillCode?.ToLower()));

                Func<Type, IList<PropertyInfo>> getRequiredProp = (type) =>
                {
                    return type.GetProperties()
                    .Where(p =>
                    {
                        var reqireAtt = p.GetCustomAttribute<RequiredAttribute>();
                        return reqireAtt != null;

                    }).ToList();
                };


                var requiredModelProps = getRequiredProp(typeof(SalaryPeriodAdditionBillModel));

                var requiredDetailProps = getRequiredProp(typeof(SalaryPeriodAdditionBillEmployeeModel));

                Action<IList<PropertyInfo>, object, int> validateRequiredProps = (requiedProps, obj, rowIndex) =>
                {
                    foreach (var prop in requiedProps)
                    {
                        if (prop.GetValue(obj) == null)
                        {
                            var displayAtt = prop.GetCustomAttribute<DisplayAttribute>();
                            var mappingProp = mapping.MappingFields.FirstOrDefault(m => m.FieldName == prop.Name);
                            throw GeneralCode.InvalidParams.BadRequest($"Trường {(displayAtt?.Name) ?? prop.Name} bắt buộc, dòng {rowIndex}, cột {mappingProp?.Column}");
                        }
                    }
                };

                foreach (var (createModel, rows) in createBills)
                {
                    var rowIndex = rows.First().Index;
                    validateRequiredProps(requiredModelProps, createModel, rowIndex);

                    foreach (var d in createModel.Details)
                    {
                        validateRequiredProps(requiredDetailProps, d, rowIndex);
                    }

                }


                var updateBills = bills.Where(b => existedCodes.Contains(b.Key.BillCode?.ToLower()));
                if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied && updateBills.Count() > 0)
                {
                    var firstDuplicate = updateBills.First();
                    var firstRow = firstDuplicate.Value.First();
                    throw GeneralCode.ItemCodeExisted.BadRequest($"Chứng từ {firstDuplicate.Key.BillCode} đã tồn tại, dòng {firstRow.Index}, cột {columnKey.Column}");
                }

                if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.IgnoreBill)
                {
                    updateBills = new List<KeyValuePair<SalaryPeriodAdditionBillModel, IList<SalaryAdditionBilExcelRow>>>();
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
                                .SetConfig(EnumObjectType.SalaryPeriodAdditionBill, EnumObjectType.SalaryPeriodAdditionType, salaryPeriodAdditionTypeId, typeInfo.Title)
                                .SetConfigData(0, date.GetUnixUtc(_currentContextService.TimeZoneOffset))
                                .TryValidateAndGenerateCode(_organizationDBContext.SalaryPeriodAdditionBill, model.Key.BillCode, (s, code) => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && s.BillCode == code);

                            model.Key.BillCode = code;

                            var info = await _salaryPeriodAdditionBillService.CreateToDb(typeInfo, model.Key, parseFacade.Employees);

                            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.CreateFromExcel)
                                 .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
                                 .ObjectId(info.SalaryPeriodAdditionBillId)
                                 .JsonData(model)
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


                            var info = await _salaryPeriodAdditionBillService.UpdateToDb(typeInfo, updateModel.SalaryPeriodAdditionBillId, model.Key, parseFacade.Employees);

                            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.UpdateFromExcel)
                               .MessageResourceFormatDatas(model.Key.BillCode, typeInfo.Title)
                               .ObjectId(entity.SalaryPeriodAdditionBillId)
                               .JsonData(model.Key)
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



    }
}
