﻿using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.Salary.Implement.Abstract;
using VErp.Services.Organization.Service.Salary.Implement.Facade;
using static VErp.Services.Organization.Service.Salary.Implement.Facade.SalaryPeriodAdditionBillFieldAbstract;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryEmployeeService : SalaryPeriodGroupEmployeeAbstract, ISalaryEmployeeService
    {

        private readonly ObjectActivityLogFacade _salaryPeriodGroupActivityLog;

        private readonly ISalaryGroupService _salaryGroupService;
        private readonly ISalaryRefTableService _salaryRefTableService;
        private readonly IHrDataService _hrDataService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ISalaryFieldService _salaryFieldService;
        private readonly ISalaryPeriodService _salaryPeriodService;
        private readonly ISalaryPeriodGroupService _salaryPeriodGroupService;
        private readonly IProgramingFunctionHelperService _programingFunctionHelperService;
        private readonly ILogger<SalaryEmployeeService> logger;
        private const string SALARY_FIELD_PREFIX = "__";
        private const string SALARY_PARAM_PREFIX = "A_A_";

        private const int DEFAULT_DECIMAL_PLACE = 2;

        private const string ADDITION_ALIAS = "pc_va_khau_tru$";

        private const string EMPLOYEE_SALARY_FIELD_NAME = "ho_ten";

        private const string EMPLOYEE_SALARY_GROUP_ID_FIELD = "salaryGroupId";

        private const string EMPLOYEE_SALARY_EMPLOYEE_EMPLOYEE_ID_FIELD = "EmployeeId";



        public SalaryEmployeeService(OrganizationDBContext organizationDBContext,
            ICurrentContextService currentContextService,
            IMapper mapper,
            IActivityLogService activityLogService,
            ISalaryGroupService salaryGroupService,
            ISalaryRefTableService salaryRefTableService,
            IHrDataService hrDataService,
            ICategoryHelperService httpCategoryHelperService,
            ISalaryFieldService salaryFieldService,
            ISalaryPeriodService salaryPeriodService,
            ISalaryPeriodGroupService salaryPeriodGroupService,
            IProgramingFunctionHelperService programingFunctionHelperService,
            ILogger<SalaryEmployeeService> logger)
            : base(organizationDBContext, currentContextService, logger)
        {

            _salaryPeriodGroupActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodGroup);

            _salaryGroupService = salaryGroupService;
            _salaryRefTableService = salaryRefTableService;
            _hrDataService = hrDataService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _salaryFieldService = salaryFieldService;
            _salaryPeriodService = salaryPeriodService;
            _salaryPeriodGroupService = salaryPeriodGroupService;
            _programingFunctionHelperService = programingFunctionHelperService;
            this.logger = logger;
        }


        public async Task<GroupSalaryEmployeeWarningInfo> GetSalaryGroupEmployeesWarning()
        {
            var groups = await _salaryGroupService.GetList();

            var (allEmployees, _) = await FilterEmployee(null, DateTime.Now.Year, DateTime.Now.Month, DateTime.UtcNow.GetUnix(), DateTime.UtcNow.GetUnix());

            var employeeGroups = allEmployees
                .ToDictionary(e =>
                {
                    long employeeId = 0;
                    if (e.TryGetValue(OrganizationConstants.HR_TABLE_F_IDENTITY, out var employeeIdObj))
                    {
                        employeeId = Convert.ToInt64(employeeIdObj);
                    }

                    return employeeId;
                }, e => new EmployeeSalaryGroupInfo()
                {
                    EmployeeInfo = e,
                    SalaryGroupIds = new List<int>()
                });

            foreach (var group in groups)
            {
                var (groupEmployees, _) = await FilterEmployee(group.EmployeeFilter, DateTime.Now.Year, DateTime.Now.Month, DateTime.UtcNow.GetUnix(), DateTime.UtcNow.GetUnix());

                foreach (var employee in groupEmployees)
                {
                    long employeeId = 0;
                    if (employee.TryGetValue(OrganizationConstants.HR_TABLE_F_IDENTITY, out var employeeIdObj))
                    {
                        employeeId = Convert.ToInt64(employeeIdObj);
                    }

                    if (employeeGroups.TryGetValue(employeeId, out var employeeGroup))
                    {
                        employeeGroup.SalaryGroupIds.Add(group.SalaryGroupId);
                    }
                }
            }

            return new GroupSalaryEmployeeWarningInfo()
            {
                NoSalaryGroupEmployees = employeeGroups.Values.Where(e => e.SalaryGroupIds.Count == 0).Select(e => e.EmployeeInfo).ToList(),
                DuplicatedSalayGroupEmployees = employeeGroups.Values.Where(e => e.SalaryGroupIds.Count > 1).ToList(),
            };
        }
        public async Task<PageData<NonCamelCaseDictionary>> GetEmployeeGroupInfo(Clause filter, int page, int size)
        {

            var (employeesOfGroup, _) = await FilterEmployee(filter, DateTime.Now.Year, DateTime.Now.Month, DateTime.UtcNow.GetUnix(), DateTime.UtcNow.GetUnix());

            if (employeesOfGroup.Count == 0)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy nhân viên trong bảng lương");
            IList<NonCamelCaseDictionary> lstEmployees = null;
            if (page == 0 && size == 0)
                lstEmployees = employeesOfGroup.OrderBy(x => x.ContainsKey(EMPLOYEE_SALARY_FIELD_NAME) ? x[EMPLOYEE_SALARY_FIELD_NAME].ToString().Split(' ').Last() : null).ToList();
            else
                lstEmployees = employeesOfGroup.OrderBy(x => x.ContainsKey(EMPLOYEE_SALARY_FIELD_NAME) ? x[EMPLOYEE_SALARY_FIELD_NAME].ToString().Split(' ').Last() : null).Skip((page - 1) * size).Take(size).ToList();

            return (lstEmployees, employeesOfGroup.Count);
        }
        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> EvalSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel req)
        {
            var period = await _salaryPeriodService.GetInfo(salaryPeriodId);
            if (period == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var groupInfo = await _salaryGroupService.GetInfo(salaryGroupId);

            if (groupInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var salaryFields = await _salaryFieldService.GetList();
            var sortedSalaryFields = new SortedSalaryFields(salaryFields);

            return await EvalSalaryEmployeeByGroup(period, groupInfo, sortedSalaryFields, req);
        }


        private async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> EvalSalaryEmployeeByGroup(SalaryPeriodInfo period, SalaryGroupInfo groupInfo, SortedSalaryFields sortedSalaryFields, GroupSalaryEmployeeModel req)
        {


            var (employees, columns) = await FilterEmployee(groupInfo.EmployeeFilter, period.Year, period.Month, req.FromDate, req.ToDate, false);

            var subsidiaryInfo = await _organizationDBContext.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == _currentContextService.SubsidiaryId);
            if (subsidiaryInfo == null)
            {
                throw GeneralCode.NotYetSupported.BadRequest();
            }
            var sqlEmployeeSalarySamePeriodButOtherGroups = $"SELECT EmployeeId FROM _SalaryEmployee_{subsidiaryInfo.SubsidiaryCode} WHERE SalaryPeriodId = @SalaryPeriodId AND SalaryGroupId <> @SalaryGroupId AND IsDeleted = 0";

            var employeeSalarySamePeriodButOtherGroups = (await _organizationDBContext.QueryDataTableRaw(sqlEmployeeSalarySamePeriodButOtherGroups, new[]
            {
                new SqlParameter("@SalaryPeriodId", period.SalaryPeriodId),
                new SqlParameter("@SalaryGroupId",  groupInfo.SalaryGroupId)
            })).ConvertData();


            var employeeInOtherGroups = employeeSalarySamePeriodButOtherGroups.Select(e => Convert.ToInt64(e.First().Value))
                .Distinct()
                .ToHashSet();

            var employeeData = new List<NonCamelCaseDictionary>();
            foreach (var item in employees)
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]);
                }
                if (employeeInOtherGroups.Contains(employeeId)) continue;
                employeeData.Add(item);
            }


            var data = new PeriodGroupSalaryEmployeeEvelInput()
            {
                FromDate = req.FromDate,
                ToDate = req.ToDate,
                Salaries = req.Salaries,

                PeriodInfo = period,
                GroupInfo = groupInfo,
                SortedSalaryFields = sortedSalaryFields
            };

            return await EvalSalaryEmployeeExpressionByGroup(data, employeeData, false);

        }

        private async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> EvalSalaryEmployeeExpressionByGroup(PeriodGroupSalaryEmployeeEvelInput data, IList<NonCamelCaseDictionary> employees, bool overrideNotRefData)
        {
            var period = data.PeriodInfo;

            var groupInfo = data.GroupInfo;

            var salaryFields = data.SortedSalaryFields.SalariesFields;


            var result = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

            var groupFields = groupInfo.TableFields.ToDictionary(t => t.SalaryFieldId, t => t);

            if (data.Salaries == null)
            {
                data.Salaries = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();
            }

            var reqDataByEmployee = data.Salaries.ToDictionary(item =>
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                }
                return employeeId;
            }, item => item);

            var sqlFunctions = await _programingFunctionHelperService.GetAllSqls();
            var sqlFnHandle = _organizationDBContext.EvaluateFunctionHandlerSql(sqlFunctions);


            var employeeIds = new HashSet<long>();
            foreach (var item in employees.OrderBy(e =>
            {
                var code = e.ContainsKey(OrganizationConstants.BILL_CODE) ? e[OrganizationConstants.BILL_CODE]?.ToString() : "";
                var id = e.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY) ? e[OrganizationConstants.HR_TABLE_F_IDENTITY]?.ToString() : "";
                return code + "_" + id;
            }))
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]);
                }
                if (employeeId == 0)
                {
                    throw SalaryPeriodValidationMessage.EmployeeIDZero.BadRequest();
                }
                if (employeeIds.Contains(employeeId))
                {
                    throw SalaryPeriodValidationMessage.EmployeeIDDuplicated.BadRequestFormat(employeeId);
                }

                employeeIds.Add(employeeId);

                var paramsData = new NonCamelCaseDictionary();

                foreach (var valuePaire in item)
                {
                    if (valuePaire.Value != null && valuePaire.Value.GetType() == typeof(DateTime))
                    {
                        paramsData.Add(valuePaire.Key, ((DateTime)valuePaire.Value).UtcToTimeZone(_currentContextService.TimeZoneOffset));
                    }
                    else
                    {
                        paramsData.Add(valuePaire.Key, valuePaire.Value);
                    }
                }

                paramsData.Add($"{SALARY_PARAM_PREFIX}FromDate", data.FromDate);
                paramsData.Add($"{SALARY_PARAM_PREFIX}ToDate", data.ToDate);
                paramsData.Add($"{SALARY_PARAM_PREFIX}Month", period.Month);
                paramsData.Add($"{SALARY_PARAM_PREFIX}Year", period.Year);


                var model = new NonCamelCaseDictionary<SalaryEmployeeValueModel>
                {
                    { OrganizationConstants.HR_TABLE_F_IDENTITY, new SalaryEmployeeValueModel(employeeId) }
                };

                result.Add(model);

                reqDataByEmployee.TryGetValue(employeeId, out var reqItem);

                foreach (var f in salaryFields)
                {
                    var fieldVariableName = SALARY_FIELD_PREFIX + f.SalaryFieldName;

                    var isFieldInGroup = groupFields.TryGetValue(f.SalaryFieldId, out var groupField);

                    object fieldValue = null;
                    var isEdited = false;

                    if (isFieldInGroup)
                    {
                        var fieldIsEditable = f.IsEditable && (!isFieldInGroup || groupField.IsEditable);
                        SalaryEmployeeValueModel reqValue = null;
                        var evalDataIsEdited = reqItem != null && reqItem.TryGetValue(f.SalaryFieldName, out reqValue) && reqValue?.IsEdited == true;

                        if (!f.IsDisplayRefData && (fieldIsEditable && evalDataIsEdited || overrideNotRefData))
                        {
                            fieldValue = reqValue?.Value;
                            isEdited = reqValue?.IsEdited == true;
                        }
                        else
                        {
                            foreach (var condition in f.Expression)
                            {

                                var (isSucess, value) = await EvalValueExpression(f, condition, paramsData, sqlFnHandle);
                                if (isSucess)
                                {
                                    fieldValue = value;
                                }
                            }
                        }

                    }

                    //if (fieldValue == null &&!f.IsDisplayRefData && f.DataTypeId.IsNumber())
                    //{
                    //    fieldValue = 0;
                    //}


                    if (fieldValue == null)
                    {
                        paramsData.Add(fieldVariableName, GetDefaultValue(f.DataTypeId, data));
                    }
                    else
                    {
                        if (f.DataTypeId != EnumDataType.Text && f.DataTypeId != EnumDataType.PhoneNumber && GetDecimal(fieldValue, f.DecimalPlace, out var decimalValue))
                        {
                            fieldValue = decimalValue;
                        }

                        if (f.DataTypeId == EnumDataType.Date)
                        {
                            fieldValue = EnumDataType.Date.GetSqlValueAtTimezone(fieldValue, _currentContextService.TimeZoneOffset);
                        }

                        paramsData.Add(fieldVariableName, fieldValue);
                    }

                    if (isFieldInGroup)
                    {
                        model.Add(f.SalaryFieldName, new SalaryEmployeeValueModel(fieldValue, isEdited));
                    }
                }
            }

            return result;
        }



        private async Task<(bool isSucess, object value)> EvalValueExpression(SalaryFieldModel field, SalaryFieldExpressionModel condition, NonCamelCaseDictionary paramsData, NCalc.EvaluateFunctionHandler functionHandler)
        {
            var filter = condition?.Filter;
            await NormalizeFieldNameInClause(filter);
            bool conditionResult;
            try
            {
                conditionResult = EvalClause(filter, paramsData, functionHandler);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "EvalClause {0}, field {1}, condition {2}", condition.Filter, field.SalaryFieldName, condition.Name);
                throw GeneralCode.NotYetSupported.BadRequest($"Lỗi kiểm tra điều kiện {condition.Name} trường {field.GroupName} {field.SalaryFieldName} ({field.Title}). Lỗi {e.Message}");
            }

            if (conditionResult && !string.IsNullOrWhiteSpace(condition.ValueExpression))
            {
                try
                {
                    var value = EvalUtils.EvalObject(EscaseFieldName(condition.ValueExpression), paramsData, functionHandler);
                    return (true, value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Eval {0}, field {1}, condition {2}", condition.ValueExpression, field.SalaryFieldName, condition.Name);
                    throw GeneralCode.NotYetSupported.BadRequest($"Lỗi tính giá trị biểu thức {condition.ValueExpression} trường {field.GroupName} {field.SalaryFieldName} ({field.Title}), điều kiện {condition.Name}. Lỗi {ex.Message}");
                }

            }
            return (false, null);
        }

        public async Task<IList<GroupSalaryEmployeeEvalData>> GetSalaryEmployeeAll(int salaryPeriodId)
        {
            var period = await _salaryPeriodService.GetInfo(salaryPeriodId);
            if (period == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var groups = await _salaryPeriodGroupService.GetList(salaryPeriodId);

            var result = new List<GroupSalaryEmployeeEvalData>();
            var salaryFields = await _salaryFieldService.GetList();
            var sortedSalaryFields = new SortedSalaryFields(salaryFields);
            foreach (var group in groups)
            {
                var groupInfo = await _salaryGroupService.GetInfo(group.SalaryGroupId);

                result.Add(new GroupSalaryEmployeeEvalData()
                {
                    FromDate = group.FromDate,
                    ToDate = group.ToDate,
                    SalaryGroupId = group.SalaryGroupId,
                    SalaryPeriodId = group.SalaryPeriodId,
                    Salaries = await GetSalaryEmployeePeriodByGroup(period, group, groupInfo, sortedSalaryFields)
                });
            }

            return result;
        }

        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetInfoEmployeeByGroupSalary(int salaryPeriodId, int salaryGroupId)
        {
            if (salaryGroupId == 0)
            {
                var result = await GetSalaryEmployeeAll(salaryPeriodId);
                var data = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();
                foreach (var salaryGroup in result)
                {
                    var salaryGroupTitle = await _salaryGroupService.GetInfo(salaryGroup.SalaryGroupId);
                    foreach (var item in salaryGroup.Salaries)
                    {
                        item.Add(EMPLOYEE_SALARY_GROUP_ID_FIELD, new SalaryEmployeeValueModel() { Value = salaryGroupTitle.Title });
                    }
                    data.AddRange(salaryGroup.Salaries);
                }
                return data;
            }
            return await GetSalaryEmployeeByGroup(salaryPeriodId, salaryGroupId);
        }

        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId)
        {
            var period = await _salaryPeriodService.GetInfo(salaryPeriodId);
            if (period == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var group = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);
            if (group == null)
            {
                return new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();
            }

            var groupInfo = await _salaryGroupService.GetInfo(salaryGroupId);

            if (groupInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var salaryFields = await _salaryFieldService.GetList();

            return await GetSalaryEmployeePeriodByGroup(period, group, groupInfo, new SortedSalaryFields(salaryFields));
        }

        private async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetSalaryEmployeePeriodByGroup(SalaryPeriodInfo period, SalaryPeriodGroupInfo group, SalaryGroupInfo groupInfo, SortedSalaryFields sortedSalaryFields)
        {
            var subsidiaryInfo = await _organizationDBContext.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == _currentContextService.SubsidiaryId);
            if (subsidiaryInfo == null)
            {
                throw GeneralCode.NotYetSupported.BadRequest();
            }
            var sql = $"SELECT * FROM {GetEmployeeSalaryTableName(subsidiaryInfo.SubsidiaryCode)} WHERE SalaryPeriodId = @SalaryPeriodId AND SalaryGroupId = @SalaryGroupId AND IsDeleted = 0";

            var salaryData = (await _organizationDBContext.QueryDataTableRaw(sql, new[]
            {
                new SqlParameter("@SalaryPeriodId", period.SalaryPeriodId),
                new SqlParameter("@SalaryGroupId",  group.SalaryGroupId)
            })).ConvertData();


            var dbSalaries = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

            var resultByEmployee = new Dictionary<long, NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

            var fromDate = group.FromDate;
            var toDate = group.ToDate;


            var salaryFields = await _salaryFieldService.GetList();


            foreach (var item in salaryData)
            {
                var model = new NonCamelCaseDictionary<SalaryEmployeeValueModel>();
                dbSalaries.Add(model);

                long employeeId = 0;
                if (item.ContainsKey(EMPLOYEE_SALARY_EMPLOYEE_EMPLOYEE_ID_FIELD))
                {
                    employeeId = Convert.ToInt64(item[EMPLOYEE_SALARY_EMPLOYEE_EMPLOYEE_ID_FIELD]);
                }


                resultByEmployee.Add(employeeId, model);

                model.Add(OrganizationConstants.HR_TABLE_F_IDENTITY, new SalaryEmployeeValueModel(employeeId));

                foreach (var f in salaryFields)
                {
                    if (item.ContainsKey(f.SalaryFieldName))
                    {
                        var value = item[f.SalaryFieldName];
                        item.TryGetValue($"{f.SalaryFieldName}_IsEdited", out var isEdited);

                        model.Add(f.SalaryFieldName, new SalaryEmployeeValueModel(value, Convert.ToBoolean(isEdited)));
                    }
                }
            }

            var clause = new SingleClause()
            {
                Value = resultByEmployee.Keys.ToArray(),
                DataType = EnumDataType.BigInt,
                FieldName = OrganizationConstants.HR_TABLE_F_IDENTITY,
                Operator = EnumOperator.InList
            };

            var (employees, columns) = await FilterEmployee(clause, period.Year, period.Month, fromDate, toDate, false);

            var data = new PeriodGroupSalaryEmployeeEvelInput()
            {
                FromDate = fromDate,
                ToDate = toDate,
                Salaries = dbSalaries,

                PeriodInfo = period,
                GroupInfo = groupInfo,
                SortedSalaryFields = sortedSalaryFields
            };

            return await EvalSalaryEmployeeExpressionByGroup(data, employees, true);
        }

        public async Task<bool> Update(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel model)
        {
            var period = await _salaryPeriodService.GetInfo(salaryPeriodId);
            if (period == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await ValidateDateOfBill(new DateTime(period.Year, period.Month, 1).ToUniversalTime(), null);

            var groupInfo = await _salaryGroupService.GetInfo(salaryGroupId);
            if (groupInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var periodGroup = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);


            var salaryFields = await _salaryFieldService.GetList();
            var updateSalaryFields = salaryFields.Where(f => !f.IsDisplayRefData).OrderBy(f => f.SalaryFieldId).ToList();

            var evalData = await EvalSalaryEmployeeByGroup(period, groupInfo, new SortedSalaryFields(salaryFields), model);

            var evalDataByEmployee = evalData.ToDictionary(item =>
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                }
                return employeeId;
            }, item => item);

            if (model.Salaries.Count != evalDataByEmployee.Count)
            {
                throw SalaryPeriodValidationMessage.DiffNumberOfUpdatedEmployeeSalary.BadRequest();
            }

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                long salaryPeriodGroupId;
                if (periodGroup == null)
                {
                    salaryPeriodGroupId = await _salaryPeriodGroupService.Create(new SalaryPeriodGroupModel()
                    {
                        SalaryPeriodId = salaryPeriodId,
                        SalaryGroupId = salaryGroupId,
                        FromDate = model.FromDate,
                        ToDate = model.ToDate
                    });

                }
                else
                {
                    salaryPeriodGroupId = periodGroup.SalaryPeriodGroupId;
                }

                await _salaryPeriodGroupService.DbUpdate(salaryPeriodGroupId, new SalaryPeriodGroupModel()
                {
                    SalaryPeriodId = salaryPeriodId,
                    SalaryGroupId = salaryGroupId,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate
                }, true);


                await DeleteSalaryEmployeeByPeriodGroup(salaryPeriodId, salaryGroupId);


                var groupFields = groupInfo.TableFields.ToDictionary(t => t.SalaryFieldId, t => t);

                var subsidiaryInfo = await _organizationDBContext.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == _currentContextService.SubsidiaryId);
                if (subsidiaryInfo == null)
                {
                    throw GeneralCode.NotYetSupported.BadRequest();
                }

                var tableType = $"_SalaryEmployeeTableType_{subsidiaryInfo.SubsidiaryCode}";

                var dataTable = new System.Data.DataTable(tableType);
                dataTable.Columns.Add("SalaryEmployeeId", typeof(long));
                dataTable.Columns.Add("SubsidiaryId", typeof(int));
                dataTable.Columns.Add("EmployeeId", typeof(long));
                dataTable.Columns.Add("SalaryPeriodId", typeof(long));
                dataTable.Columns.Add("SalaryGroupId", typeof(int));
                dataTable.Columns.Add("CreatedByUserId", typeof(int));
                dataTable.Columns.Add("CreatedDatetimeUtc", typeof(DateTime));
                dataTable.Columns.Add("UpdatedByUserId", typeof(int));
                dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));
                dataTable.Columns.Add("IsDeleted", typeof(bool));
                dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));

                foreach (var f in updateSalaryFields)
                {
                    var type = typeof(string);
                    //object defaultValue = "";
                    switch (f.DataTypeId)
                    {
                        case EnumDataType.Int:
                        case EnumDataType.BigInt:
                        case EnumDataType.Decimal:
                            type = typeof(decimal);
                            //defaultValue = 0;
                            break;
                        case EnumDataType.Date:
                            type = typeof(DateTime);
                            //  defaultValue = null;
                            break;
                        case EnumDataType.Boolean:
                            type = typeof(bool);
                            //defaultValue = false;
                            break;

                    }
                    dataTable.Columns.Add(new DataColumn(f.SalaryFieldName, type));// { DefaultValue = defaultValue });
                    dataTable.Columns.Add(new DataColumn(f.SalaryFieldName + "_IsEdited", typeof(bool)) { DefaultValue = false });
                }

                foreach (var item in model.Salaries)
                {
                    long employeeId = 0;
                    if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                    {
                        employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                    }


                    if (!evalDataByEmployee.TryGetValue(employeeId, out var evalItem))
                    {
                        throw SalaryPeriodValidationMessage.EmployeeNotFoundInEval.BadRequestFormat(employeeId);
                    }


                    var row = dataTable.NewRow();
                    dataTable.Rows.Add(row);

                    row["SalaryEmployeeId"] = DBNull.Value;
                    row["SubsidiaryId"] = _currentContextService.SubsidiaryId;
                    row["EmployeeId"] = employeeId;
                    row["SalaryPeriodId"] = salaryPeriodId;
                    row["SalaryGroupId"] = salaryGroupId;
                    row["CreatedByUserId"] = _currentContextService.UserId;
                    row["CreatedDatetimeUtc"] = DateTime.UtcNow;
                    row["UpdatedByUserId"] = _currentContextService.UserId;
                    row["UpdatedDatetimeUtc"] = DateTime.UtcNow;

                    foreach (var field in updateSalaryFields)
                    {
                        var hasDataValue = item.TryGetValue(field.SalaryFieldName, out var dataValue);

                        if (!field.IsEditable || groupFields.TryGetValue(field.SalaryFieldId, out var groupField) && !groupField.IsEditable)
                        {
                            SalaryEmployeeValueModel evalValue = null;
                            if (evalItem != null)
                            {
                                evalItem.TryGetValue(field.SalaryFieldName, out evalValue);
                            }


                            if (dataValue?.IsEdited == true || evalValue?.IsEdited == true || !IsEqualFieldsValue(field.DataTypeId, dataValue?.Value, evalValue?.Value))
                            {
                                throw SalaryPeriodValidationMessage.FieldIsNotEditable.BadRequestFormat(field.GroupName + " > " + field.Title, dataValue?.Value, evalValue?.Value);
                            }
                        }

                        if (hasDataValue)
                        {
                            if (dataValue != null && !dataValue.Value.IsNullOrEmptyObject())
                            {
                                row[field.SalaryFieldName] = dataValue.Value;
                                row[field.SalaryFieldName + "_IsEdited"] = dataValue.IsEdited;

                            }
                        }

                    }
                }


                var fields = new List<string>() { "SubsidiaryId", "EmployeeId", "SalaryPeriodId", "SalaryGroupId", "CreatedByUserId", "CreatedDatetimeUtc", "UpdatedByUserId", "UpdatedDatetimeUtc" };
                foreach (var f in updateSalaryFields.OrderBy(f => f.SalaryFieldId))
                {
                    fields.Add($"[{f.SalaryFieldName}]");
                    fields.Add($"[{f.SalaryFieldName + "_IsEdited"}]");
                }
                var sql = $"INSERT INTO {GetEmployeeSalaryTableName(subsidiaryInfo.SubsidiaryCode)} ({string.Join(',', fields.ToArray())}) SELECT {string.Join(',', fields.ToArray())} FROM @Data";

                await _organizationDBContext.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Data", dataTable) { TypeName = tableType, SqlDbType = SqlDbType.Structured });

                var periodInfo = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
                if (periodInfo == null)
                {
                    throw GeneralCode.ItemNotFound.BadRequest();
                }

                periodInfo.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;

                await _organizationDBContext.SaveChangesAsync();


                await trans.CommitAsync();

                await _salaryPeriodGroupActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.UpdateSalaryEmployee)
                   .MessageResourceFormatDatas(groupInfo.Title, period.Month, period.Year)
                   .ObjectId(salaryPeriodGroupId)
                   .JsonData(model)
                   .CreateLog();

                return true;
            }
        }


        private async Task<(IList<NonCamelCaseDictionary> data, IList<string> columns)> FilterEmployee(Clause filter, int year, int month, long fromDate, long toDate, bool dateTimeToUnix = true)
        {
            var columns = new List<string>();

            var (query, fieldNames) = await _hrDataService.BuildHrQuery(OrganizationConstants.HR_EMPLOYEE_TYPE_CODE, false);

            var select = new StringBuilder();
            var join = new StringBuilder($"({query}) v");

            var sqlFunction = await _programingFunctionHelperService.GetAllSqls();

            foreach (var f in fieldNames)
            {
                columns.Add(f);
                select.Append($"v.{f}");
                select.Append(",");
            }


            var data = new NonCamelCaseDictionary()
            {
                {"FromDate",fromDate.UnixToDateTime() },
                {"ToDate",toDate.UnixToDateTime() },
                {"Year",year },
                {"Month",month },
            };

            var sqlParams = new List<SqlParameter>()
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Year", year),
                new SqlParameter("@Month", month)
            };
            var suffix = 0;

            suffix = await JoinRefTables(columns, select, join, suffix, sqlParams, data);

            var whereCondition = new StringBuilder();


            if (filter != null)
            {
                await NormalizeFieldNameInClause(filter, sqlFunction, fromDate, toDate, month, year);

                suffix = filter.FilterClauseProcess($"({query}) vm", "v", whereCondition, sqlParams, suffix, false, null, data);
            }
            var queryData = $"SELECT * FROM (SELECT {select.ToString().TrimEnd().TrimEnd(',')} FROM {join}) v " + (whereCondition.Length > 0 ? "WHERE " : " ") + whereCondition;
            var dataTable = await _organizationDBContext.QueryDataTableRaw(queryData, sqlParams.ToArray());

            var lstData = dataTable.ConvertData(dateTimeToUnix);

            var additionValues = await PeriodAdditionValues(year, month);

            var additionBillFields = await _organizationDBContext.SalaryPeriodAdditionField.AsNoTracking().ToListAsync();

            var duplicateData = new Dictionary<long, NonCamelCaseDictionary>();
            foreach (var item in lstData)
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]);
                }
                if (duplicateData.TryGetValue(employeeId, out var existedItem))
                {

                    foreach (var (fieldName, v1) in item)
                    {
                        existedItem.TryGetValue(fieldName, out var v2);
                        if (v1?.ToString() != v2?.ToString())
                        {
                            throw GeneralCode.InternalError.BadRequest($"Ambiguous value for employee ID {employeeId}, FieldName ({fieldName}): {v1}, {v2}");
                        }
                    }
                }
                else
                {
                    duplicateData.Add(employeeId, item);
                }

                if (additionValues.TryGetValue(employeeId, out var fieldValues))
                {
                    foreach (var (fieldName, value) in fieldValues)
                    {
                        var colName = EscaseFieldName($"{ADDITION_ALIAS}.{fieldName}");
                        item.Add(colName, value);
                    }
                }
                foreach (var f in additionBillFields)
                {
                    var colName = EscaseFieldName($"{ADDITION_ALIAS}.{f.FieldName}");
                    if (!item.ContainsKey(colName))
                        item.Add(colName, 0);
                }

            }

            return (lstData, columns);
        }

        private async Task<int> JoinRefTables(List<string> columns, StringBuilder select, StringBuilder join, int suffix, List<SqlParameter> sqlParams, NonCamelCaseDictionary data)
        {
            var refTables = (_salaryRefTableService.GetList()).Result;
            var refFields = await _httpCategoryHelperService.GetReferFields(refTables.Select(c => c.RefTableCode).ToList(), null);

            foreach (var refTable in refTables)
            {
                var fromField = refTable.FromField;
                var lastPoint = fromField.LastIndexOf('.');

                var refAlias = $"{refTable.Alias}";

                if (lastPoint < 0)
                {
                    fromField = $"v.{fromField}";
                }


                var cateFields = refFields.Where(f => f.CategoryCode == refTable.RefTableCode).ToList();

                foreach (var f in cateFields)
                {
                    var colName = $"{refTable.Alias}.{f.CategoryFieldName}";
                    colName = EscaseFieldName(colName);
                    var idx = 1;
                    var originalColName = colName;
                    while (columns.Contains(colName))
                    {
                        colName = originalColName + idx;
                    }
                    columns.Add(colName);
                    select.Append($"{refAlias}.{f.CategoryFieldName} AS [{colName}]");


                    select.Append(",");
                }

                var refWhereCondition = new StringBuilder();
                if (refTable.Filter != null)
                {

                    suffix = refTable.Filter.FilterClauseProcess(refTable.RefTableCode, refAlias, refWhereCondition, sqlParams, suffix, false, null, data);
                }

                join.AppendLine($" LEFT JOIN v{refTable.RefTableCode} AS {refAlias} ON ({fromField} = [{refAlias}].{refTable.RefTableField})");
                if (refWhereCondition.Length > 0)
                {
                    join.Append($" AND {refWhereCondition}");
                }
            }
            return suffix;

        }


        private async Task<Dictionary<long, Dictionary<string, decimal>>> PeriodAdditionValues(int year, int month)
        {
            var employeeValues = await (
                from b in _organizationDBContext.SalaryPeriodAdditionBill
                join e in _organizationDBContext.SalaryPeriodAdditionBillEmployee on b.SalaryPeriodAdditionBillId equals e.SalaryPeriodAdditionBillId
                join v in _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue on e.SalaryPeriodAdditionBillEmployeeId equals v.SalaryPeriodAdditionBillEmployeeId
                join f in _organizationDBContext.SalaryPeriodAdditionField on v.SalaryPeriodAdditionFieldId equals f.SalaryPeriodAdditionFieldId
                where b.Year == year && b.Month == month
                select new
                {
                    e.EmployeeId,
                    f.FieldName,
                    v.Value,
                }).ToListAsync();

            return employeeValues.Where(v => v.Value.HasValue)
                .GroupBy(e => e.EmployeeId)
                .ToDictionary(
                e => e.Key,
                e => e.GroupBy(f => f.FieldName).ToDictionary(f => f.Key, f => f.Sum(v => v.Value.Value))
                );
        }
        private async Task NormalizeFieldNameInClause(Clause clause, IList<ProgramingFunctionBaseModel> sqls = null, long fromDate = 0, long toDate = 0, int month = 0, int year = 0)
        {
            if (clause is SingleClause single)
            {
                single.FieldName = EscaseFieldName(single.FieldName);

                if (sqls != null && sqls.Any(x => single.Value != null && single.Value.ToString().Contains(x.ProgramingFunctionName)))
                {
                    var paramsData = new NonCamelCaseDictionary();
                    paramsData.Add($"{SALARY_PARAM_PREFIX}FromDate", fromDate);
                    paramsData.Add($"{SALARY_PARAM_PREFIX}ToDate", toDate);
                    paramsData.Add($"{SALARY_PARAM_PREFIX}Month", month);
                    paramsData.Add($"{SALARY_PARAM_PREFIX}Year", year);
                    var sqlhandles = _organizationDBContext.EvaluateFunctionHandlerSql(sqls);
                    var value = EvalUtils.EvalObject(EscaseFieldName(single.Value.ToString()), paramsData, sqlhandles);
                    if (DateTime.TryParse(value.ToString(), out var time))
                    {
                        value = time.GetUnix();
                    }
                    single.Value = value;
                }

                single.Value = EscaseFieldName(single.Value);

            }
            else if (clause is ArrayClause arrClause && arrClause?.Rules?.Count > 0)
            {
                foreach (var c in arrClause.Rules)
                {
                    await NormalizeFieldNameInClause(c, sqls, fromDate, toDate, month, year);
                }
            }
        }



        private T EscaseFieldName<T>(T expression)
        {
            if (expression == null) return expression;
            if (expression.GetType() == typeof(string))
            {
                var expressionStr = expression.ToString().Replace("#", SALARY_FIELD_PREFIX).Replace("$.", "_");
                expressionStr = expressionStr.ToString().Replace("@", SALARY_PARAM_PREFIX);
                return (T)(expressionStr as object);
            }
            return expression;
        }

        private bool EvalClause(Clause clause, NonCamelCaseDictionary refValues, NCalc.EvaluateFunctionHandler functionHandler)
        {
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;

                    var value = singleClause.Value;

                    if (singleClause.Value?.GetType() == typeof(string) && !singleClause.Value.IsNullOrEmptyObject())
                    {
                        value = Regex.Replace(singleClause.Value?.ToString(), "\\{(?<ex>[^\\}]*)\\}", delegate (Match match)
                        {
                            var expression = match.Groups["ex"].Value;
                            return EvalUtils.EvalObject(expression, refValues, functionHandler)?.ToString();
                        });

                        var values = singleClause.Operator == EnumOperator.InList ? singleClause.Value.ToString().Split(',') : null;

                        if (values?.Length > 0)
                        {
                            var lst = new List<object>();
                            foreach (var v in values)
                            {
                                lst.Add(EvalUtils.EvalObject(v, refValues, functionHandler));
                            }

                            value = string.Join(",", lst.ToArray());
                        }
                        else
                        {
                            value = EvalUtils.EvalObject(singleClause.Value?.ToString(), refValues, functionHandler);
                        }
                    }




                    return EvalOperatorCompare(singleClause, refValues[singleClause.FieldName], value);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;

                    if (arrClause.Rules.Count == 0)
                    {
                        return true;
                    }

                    var res = new List<bool>();
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        res.Add(EvalClause(arrClause.Rules.ElementAt(indx), refValues, functionHandler));
                    }

                    var r = arrClause.Condition == EnumLogicOperator.Or ? res.Any(v => v) : res.All(v => v);
                    if (arrClause.Not) return !r;
                    return r;

                }
                else
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin lọc không sai định dạng");
                }
            }
            return true;
        }

        private bool IsEqualFieldsValue(EnumDataType dataTypeId, object value1, object value2)
        {
            if (dataTypeId.IsNumber())
            {
                if (GetDecimal(value1, null, out var decimalValue1) && GetDecimal(value2, null, out var decimalValue2))
                {
                    return decimalValue1 == decimalValue2;
                }
            }

            return value1?.ToString() == value2?.ToString();
        }
        private bool GetDecimal(object value, int? decimalPlace, out decimal decimalValue)
        {
            try
            {
                decimalValue = Convert.ToDecimal(value).RoundBy(decimalPlace ?? DEFAULT_DECIMAL_PLACE);
                return true;
            }
            catch (Exception e)
            {

                _logger.LogWarning(e, "Can not convert {0} to decimal", value);
                decimalValue = 0;
                return false;
            }
        }

        public object GetDefaultValue(EnumDataType dataTypeId, GroupSalaryEmployeeModel req)
        {
            object defaultValue;
            switch (dataTypeId)
            {
                case EnumDataType.Int:
                case EnumDataType.BigInt:
                case EnumDataType.Month:
                case EnumDataType.Year:
                case EnumDataType.Decimal:
                case EnumDataType.HBarRelative:
                case EnumDataType.Percentage:
                case EnumDataType.QuarterOfYear:
                    defaultValue = 0;
                    break;

                case EnumDataType.Boolean:
                    defaultValue = false;
                    break;

                case EnumDataType.Date:
                case EnumDataType.DateRange:
                    defaultValue = req.FromDate;
                    break;
                case EnumDataType.Text:
                case EnumDataType.Email:
                case EnumDataType.PhoneNumber:
                    defaultValue = req.FromDate;
                    break;
                default:
                    defaultValue = null;
                    break;

            }
            return defaultValue;
        }
        private bool EvalOperatorCompare(SingleClause clause, object x, object y)
        {
            if (x is DateTime)
            {
                x = ((DateTime)x).GetUnix();
            }
            if (y is DateTime)
            {
                y = ((DateTime)y).GetUnix();
            }
            switch (clause.Operator)
            {
                case EnumOperator.Equal:
                    return x?.ToString() == y?.ToString();
                case EnumOperator.NotEqual:

                    return x?.ToString() != y?.ToString();

                case EnumOperator.Contains:
                    if (x?.ToString() != y?.ToString()) return true;
                    return x?.ToString()?.Contains(y?.ToString()) == true;

                case EnumOperator.NotContains:
                    if (x.IsNullOrEmptyObject()) return true;
                    return x?.ToString()?.Contains(y?.ToString()) != true;
                case EnumOperator.InList:


                    IList<object> values = new List<object>();

                    var type = clause.Value.GetType();
                    if (type != typeof(string) && (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type)))
                    {
                        foreach (object v in (dynamic)y)
                        {
                            if (!values.Contains(v))
                                values.Add(v);
                        }
                    }

                    if (y is string str)
                    {
                        values = (str ?? "").Split(",").Distinct().Select(v => (object)v.Trim()).ToList();
                    }
                    return values.Contains(x);
                case EnumOperator.IsLeafNode:
                    throw new NotSupportedException();

                case EnumOperator.StartsWith:
                    if (x?.ToString() != y?.ToString()) return true;
                    return x?.ToString()?.StartsWith(y?.ToString()) == true;

                case EnumOperator.NotStartsWith:

                    return x?.ToString()?.StartsWith(y?.ToString()) != true;


                case EnumOperator.EndsWith:
                    if (x?.ToString() != y?.ToString()) return true;
                    return x?.ToString()?.EndsWith(y?.ToString()) == true;
                case EnumOperator.NotEndsWith:
                    return x?.ToString()?.EndsWith(y?.ToString()) != true;

                case EnumOperator.Greater:
                    if (clause.DataType.IsNumber())
                    {
                        return x.ToDecimal() > y.ToDecimal();
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return x.ToDecimal() > y.ToDecimal();
                    }
                    throw new NotSupportedException();

                case EnumOperator.GreaterOrEqual:
                    if (clause.DataType.IsNumber())
                    {
                        return x.ToDecimal() >= y.ToDecimal();
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return x.ToDecimal() >= y.ToDecimal();
                    }
                    throw new NotSupportedException();

                case EnumOperator.LessThan:
                    if (clause.DataType.IsNumber())
                    {
                        return x.ToDecimal() < y.ToDecimal();
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return x.ToDecimal() < y.ToDecimal();
                    }
                    throw new NotSupportedException();
                case EnumOperator.LessThanOrEqual:
                    if (clause.DataType.IsNumber())
                    {
                        return x.ToDecimal() <= y.ToDecimal();
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return x.ToDecimal() <= y.ToDecimal();
                    }
                    throw new NotSupportedException();
                case EnumOperator.IsNull:
                    return x == null;
                case EnumOperator.IsEmpty:
                    return x?.ToString() == "";

                case EnumOperator.IsNullOrEmpty:
                    return x.IsNullOrEmptyObject();

                default:
                    throw new NotSupportedException();
            }

        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(IList<string> fieldNames, IList<string> groupField, int salaryPeriodId, int salaryGroupId, IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> data)
        {
            var salaryEmployeeExport = new SalaryGroupEmployeeExportFacade(fieldNames, _salaryFieldService, this, _salaryGroupService);
            string titleFile = string.Empty;
            string titleName = string.Empty;
            var periodInfo = await _salaryPeriodService.GetInfo(salaryPeriodId);
            if (salaryGroupId != 0)
            {
                var nameGroup = (await _salaryGroupService.GetInfo(salaryGroupId)).Title;
                titleFile = $"Kỳ lương tháng {periodInfo.Month}/{periodInfo.Year}- {nameGroup}";
                titleName = $"Kỳ lương tháng {periodInfo.Month}/{periodInfo.Year}- {nameGroup}";
            }
            else
            {
                titleFile = "Tổng quan về lương";
                titleName = $"Tổng quan về lương cho kỳ lương tháng {periodInfo.Month}/{periodInfo.Year}";
            }

            return await salaryEmployeeExport.Export(data, groupField, titleFile, titleName, salaryGroupId);
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping(int salaryGroupId)
        {
            var fieldDatas = await _salaryFieldService.GetList();
            IList<CategoryFieldNameModel> categoryNameModels = new List<CategoryFieldNameModel>();
            if (salaryGroupId != 0)
            {
                var groupFieldData = await _salaryGroupService.GetInfo(salaryGroupId);
                foreach (var field in groupFieldData.TableFields)
                {
                    if (field.IsHidden) continue;
                    var fieldData = fieldDatas.FirstOrDefault(x => x.SalaryFieldId == field.SalaryFieldId);
                    categoryNameModels.Add(new CategoryFieldNameModel()
                    {
                        DataTypeId = fieldData?.DataTypeId,
                        FieldName = fieldData?.SalaryFieldName,
                        FieldTitle = fieldData?.Title,
                        GroupName = !string.IsNullOrEmpty(field?.GroupName) ? field.GroupName : "",
                        SortOrder = field?.SortOrder,
                    });
                }
            }
            else
            {
                foreach (var field in fieldDatas)
                {
                    categoryNameModels.Add(new CategoryFieldNameModel()
                    {
                        DataTypeId = field?.DataTypeId,
                        FieldName = field?.SalaryFieldName,
                        FieldTitle = field?.Title,
                        GroupName = !string.IsNullOrEmpty(field?.GroupName) ? field.GroupName : "",
                        SortOrder = field?.SortOrder,
                    });
                }
            }
            return new CategoryNameModel()
            {
                CategoryCode = "Salary",
                CategoryTitle = "Bảng lương",
                Fields = categoryNameModels,
            };
        }


    }
}
