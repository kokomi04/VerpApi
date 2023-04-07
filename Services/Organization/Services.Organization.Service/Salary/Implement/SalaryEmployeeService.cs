using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
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

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryEmployeeService : ISalaryEmployeeService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryRefTableActivityLog;
        private readonly ISalaryGroupService _salaryGroupService;
        private readonly ISalaryRefTableService _salaryRefTableService;
        private readonly IHrDataService _hrDataService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ISalaryFieldService _salaryFieldService;
        private readonly ISalaryPeriodService _salaryPeriodService;
        private readonly ISalaryPeriodGroupService _salaryPeriodGroupService;

        public SalaryEmployeeService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService, ISalaryGroupService salaryGroupService, ISalaryRefTableService salaryRefTableService, IHrDataService hrDataService, ICategoryHelperService httpCategoryHelperService, ISalaryFieldService salaryFieldService, ISalaryPeriodService salaryPeriodService, ISalaryPeriodGroupService salaryPeriodGroupService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryRefTableActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryEmployee);
            _salaryGroupService = salaryGroupService;
            _salaryRefTableService = salaryRefTableService;
            _hrDataService = hrDataService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _salaryFieldService = salaryFieldService;
            _salaryPeriodService = salaryPeriodService;
            _salaryPeriodGroupService = salaryPeriodGroupService;
        }

        public async Task<IList<NonCamelCaseDictionary>> EvalSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeRequestModel req)
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

            var lst = await FilterEmployee(groupInfo.EmployeeFilter, period.Year, period.Month, req.FromDate, req.ToDate);

            var salaryFields = await _salaryFieldService.GetList();

            var result = new List<NonCamelCaseDictionary>();
            foreach (var item in lst)
            {
                var model = new NonCamelCaseDictionary();
                result.Add(model);
                foreach (var column in item)
                {
                    model.Add(column.Key, column.Value);
                }

                model.Add("@FromDate", req.FromDate);
                model.Add("@ToDate", req.ToDate);
                model.Add("@Month", period.Month);
                model.Add("@Year", period.Year);

                foreach (var f in salaryFields)
                {
                    foreach (var condition in f.Expression)
                    {
                        var filter = condition?.Filter;
                        var conditionResult = EvalClause(filter, model);
                        if (conditionResult)
                        {
                            var value = EvalUtils.EvalObject(condition.ValueExpression, model);
                            if (!model.ContainsKey(f.SalaryFieldName))
                            {
                                model.Add(f.SalaryFieldName, value);
                            }
                        }
                    }
                }
            }

            return result;
        }


        public async Task<IList<NonCamelCaseDictionary>> GetSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId)
        {
            var group = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);
            if (group == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var salaryData = await _organizationDBContext.SalaryEmployee
                .Include(s => s.SalaryEmployeeValue)
                .ThenInclude(v => v.SalaryField)
                .Where(s => s.SalaryPeriodId == salaryPeriodId && s.SalaryGroupId == salaryGroupId)
                .ToListAsync();
            var result = new List<NonCamelCaseDictionary>();

            var salaryFields = await _salaryFieldService.GetList();

            foreach (var item in salaryData)
            {
                var model = new NonCamelCaseDictionary();
                result.Add(model);

                foreach (var v in item.SalaryEmployeeValue)
                {
                    model.Add(v.SalaryField.SalaryFieldName, v.Value);
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

            var group = await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);
            if (group == null)
            {
                await _salaryPeriodGroupService.Create(new SalaryPeriodGroupModel()
                {
                    SalaryPeriodId = salaryPeriodId,
                    SalaryGroupId = salaryGroupId,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate
                });
            }
            else
            {
                await _salaryPeriodGroupService.Update(group.SalaryPeriodGroupId, new SalaryPeriodGroupModel()
                {
                    SalaryPeriodId = salaryPeriodId,
                    SalaryGroupId = salaryGroupId,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate
                });
            }

            var salaryFields = await _salaryFieldService.GetList();

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                var salaries = new Dictionary<SalaryEmployee, NonCamelCaseDictionary>();

                var lst = new List<SalaryEmployee>();
                foreach (var item in model.Salaries)
                {
                    long employeeId = 0;
                    if (item.ContainsKey(OrganizationConstants.HR_TABLE_F_IDENTITY))
                    {
                        employeeId = Convert.ToInt64(item[OrganizationConstants.HR_TABLE_F_IDENTITY]);
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
                foreach (var item in lst)
                {
                    var data = salaries[item];
                    foreach (var field in salaryFields)
                    {
                        if (data.ContainsKey(field.SalaryFieldName))
                        {
                            var value = data[field.SalaryFieldName];
                            if (!value.IsNullOrEmptyObject())
                            {
                                salaryData.Add(new SalaryEmployeeValue()
                                {
                                    SalaryEmployeeId = item.SalaryEmployeeId,
                                    SalaryFieldId = field.SalaryFieldId,
                                    Value = value
                                });
                            }
                        }

                    }
                }

                await _organizationDBContext.InsertByBatch(salaryData, false, false);

                await trans.CommitAsync();

                return true;
            }
        }

        private async Task<IList<NonCamelCaseDictionary>> FilterEmployee(Clause filter, int year, int month, long fromDate, long toDate)
        {
            var refTables = await _salaryRefTableService.GetList();
            var (query, fieldNames) = await _hrDataService.BuildHrQuery("CTNS_Ho_So");

            var select = new StringBuilder();
            var join = new StringBuilder($"({query}) v");

            foreach (var f in fieldNames)
            {
                select.Append($"v.{f},");
            }

            var refFields = await _httpCategoryHelperService.GetReferFields(refTables.Select(c => c.RefTableCode).ToList(), null);

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

                var refAlias = $"[v{fromField}]";

                if (lastPoint < 0)
                {
                    fromField = $"v.[{fromField}]";
                }
                else
                {
                    fromField = $"v{fromField}";
                }

                var cateFields = refFields.Where(f => f.CategoryCode == refTable.RefTableCode).ToList();

                foreach (var f in cateFields)
                {
                    select.Append($"[{refAlias}].{f.CategoryFieldName} AS [{refTable.FromField}_{f.CategoryFieldName}],");
                }

                var refWhereCondition = new StringBuilder();
                if (refTable.Filter != null)
                {

                    filter.FilterClauseProcess(refTable.RefTableCode, refAlias, ref refWhereCondition, ref sqlParams, ref suffix);
                }

                join.AppendLine($"LEFT JOIN {refTable.RefTableCode} AS {refAlias} ON ({fromField} = [{refAlias}].{refTable.RefTableField})");
                if (refWhereCondition.Length > 0)
                {
                    join.Append($" AND {refWhereCondition}");
                }
            }

            var whereCondition = new StringBuilder();


            if (filter != null)
            {

                filter.FilterClauseProcess("({query}) vm", "v", ref whereCondition, ref sqlParams, ref suffix);
            }
            var queryData = $"SELECT {select.ToString().TrimEnd(',')} FROM {join}" + (whereCondition.Length > 0 ? "WHERE " : " ") + whereCondition;
            var lstData = await _organizationDBContext.QueryDataTable(queryData, sqlParams.ToArray());

            return lstData.ConvertData();

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
                    }


                    return EvalOperatorCompare(singleClause, refValues[singleClause.FieldName], singleClause.Value);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;

                    if (arrClause.Rules.Count == 0)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin trong mảng điều kiện không được để trống.Vui lòng kiểm tra lại cấu hình điều kiện lọc!");
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
                        return (decimal)x > (decimal)y;
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return (DateTime)x > (DateTime)y;
                    }
                    throw new NotSupportedException();

                case EnumOperator.GreaterOrEqual:
                    if (clause.DataType.IsNumber())
                    {
                        return (decimal)x >= (decimal)y;
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return (DateTime)x >= (DateTime)y;
                    }
                    throw new NotSupportedException();

                case EnumOperator.LessThan:
                    if (clause.DataType.IsNumber())
                    {
                        return (decimal)x < (decimal)y;
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return (DateTime)x < (DateTime)y;
                    }
                    throw new NotSupportedException();
                case EnumOperator.LessThanOrEqual:
                    if (clause.DataType.IsNumber())
                    {
                        return (decimal)x <= (decimal)y;
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.DateRange }.Contains(clause.DataType))
                    {
                        return (DateTime)x <= (DateTime)y;
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
