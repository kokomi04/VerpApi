using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using NPOI.Util;
using OpenXmlPowerTools;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
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
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.Salary.Implement.Abstract;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryEmployeeService : SalaryPeriodGroupEmployeeAbstract, ISalaryEmployeeService
    {
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryEmployeeActivityLog;
        private readonly ObjectActivityLogFacade _salaryPeriodGroupActivityLog;

        private readonly ISalaryGroupService _salaryGroupService;
        private readonly ISalaryRefTableService _salaryRefTableService;
        private readonly IHrDataService _hrDataService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ISalaryFieldService _salaryFieldService;
        private readonly ISalaryPeriodService _salaryPeriodService;
        private readonly ISalaryPeriodGroupService _salaryPeriodGroupService;

        private const string SALARY_FIELD_PREFIX = "__";

        private const int DEFAULT_DECIMAL_PLACE = 2;

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
            ILogger<SalaryEmployeeService> logger)
            : base(organizationDBContext, currentContextService, logger)
        {
            _mapper = mapper;
            _salaryEmployeeActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryEmployee);
            _salaryPeriodGroupActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodGroup);

            _salaryGroupService = salaryGroupService;
            _salaryRefTableService = salaryRefTableService;
            _hrDataService = hrDataService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _salaryFieldService = salaryFieldService;
            _salaryPeriodService = salaryPeriodService;
            _salaryPeriodGroupService = salaryPeriodGroupService;
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

            var (lst, columns) = await FilterEmployee(groupInfo.EmployeeFilter, period.Year, period.Month, req.FromDate, req.ToDate);

            var salaryFields = await _salaryFieldService.GetList();
            salaryFields = SortFieldNameByReference(salaryFields);
            var result = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

            var groupFields = groupInfo.TableFields.ToDictionary(t => t.SalaryFieldId, t => t);

            if (req.Salaries == null)
            {
                req.Salaries = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();
            }

            var evalDataByEmployee = req.Salaries.ToDictionary(item =>
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                }
                return employeeId;
            }, item => item);

            var employeeInOtherGroups = (await
                    _organizationDBContext.SalaryEmployee
                    .Where(e => e.SalaryPeriodId == salaryPeriodId && e.SalaryGroupId != salaryGroupId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync()
                ).Distinct()
                .ToHashSet();

            var employeeIds = new HashSet<long>();
            foreach (var item in lst)
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
                if (employeeInOtherGroups.Contains(employeeId)) continue;

                employeeIds.Add(employeeId);

                var paramsData = new NonCamelCaseDictionary();

                foreach (var valuePaire in item)
                {
                    paramsData.Add(valuePaire.Key, valuePaire.Value);
                }

                paramsData.Add("@FromDate", req.FromDate);
                paramsData.Add("@ToDate", req.ToDate);
                paramsData.Add("@Month", period.Month);
                paramsData.Add("@Year", period.Year);


                var model = new NonCamelCaseDictionary<SalaryEmployeeValueModel>
                {
                    { OrganizationConstants.HR_TABLE_F_IDENTITY, new SalaryEmployeeValueModel(employeeId) }
                };

                result.Add(model);

                evalDataByEmployee.TryGetValue(employeeId, out var evalItem);

                foreach (var f in salaryFields)
                {
                    var fieldVariableName = SALARY_FIELD_PREFIX + f.SalaryFieldName;

                    var isOverride = groupFields.TryGetValue(f.SalaryFieldId, out var groupField);
                    var fieldIsEditable = f.IsEditable && (!isOverride || groupField.IsEditable);
                    SalaryEmployeeValueModel evalValue = null;
                    var evalDataIsEdited = evalItem != null && evalItem.TryGetValue(f.SalaryFieldName, out evalValue) && evalValue?.IsEdited == true;
                    if (fieldIsEditable && evalDataIsEdited)
                    {
                        paramsData.Add(fieldVariableName, evalValue.Value);
                        model.Add(f.SalaryFieldName, evalValue);
                    }
                    else
                    {
                        object fieldValue = null;
                        foreach (var condition in f.Expression)
                        {
                            var (isSucess, value) = EvalValueExpression(f, condition, paramsData);
                            if (isSucess)
                            {
                                fieldValue = value;
                            }

                        }



                        if (fieldValue == null)
                        {
                            paramsData.Add(fieldVariableName, GetDefaultValue(f.DataTypeId, req));
                        }
                        else
                        {
                            try
                            {
                                if (f.DataTypeId.IsNumber())
                                {
                                    fieldValue = Convert.ToDecimal(fieldValue).RoundBy(DEFAULT_DECIMAL_PLACE);
                                }
                            }
                            catch (Exception e)
                            {

                                _logger.LogError(e, "Can not convert {0} to decimal", fieldValue);
                            }

                            paramsData.Add(fieldVariableName, fieldValue);
                        }

                        model.Add(f.SalaryFieldName, new SalaryEmployeeValueModel(fieldValue));
                    }

                }
            }

            return result;
        }


        private (bool isSucess, object value) EvalValueExpression(SalaryFieldModel field, SalaryFieldExpressionModel condition, NonCamelCaseDictionary paramsData)
        {
            var filter = condition?.Filter;
            NormalizeFieldNameInClause(filter);
            bool conditionResult;
            try
            {
                conditionResult = EvalClause(filter, paramsData);
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
                    var value = EvalUtils.EvalObject(EscaseFieldName(condition.ValueExpression), paramsData);
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

        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId)
        {
            var group = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);
            if (group == null)
            {
                return new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();
            }

            var salaryData = await _organizationDBContext.SalaryEmployee
                .Include(s => s.SalaryEmployeeValue)
                .ThenInclude(v => v.SalaryField)
                .Where(s => s.SalaryPeriodId == salaryPeriodId && s.SalaryGroupId == salaryGroupId)
                .ToListAsync();
            var result = new List<NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

            var salaryFields = await _salaryFieldService.GetList();

            foreach (var item in salaryData)
            {
                var model = new NonCamelCaseDictionary<SalaryEmployeeValueModel>();
                result.Add(model);

                model.Add(OrganizationConstants.HR_TABLE_F_IDENTITY, new SalaryEmployeeValueModel(item.EmployeeId));

                foreach (var v in item.SalaryEmployeeValue)
                {
                    model.Add(v.SalaryField.SalaryFieldName, new SalaryEmployeeValueModel(v.Value, v.IsEdited));
                }
            }

            return result;
        }

        public async Task<bool> Update(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel model)
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

            var periodGroup = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);


            var salaryFields = await _salaryFieldService.GetList();

            var evalData = await EvalSalaryEmployeeByGroup(salaryPeriodId, salaryGroupId, model);

            var evalDataByEmployee = evalData.ToDictionary(item =>
            {
                long employeeId = 0;
                if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                {
                    employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                }
                return employeeId;
            }, item => item);

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
                    await _salaryPeriodGroupService.DbUpdate(periodGroup.SalaryPeriodGroupId, new SalaryPeriodGroupModel()
                    {
                        SalaryPeriodId = salaryPeriodId,
                        SalaryGroupId = salaryGroupId,
                        FromDate = model.FromDate,
                        ToDate = model.ToDate
                    }, true);
                    salaryPeriodGroupId = periodGroup.SalaryPeriodGroupId;
                }

                await DeleteSalaryEmployeeByPeriodGroup(salaryPeriodId, salaryGroupId);

                var salaries = new Dictionary<SalaryEmployee, NonCamelCaseDictionary<SalaryEmployeeValueModel>>();

                var lst = new List<SalaryEmployee>();
                foreach (var item in model.Salaries)
                {
                    long employeeId = 0;
                    if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                    {
                        employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]?.Value);
                    }


                    var entity = new SalaryEmployee()
                    {
                        EmployeeId = employeeId,
                        SalaryPeriodId = salaryPeriodId,
                        SalaryGroupId = salaryGroupId,
                    };
                    salaries.Add(entity, item);
                    lst.Add(entity);
                }

                await _organizationDBContext.InsertByBatch(lst, true, true);


                var salaryData = new List<SalaryEmployeeValue>();

                var groupFields = groupInfo.TableFields.ToDictionary(t => t.SalaryFieldId, t => t);

                foreach (var item in lst)
                {

                    if (!evalDataByEmployee.TryGetValue(item.EmployeeId, out var evalItem))
                    {
                        throw SalaryPeriodValidationMessage.EmployeeNotFoundInEval.BadRequestFormat(item.EmployeeId);
                    }

                    var data = salaries[item];
                    foreach (var field in salaryFields)
                    {
                        var hasDataValue = data.TryGetValue(field.SalaryFieldName, out var dataValue);

                        if (!field.IsEditable || groupFields.TryGetValue(field.SalaryFieldId, out var groupField) && !groupField.IsEditable)
                        {
                            SalaryEmployeeValueModel evalValue = null;
                            if (evalItem != null)
                            {
                                evalItem.TryGetValue(field.SalaryFieldName, out evalValue);
                            }


                            if (dataValue.IsEdited || evalValue.IsEdited || dataValue?.Value?.ToString() != evalValue?.Value?.ToString())
                            {
                                throw SalaryPeriodValidationMessage.FieldIsNotEditable.BadRequestFormat(field.GroupName + " > " + field.Title, dataValue, evalValue);
                            }
                        }

                        if (hasDataValue)
                        {
                            if (!dataValue.IsNullOrEmptyObject())
                            {
                                salaryData.Add(new SalaryEmployeeValue()
                                {
                                    SalaryEmployeeId = item.SalaryEmployeeId,
                                    SalaryFieldId = field.SalaryFieldId,
                                    IsEdited = dataValue.IsEdited,
                                    Value = dataValue.Value
                                });
                            }
                        }

                    }
                }

                await _organizationDBContext.InsertByBatch(salaryData, false, false);


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
                   .JsonData(model.JsonSerialize())
                   .CreateLog();

                return true;
            }
        }



        private async Task<(IList<NonCamelCaseDictionary> data, IList<string> columns)> FilterEmployee(Clause filter, int year, int month, long fromDate, long toDate)
        {
            var columns = new List<string>();

            var refTables = await _salaryRefTableService.GetList();
            var (query, fieldNames) = await _hrDataService.BuildHrQuery("CTNS_Ho_So", false);

            var select = new StringBuilder();
            var join = new StringBuilder($"({query}) v");

            foreach (var f in fieldNames)
            {
                columns.Add(f);
                select.Append($"v.{f}");
                select.Append(",");
            }

            var refFields = await _httpCategoryHelperService.GetReferFields(refTables.Select(c => c.RefTableCode).ToList(), null);

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

                    refTable.Filter.FilterClauseProcess(refTable.RefTableCode, refAlias, ref refWhereCondition, ref sqlParams, ref suffix, false, null, data);
                }

                join.AppendLine($" LEFT JOIN v{refTable.RefTableCode} AS {refAlias} ON ({fromField} = [{refAlias}].{refTable.RefTableField})");
                if (refWhereCondition.Length > 0)
                {
                    join.Append($" AND {refWhereCondition}");
                }
            }

            var whereCondition = new StringBuilder();


            if (filter != null)
            {
                NormalizeFieldNameInClause(filter);

                filter.FilterClauseProcess($"({query}) vm", "v", ref whereCondition, ref sqlParams, ref suffix, false, null, data);
            }
            var queryData = $"SELECT * FROM (SELECT {select.ToString().TrimEnd().TrimEnd(',')} FROM {join}) v " + (whereCondition.Length > 0 ? "WHERE " : " ") + whereCondition;
            var lstData = await _organizationDBContext.QueryDataTable(queryData, sqlParams.ToArray());

            return (lstData.ConvertData(), columns);

        }

        private void NormalizeFieldNameInClause(Clause clause)
        {
            if (clause is SingleClause single)
            {
                single.FieldName = EscaseFieldName(single.FieldName);
                single.Value = EscaseFieldName(single.Value);

            }
            else if (clause is ArrayClause arrClause && arrClause?.Rules?.Count > 0)
            {
                foreach (var c in arrClause.Rules)
                {
                    NormalizeFieldNameInClause(c);
                }
            }
        }



        private T EscaseFieldName<T>(T expression)
        {
            if (expression == null) return expression;
            if (expression.GetType() == typeof(string))
            {
                var expressionStr = expression.ToString().Replace("#", SALARY_FIELD_PREFIX).Replace("$.", "_");

                return (T)(expressionStr as object);
            }
            return expression;
        }


        private IList<SalaryFieldModel> SortFieldNameByReference(IList<SalaryFieldModel> fields)
        {
            var sortedFields = new List<SalaryFieldModel>();

            foreach (var field in fields)
            {
                var stack = new Stack<SalaryFieldModel>();
                stack.Push(field);
                while (stack.Count > 0)
                {
                    SalaryFieldModel currentField = stack.Pop();
                    var children = fields.Where(f => f != currentField && ContainRefField(currentField, "#" + f.SalaryFieldName)).ToList();
                    if (children.Count == 0 || children.All(c => sortedFields.Contains(c)))
                    {
                        if (!sortedFields.Contains(currentField))
                        {
                            sortedFields.Add(currentField);
                        }
                    }
                    else
                    {
                        stack.Push(currentField);
                        foreach (var c in children)
                        {
                            if (!stack.Contains(c))
                                stack.Push(c);
                        }
                    }
                }

            }

            return sortedFields;

        }

        private bool ContainRefField(SalaryFieldModel expression, string fieldName)
        {
            if (expression.Expression == null || expression.Expression.Count == 0) return false;
            return expression.Expression.Any(e => ContainRefField(e, fieldName));
        }

        private bool ContainRefField(SalaryFieldExpressionModel expression, string fieldName)
        {
            return ContainRefField(expression.Filter, fieldName) || ContainVarible(expression.ValueExpression, fieldName);
        }

        private bool ContainRefField(Clause clause, string fieldName)
        {
            if (clause == null) return false;
            if (clause is SingleClause single)
            {
                if (ContainVarible(single.FieldName, fieldName)) return true;
                if (ContainVarible(single.Value?.ToString(), fieldName)) return true;
                return false;
            }
            else
            {
                var arrClause = clause as ArrayClause;
                if (arrClause == null || arrClause.Rules == null || arrClause.Rules.Count == 0) return false;
                return arrClause.Rules.Any(r => ContainRefField(r, fieldName));
            }
        }


        private bool ContainVarible(string str, string childString)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            var _regContainVariable = new Regex($".*(^|[^a-zA-Z0-9_]){childString}([^a-zA-Z0-9_]|$).*");
            return _regContainVariable.IsMatch(str);
        }

        private bool EvalClause(Clause clause, NonCamelCaseDictionary refValues = null)
        {
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;


                    if (singleClause.Value?.GetType() == typeof(string) && !singleClause.Value.IsNullOrEmptyObject())
                    {
                        singleClause.Value = Regex.Replace(singleClause.Value?.ToString(), "\\{(?<ex>[^\\}]*)\\}", delegate (Match match)
                        {
                            var expression = match.Groups["ex"].Value;
                            return EvalUtils.EvalObject(expression, refValues)?.ToString();
                        });

                        singleClause.Value = EvalUtils.EvalObject(singleClause.Value?.ToString(), refValues);

                    }




                    return EvalOperatorCompare(singleClause, refValues[singleClause.FieldName], singleClause.Value);
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
                        res.Add(EvalClause(arrClause.Rules.ElementAt(indx), refValues));
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
        public object GetDefaultValue(EnumDataType dataTypeId, GroupSalaryEmployeeModel req)
        {
            object defaultValue = null;
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
                    if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
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
    }
}
