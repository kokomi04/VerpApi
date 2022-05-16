using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VErp.Commons.Enums.AccountantEnum;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using System.Data;
using VErp.Infrastructure.EF.EFExtensions;
using Verp.Cache.RedisCache;
using VErp.Commons.Library;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Accountancy.Model.Data;
using VErp.Commons.Constants;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Accountancy;
using Verp.Resources.Accountancy.InputData;
using static Verp.Resources.Accountancy.InputData.InputDataValidationMessage;
using static VErp.Commons.Library.ExcelReader;
using Verp.Resources.GlobalObject;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputDataService : IInputDataService
    {
        private const string INPUTVALUEROW_TABLE = AccountantConstants.INPUTVALUEROW_TABLE;
        private const string INPUTVALUEROW_VIEW = AccountantConstants.INPUTVALUEROW_VIEW;

        private readonly ILogger _logger;
        //private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly IInputConfigService _inputConfigService;
        private readonly ObjectActivityLogFacade _inputDataActivityLog;

        public InputDataService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , ICategoryHelperService httpCategoryHelperService
            , IOutsideMappingHelperService outsideMappingHelperService
            , IInputConfigService inputConfigService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            //_activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _inputConfigService = inputConfigService;
            _inputDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InputBill);
        }

        public async Task<PageDataTable> GetBills(int inputTypeId, bool isMultirow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var viewInfo = await _accountancyDBContext.InputTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var inputTypeViewId = viewInfo?.InputTypeViewId;

            var fields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                select new { a.InputAreaId, af.InputAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.FormTypeId, f.DataTypeId, a.IsMultiRow, a.IsAddition }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);

            var viewFields = await (
                from f in _accountancyDBContext.InputTypeViewField
                where f.InputTypeViewId == inputTypeViewId
                select f
            ).ToListAsync();

            var sqlParams = new List<SqlParameter>();

            var whereCondition = new StringBuilder();

            whereCondition.Append($"r.InputTypeId = {inputTypeId} AND {GlobalFilter()}");
            if (fromDate.HasValue && toDate.HasValue)
            {
                whereCondition.Append($" AND r.{AccountantConstants.BILL_DATE} BETWEEN @FromDate AND @ToDate");

                sqlParams.Add(new SqlParameter("@FromDate", EnumDataType.Date.GetSqlValue(fromDate.Value)));
                sqlParams.Add(new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate.Value)));
            }


            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.InputTypeViewFieldId == filter.Key);
                    if (viewField == null) continue;

                    var value = filter.Value;

                    if (value.IsNullObject()) continue;

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)viewField.DataTypeId))
                    {
                        value = Convert.ToInt64(value);
                    }

                    if (!string.IsNullOrEmpty(viewField.SelectFilters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(viewField.SelectFilters);
                        if (filterClause != null)
                        {
                            if (whereCondition.Length > 0)
                            {
                                whereCondition.Append(" AND ");
                            }

                            filterClause.FilterClauseProcess(INPUTVALUEROW_VIEW, "r", ref whereCondition, ref sqlParams, ref suffix, false, value);
                        }
                    }
                }
            }

            if (columnsFilters != null)
            {
                if (whereCondition.Length > 0)
                {
                    whereCondition.Append(" AND ");
                }

                columnsFilters.FilterClauseProcess(INPUTVALUEROW_VIEW, "r", ref whereCondition, ref sqlParams, ref suffix);
            }


            string totalSql;

            var fieldToSelect = fields.Values.ToList();

            if (isMultirow)
            {
                totalSql = @$"SELECT COUNT(0) as Total FROM {INPUTVALUEROW_VIEW} r WHERE {whereCondition}";
            }
            else
            {
                totalSql = @$"SELECT COUNT(DISTINCT r.InputBill_F_Id) as Total FROM {INPUTVALUEROW_VIEW} r WHERE {whereCondition}";
                fieldToSelect = fieldToSelect.Where(f => !f.IsMultiRow).ToList();
            }


            var selectColumns = fieldToSelect.SelectMany(f =>
            {
                var refColumns = new List<string>()
                {
                    f.FieldName
                };

                if (((EnumFormType)f.FormTypeId).IsJoinForm()
                && !string.IsNullOrWhiteSpace(f.RefTableTitle)
                && !string.IsNullOrWhiteSpace(f.RefTableTitle))
                {
                    refColumns.AddRange(f.RefTableTitle.Split(',').Select(c => f.FieldName + "_" + c.Trim()));
                }
                return refColumns;
            }).ToList();

            if (!selectColumns.Contains(orderByFieldName))
            {
                orderByFieldName = selectColumns.Contains("ngay_ct") ? "ngay_ct" : "F_Id";
                asc = false;
            }


            var table = await _accountancyDBContext.QueryDataTable(totalSql, sqlParams.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            var selectColumn = string.Join(",", selectColumns.Select(c => $"r.[{c}]"));

            string dataSql;
            if (isMultirow)
            {
                dataSql = @$"
                 
                    SELECT r.InputBill_F_Id, r.F_Id {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                    FROM {INPUTVALUEROW_VIEW} r
                    WHERE {whereCondition}
               
                    ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }
            else
            {
                dataSql = @$"
                 ;WITH tmp AS (
                    SELECT r.InputBill_F_Id, MAX(F_Id) as F_Id
                    FROM {INPUTVALUEROW_VIEW} r
                    WHERE {whereCondition}
                    GROUP BY r.InputBill_F_Id    
                )
                SELECT 
                    t.InputBill_F_Id AS F_Id
                    {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                FROM tmp t JOIN {INPUTVALUEROW_VIEW} r ON t.F_Id = r.F_Id
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }

            if (size >= 0)
            {
                dataSql += @$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY";
            }

            var data = await _accountancyDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return (data, total);
        }

        public async Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size)
        {
            var singleFields = (await (
               from af in _accountancyDBContext.InputAreaField
               join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
               join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
               where af.InputTypeId == inputTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var totalSql = @$"SELECT COUNT(0) as Total FROM {INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0";

            var table = await _accountancyDBContext.QueryDataTable(totalSql, new SqlParameter[0]);

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (string.IsNullOrWhiteSpace(orderByFieldName))
            {
                orderByFieldName = "CreatedDatetimeUtc";
            }

            var dataSql = @$"

                SELECT     r.*
                FROM {INPUTVALUEROW_VIEW} r 

                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0

                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
            ";

            if (size > 0)
            {
                dataSql += @$"
                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
            ";
            }
            var data = await _accountancyDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM { INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _accountancyDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

            if (billEntryInfo.Rows.Count > 0)
            {
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    var row = data.Rows[i];
                    for (var j = 0; j < data.Columns.Count; j++)
                    {
                        var column = data.Columns[j];
                        if (singleFields.Contains(column.ColumnName))
                        {
                            row[column] = billEntryInfo.Rows[0][column.ColumnName];
                        }
                    }
                }
            }


            return (data, total);
        }

        public async Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId)
        {
            var singleFields = (await (
               from af in _accountancyDBContext.InputAreaField
               join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
               join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
               where af.InputTypeId == inputTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var result = new BillInfoModel();

            var dataSql = @$"

                SELECT     r.*
                FROM {INPUTVALUEROW_VIEW} r 

                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM { INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _accountancyDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

            result.Info = billEntryInfo.ConvertFirstRowData().ToNonCamelCaseDictionary();

            if (billEntryInfo.Rows.Count > 0)
            {
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    var row = data.Rows[i];
                    for (var j = 0; j < data.Columns.Count; j++)
                    {
                        var column = data.Columns[j];
                        if (singleFields.Contains(column.ColumnName))
                        {
                            row[column] = billEntryInfo.Rows[0][column.ColumnName];
                        }
                    }
                }
            }
            else
            {
                result.Info = data.ConvertFirstRowData().ToNonCamelCaseDictionary();
            }

            result.Rows = data.ConvertData();

            return result;
        }


        public async Task<long> CreateBill(int inputTypeId, BillInfoModel data)
        {
            await ValidateAccountantConfig(data?.Info, null);

            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0)
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            // Lấy thông tin field
            var inputAreaFields = await GetInputFields(inputTypeId);
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, null, null);

            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            checkRows = data.Rows.Select(r => new ValidateRowModel(r, null, null)).ToList();

            // Validate info
            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = inputAreaFields.Where(f => (!f.IsAutoIncrement && f.IsUnique) || f.FieldName == AccountantConstants.BILL_CODE).ToList();
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, inputAreaFields);
            // Check refer
            await CheckReferAsync(inputAreaFields, checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkInfo, checkRows, uniqueFields);
            // Check value
            CheckValue(checkInfo, checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get all fields
                var inputFields = _accountancyDBContext.InputField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, data, inputFields, EnumActionType.Add);

                if (result.Code != 0)
                {
                    if (string.IsNullOrWhiteSpace(result.Message))
                        throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                    else
                    {
                        throw result.Message.BadRequest();

                    }
                }

                var billInfo = new InputBill()
                {
                    InputTypeId = inputTypeId,
                    LatestBillVersion = 1,
                    SubsidiaryId = _currentContextService.SubsidiaryId,
                    IsDeleted = false
                };
                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                await _accountancyDBContext.SaveChangesAsync();

                var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                await CreateBillVersion(inputTypeId, billInfo, data, generateTypeLastValues);

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Add);

                if (!string.IsNullOrWhiteSpace(data?.OutsideImportMappingData?.MappingFunctionKey))
                {
                    await _outsideMappingHelperService.MappingObjectCreate(data.OutsideImportMappingData.MappingFunctionKey, data.OutsideImportMappingData.ObjectId, EnumObjectType.InputBill, billInfo.FId);
                }

                await ConfirmCustomGenCode(generateTypeLastValues);

                trans.Commit();


                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Create)
                .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                .BillTypeId(inputTypeId)
                .ObjectId(billInfo.FId)
                .JsonData(data.JsonSerialize())
                .CreateLog();


                return billInfo.FId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateBill");
                throw;
            }
        }

        private async Task<bool> CheckRequireFilter(Clause clause, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> inputAreaFields, Dictionary<string, Dictionary<object, object>> sfValues, int? rowIndex, bool not = false)
        {
            bool? isRequire = null;
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    var field = inputAreaFields.First(f => f.FieldName == singleClause.FieldName);
                    // Data check nằm trong thông tin chung và data điều kiện nằm trong thông tin chi tiết
                    if (!rowIndex.HasValue && field.IsMultiRow)
                    {
                        var rowValues = rows.Select(r => r.Data.ContainsKey(field.FieldName)
                        ? sfValues.ContainsKey(field.FieldName)
                        ? sfValues[field.FieldName].ContainsKey(r.Data[field.FieldName])
                        ? sfValues[field.FieldName][r.Data[field.FieldName]]
                        : null
                        : r.Data[field.FieldName]
                        : null).ToList();
                        switch (singleClause.Operator)
                        {
                            case EnumOperator.Equal:
                                isRequire = rowValues.Any(v => v == singleClause.Value);
                                break;
                            case EnumOperator.NotEqual:
                                isRequire = rowValues.Any(v => v != singleClause.Value);
                                break;
                            case EnumOperator.Contains:
                                isRequire = rowValues.Any(v => v.StringContains(singleClause.Value));
                                break;
                            case EnumOperator.NotContains:
                                isRequire = rowValues.All(v => !v.StringContains(singleClause.Value));
                                break;
                            case EnumOperator.InList:
                                var arrValues = singleClause.Value.ToString().Split(",");
                                isRequire = rowValues.Any(v => v != null && arrValues.Contains(v.ToString()));
                                break;
                            case EnumOperator.IsLeafNode:
                                // Check is leaf node
                                var paramName = $"@{field.RefTableField}";
                                var sql = $"SELECT F_Id FROM {field.RefTableCode} t WHERE {field.RefTableField} = {paramName} AND NOT EXISTS( SELECT F_Id FROM {field.RefTableCode} WHERE ParentId = t.F_Id)";
                                var sqlParams = new List<SqlParameter>() { new SqlParameter(paramName, singleClause.Value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() } };
                                var result = await _accountancyDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
                                isRequire = result != null && result.Rows.Count > 0;
                                break;
                            case EnumOperator.StartsWith:
                                isRequire = rowValues.Any(v => v.StringStartsWith(singleClause.Value));
                                break;
                            case EnumOperator.NotStartsWith:
                                isRequire = rowValues.All(v => !v.StringStartsWith(singleClause.Value));
                                break;
                            case EnumOperator.EndsWith:
                                isRequire = rowValues.Any(v => v.StringEndsWith(singleClause.Value));
                                break;
                            case EnumOperator.NotEndsWith:
                                isRequire = rowValues.All(v => !v.StringEndsWith(singleClause.Value));
                                break;
                            case EnumOperator.IsNull:
                                isRequire = rowValues.Any(v => v == null);
                                break;
                            case EnumOperator.IsEmpty:
                                isRequire = rowValues.Any(v => v != null && string.IsNullOrEmpty(v.ToString()));
                                break;
                            case EnumOperator.IsNullOrEmpty:
                                isRequire = rowValues.Any(v => v == null || string.IsNullOrEmpty(v.ToString()));
                                break;
                            default:
                                isRequire = true;
                                break;
                        }
                    }
                    else
                    {
                        object value = null;
                        if (!field.IsMultiRow)
                        {
                            info.Data.TryGetValue(field.FieldName, out value);
                        }
                        else
                        {
                            rows[rowIndex.Value].Data.TryGetValue(field.FieldName, out value);
                        }

                        value = ((EnumDataType)field.DataTypeId).GetSqlValue(value);

                        if (sfValues.ContainsKey(field.FieldName) && value != null)
                        {
                            value = sfValues[field.FieldName].ContainsKey(value) ? sfValues[field.FieldName][value] : null;
                        }
                        switch (singleClause.Operator)
                        {
                            case EnumOperator.Equal:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) == 0;
                                break;
                            case EnumOperator.NotEqual:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) != 0;
                                break;
                            case EnumOperator.Contains:
                                isRequire = value.StringContains(singleClause.Value);
                                break;
                            case EnumOperator.NotContains:
                                isRequire = !value.StringContains(singleClause.Value);
                                break;
                            case EnumOperator.InList:
                                var arrValues = singleClause.Value.ToString().Split(",");
                                isRequire = value != null && arrValues.Contains(value.ToString());
                                break;
                            case EnumOperator.IsLeafNode:
                                // Check is leaf node
                                var paramName = $"@{field.RefTableField}";
                                var sql = $"SELECT F_Id FROM {field.RefTableCode} t WHERE {field.RefTableField} = {paramName} AND NOT EXISTS( SELECT F_Id FROM {field.RefTableCode} WHERE ParentId = t.F_Id)";
                                var sqlParams = new List<SqlParameter>() { new SqlParameter(paramName, singleClause.Value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() } };
                                var result = await _accountancyDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
                                isRequire = result != null && result.Rows.Count > 0;
                                break;
                            case EnumOperator.StartsWith:
                                isRequire = value.StringStartsWith(singleClause.Value);
                                break;
                            case EnumOperator.NotStartsWith:
                                isRequire = !value.StringStartsWith(singleClause.Value);
                                break;
                            case EnumOperator.EndsWith:
                                isRequire = value.StringEndsWith(singleClause.Value);
                                break;
                            case EnumOperator.NotEndsWith:
                                isRequire = !value.StringEndsWith(singleClause.Value);
                                break;
                            case EnumOperator.IsNull:
                                isRequire = value == null;
                                break;
                            case EnumOperator.IsEmpty:
                                isRequire = value != null && string.IsNullOrEmpty(value.ToString());
                                break;
                            case EnumOperator.IsNullOrEmpty:
                                isRequire = value == null || string.IsNullOrEmpty(value.ToString());
                                break;
                            case EnumOperator.Greater:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) > 0;
                                break;
                            case EnumOperator.GreaterOrEqual:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) >= 0;
                                break;
                            case EnumOperator.LessThan:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) < 0;
                                break;
                            case EnumOperator.LessThanOrEqual:
                                isRequire = ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) <= 0;
                                break;
                            default:
                                isRequire = true;
                                break;
                        }
                    }
                    isRequire = not ? !isRequire : isRequire;
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        bool clauseResult = await CheckRequireFilter(arrClause.Rules.ElementAt(indx), info, rows, inputAreaFields, sfValues, rowIndex, isNot);
                        isRequire = isRequire.HasValue ? isOr ? isRequire.Value || clauseResult : isRequire.Value && clauseResult : clauseResult;
                    }
                }
            }
            return isRequire.Value;
        }

        private async Task<(int Code, string Message, List<NonCamelCaseDictionary> ResultData)> ProcessActionAsync(int inputTypeId, string script, BillInfoModel data, Dictionary<string, EnumDataType> fields, EnumActionType action, long inputValueBillId = 0)
        {
            List<NonCamelCaseDictionary> resultData = null;
            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(script))
            {
                DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                var parammeters = new List<SqlParameter>() {
                    new SqlParameter("@Action", (int)action),
                    new SqlParameter("@BillF_Id", inputValueBillId),
                      new SqlParameter("@InputTypeId", inputTypeId),
                    resultParam,
                    messageParam,
                    new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.InputTableType" }
                };

                resultData = (await _accountancyDBContext.QueryDataTable(script, parammeters)).ConvertData();
            }
            return ((resultParam.Value as int?).GetValueOrDefault(), messageParam.Value as string, resultData);
        }

        private string[] GetFieldInFilter(Clause[] clauses)
        {
            List<string> fields = new List<string>();
            foreach (var clause in clauses)
            {
                if (clause == null) continue;

                if (clause is SingleClause)
                {
                    fields.Add((clause as SingleClause).FieldName);
                }
                else if (clause is ArrayClause)
                {
                    fields.AddRange(GetFieldInFilter((clause as ArrayClause).Rules.ToArray()));
                }
            }

            return fields.Distinct().ToArray();
        }

        private async Task CheckRequired(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> requiredFields, List<ValidateField> inputAreaFields)
        {
            var filters = requiredFields
                .Where(f => !string.IsNullOrEmpty(f.RequireFilters))
                .ToDictionary(f => f.FieldName, f => JsonConvert.DeserializeObject<Clause>(f.RequireFilters));

            string[] filterFieldNames = GetFieldInFilter(filters.Select(f => f.Value).ToArray());
            var sfFields = inputAreaFields.Where(f => ((EnumFormType)f.FormTypeId).IsSelectForm() && filterFieldNames.Contains(f.FieldName)).ToList();
            var sfValues = new Dictionary<string, Dictionary<object, object>>();

            foreach (var field in sfFields)
            {
                var values = new List<object>();
                if (field.IsMultiRow)
                {
                    values.AddRange(rows.Where(r => r.Data.ContainsKey(field.FieldName) && r.Data[field.FieldName] != null).Select(r => r.Data[field.FieldName]));
                }
                else
                {
                    if (info.Data.ContainsKey(field.FieldName) && info.Data[field.FieldName] != null) values.Add(info.Data[field.FieldName]);
                }
                if (values.Count > 0)
                {
                    Dictionary<object, object> mapTitles = new Dictionary<object, object>(new DataEqualityComparer((EnumDataType)field.DataTypeId));
                    var sqlParams = new List<SqlParameter>();
                    var sql = new StringBuilder($"SELECT DISTINCT {field.RefTableField}, {field.RefTableTitle} FROM v{field.RefTableCode} WHERE {field.RefTableField} IN (");
                    var suffix = 0;
                    foreach (var value in values)
                    {
                        var paramName = $"@{field.RefTableField}_{suffix}";
                        if (suffix > 0) sql.Append(",");
                        sql.Append(paramName);
                        sqlParams.Add(new SqlParameter(paramName, value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() });
                        suffix++;
                    }

                    sql.Append(")");
                    var data = await _accountancyDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
                    for (int indx = 0; indx < data.Rows.Count; indx++)
                    {
                        mapTitles.Add(data.Rows[indx][field.RefTableField], data.Rows[indx][field.RefTableTitle]);
                    }
                    sfValues.Add(field.FieldName, mapTitles);
                }
            }


            foreach (var field in requiredFields)
            {
                // ignore auto generate field
                if (field.FormTypeId == (int)EnumFormType.Generate) continue;

                // Validate info
                if (!field.IsMultiRow)
                {
                    if (info.CheckFields != null && !info.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(field.RequireFilters))
                    {
                        Clause filterClause = filters[field.FieldName];
                        if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, inputAreaFields, sfValues, null)))
                        {
                            continue;
                        }
                    }

                    info.Data.TryGetValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { SingleRowArea, field.Title });
                    }
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(field.RequireFilters))
                        {
                            Clause filterClause = JsonConvert.DeserializeObject<Clause>(field.RequireFilters);
                            if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, inputAreaFields, sfValues, rowIndx - 1)))
                            {
                                continue;
                            }
                        }

                        row.Data.TryGetValue(field.FieldName, out string value);
                        if (string.IsNullOrEmpty(value))
                        {
                            throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { row.ExcelRow ?? rowIndx, field.Title });
                        }
                    }
                }
            }
        }

        private async Task CheckUniqueAsync(int inputTypeId, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> uniqueFields, long? inputValueBillId = null)
        {
            // Check unique
            foreach (var field in uniqueFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    if (info.CheckFields != null && !info.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    info.Data.TryGetValue(field.FieldName, out object value);
                    // Checkin unique trong db
                    if (value != null)
                        await ValidUniqueAsync(inputTypeId, new List<object>() { ((EnumDataType)field.DataTypeId).GetSqlValue(value) }, field, inputValueBillId);
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                        {
                            continue;
                        }
                        // Get list change value
                        List<object> values = new List<object>();
                        row.Data.TryGetValue(field.FieldName, out object value);
                        if (value != null)
                        {
                            values.Add(((EnumDataType)field.DataTypeId).GetSqlValue(value));
                        }
                        // Check unique trong danh sách values thêm mới/sửa
                        if (values.Count != values.Distinct().Count())
                        {
                            var dupValue = values.GroupBy(v => v).Where(v => v.Count() > 1).FirstOrDefault()?.Key?.ToString();
                            throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, dupValue, "" });
                        }
                        if (values.Count == 0)
                        {
                            continue;
                        }
                        // Checkin unique trong db
                        await ValidUniqueAsync(inputTypeId, values, field, inputValueBillId);
                    }
                }
            }
        }

        private async Task ValidUniqueAsync(int inputTypeId, List<object> values, ValidateField field, long? inputValueBillId = null)
        {
            string typeTitleField = AccountantConstants.INPUT_TYPE_TITLE;
            var existSql = $"SELECT F_Id, {typeTitleField}, {field.FieldName} FROM vInputValueRow WHERE ";
            if (field.FieldName == AccountantConstants.BILL_CODE)
            {
                existSql += $" 1 = 1 ";
            }
            else
            {
                existSql += $" InputTypeId = {inputTypeId} ";
            }

            if (inputValueBillId.HasValue)
            {
                existSql += $"AND InputBill_F_Id != {inputValueBillId}";
            }
            existSql += $" AND {field.FieldName} IN (";
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            var suffix = 0;
            foreach (var value in values)
            {
                var paramName = $"@{field.FieldName}_{suffix}";
                if (suffix > 0)
                {
                    existSql += ",";
                }
                existSql += paramName;
                sqlParams.Add(new SqlParameter(paramName, value));
                suffix++;
            }
            existSql += ")";
            var result = await _accountancyDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;

            if (isExisted)
            {
                throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, result.Rows[0][field.FieldName]?.ToString(), result.Rows[0][typeTitleField]?.ToString() });
            }
        }

        private async Task CheckReferAsync(List<ValidateField> allFields, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    await ValidReferAsync(allFields, info, info, field, null);
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        await ValidReferAsync(allFields, row, info, field, rowIndx);
                    }
                }
            }
        }

        private async Task ValidReferAsync(List<ValidateField> allFields, ValidateRowModel checkData, ValidateRowModel info, ValidateField field, int? rowIndex)
        {
            string tableName = $"v{field.RefTableCode}";
            if (checkData.CheckFields != null && !checkData.CheckFields.Contains(field.FieldName))
            {
                return;
            }
            checkData.Data.TryGetValue(field.FieldName, out string textValue);
            if (string.IsNullOrEmpty(textValue))
            {
                return;
            }
            var value = ((EnumDataType)field.DataTypeId).GetSqlValue(textValue);
            var whereCondition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            int suffix = 0;
            var paramName = $"@{field.RefTableField}_{suffix}";
            var existSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
            sqlParams.Add(new SqlParameter(paramName, value));
            if (!string.IsNullOrEmpty(field.Filters))
            {
                var filters = field.Filters;
                var pattern = @"@{(?<word>\w+)}\((?<start>\d*),(?<length>\d*)\)";
                Regex rx = new Regex(pattern);
                MatchCollection match = rx.Matches(field.Filters);
                for (int i = 0; i < match.Count; i++)
                {
                    var fieldName = match[i].Groups["word"].Value;
                    var startText = match[i].Groups["start"].Value;
                    var lengthText = match[i].Groups["length"].Value;
                    checkData.Data.TryGetValue(fieldName, out string filterValue);
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        info.Data.TryGetValue(fieldName, out filterValue);
                    }
                    if (!string.IsNullOrEmpty(filterValue) && !string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                    {
                        if (filterValue.Length < start)
                        {
                            //TODO: Validate message
                            throw new BadRequestException($"Invalid value sustring {filterValue} start {start}, length {length}");
                        }

                        filterValue = filterValue.Substring(start, length);
                    }
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        var fieldBefore = (allFields.FirstOrDefault(f => f.FieldName == fieldName)?.Title) ?? fieldName;
                        throw RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                    }

                    filters = filters.Replace(match[i].Value, filterValue);
                }

                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                if (filterClause != null)
                {
                    filterClause.FilterClauseProcess(tableName, tableName, ref whereCondition, ref sqlParams, ref suffix);
                }
            }

            if (whereCondition.Length > 0)
            {
                existSql += $" AND {whereCondition}";
            }

            var result = await _accountancyDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, value) };
                result = await _accountancyDBContext.QueryDataTable(checkExistedReferSql, checkExistedReferParams.ToArray());
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(InputErrorCode.ReferValueNotValidFilter, new object[] { rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title + ": " + value });
                }
            }
        }

        private void CheckValue(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    ValidValueAsync(info, field, null);
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        ValidValueAsync(row, field, rowIndx);
                    }
                }
            }
        }

        private void ValidValueAsync(ValidateRowModel checkData, ValidateField field, int? rowIndex)
        {
            if (checkData.CheckFields != null && !checkData.CheckFields.Contains(field.FieldName))
            {
                return;
            }

            checkData.Data.TryGetValue(field.FieldName, out string value);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            if (((EnumFormType)field.FormTypeId).IsSelectForm() || field.IsAutoIncrement || string.IsNullOrEmpty(value))
            {
                return;
            }
            string regex = ((EnumDataType)field.DataTypeId).GetRegex();
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
            {
                throw new BadRequestException(InputErrorCode.InputValueInValid, new object[] { value?.JsonSerialize(), rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title });
            }
        }

        private void AppendSelectFields(ref StringBuilder sql, List<ValidateField> fields)
        {
            for (int indx = 0; indx < fields.Count; indx++)
            {
                if (indx > 0)
                {
                    sql.Append(", ");
                }
                sql.Append(fields[indx].FieldName);
            }
        }

        public async Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data)
        {
            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0)
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            // Lấy thông tin field
            var inputAreaFields = await GetInputFields(inputTypeId);

            // Get changed info
            var infoSQL = new StringBuilder("SELECT TOP 1 ");
            var singleFields = inputAreaFields.Where(f => !f.IsMultiRow).ToList();
            AppendSelectFields(ref infoSQL, singleFields);
            infoSQL.Append($" FROM vInputValueRow r WHERE InputTypeId={inputTypeId} AND InputBill_F_Id = {inputValueBillId} AND {GlobalFilter()}");
            var currentInfo = (await _accountancyDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData().FirstOrDefault();

            if (currentInfo == null)
            {
                throw BillNotFound.BadRequest();
            }

            await ValidateAccountantConfig(data?.Info, currentInfo);

            NonCamelCaseDictionary futureInfo = data.Info;
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, CompareRow(currentInfo, futureInfo, singleFields), null);

            // Get changed rows
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            var rowsSQL = new StringBuilder("SELECT F_Id,");
            var multiFields = inputAreaFields.Where(f => f.IsMultiRow).ToList();
            AppendSelectFields(ref rowsSQL, multiFields);
            rowsSQL.Append($" FROM vInputValueRow r WHERE InputBill_F_Id = {inputValueBillId} AND {GlobalFilter()}");
            var currentRows = (await _accountancyDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            foreach (var futureRow in data.Rows)
            {
                futureRow.TryGetValue("F_Id", out string futureValue);
                NonCamelCaseDictionary curRow = currentRows.FirstOrDefault(r => futureValue != null && r["F_Id"].ToString() == futureValue);
                if (curRow == null)
                {
                    checkRows.Add(new ValidateRowModel(futureRow, null, null));
                }
                else
                {
                    string[] changeFieldIndexes = CompareRow(curRow, futureRow, multiFields);
                    checkRows.Add(new ValidateRowModel(futureRow, changeFieldIndexes, null));
                }
            }

            // Lấy thông tin field
            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = inputAreaFields.Where(f => (!f.IsAutoIncrement && f.IsUnique) || f.FieldName == AccountantConstants.BILL_CODE).ToList();
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, inputAreaFields);
            // Check refer
            await CheckReferAsync(inputAreaFields, checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkInfo, checkRows, uniqueFields, inputValueBillId);
            // Check value
            CheckValue(checkInfo, checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get all fields
                var inputFields = _accountancyDBContext.InputField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, data, inputFields, EnumActionType.Update, inputValueBillId);
                if (result.Code != 0)
                {
                    if (string.IsNullOrWhiteSpace(result.Message))
                        throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                    else
                    {
                        throw result.Message.BadRequest();

                    }
                }
                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.InputTypeId == inputTypeId && b.FId == inputValueBillId && b.SubsidiaryId == _currentContextService.SubsidiaryId);

                if (billInfo == null) throw BillNotFound.BadRequest();


                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);


                var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                billInfo.LatestBillVersion++;

                await CreateBillVersion(inputTypeId, billInfo, data, generateTypeLastValues);



                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Update, inputValueBillId);

                await ConfirmCustomGenCode(generateTypeLastValues);

                trans.Commit();

                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Update)
                  .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                  .BillTypeId(inputTypeId)
                  .ObjectId(billInfo.FId)
                  .JsonData(data.JsonSerialize())
                  .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateBill");
                throw;
            }
        }

        public async Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            if (fIds.Length == 0) throw ListBillsToUpdateIsEmpty.BadRequest();

            // Get field
            var field = _accountancyDBContext.InputAreaField.Include(f => f.InputField).FirstOrDefault(f => f.InputField.FieldName == fieldName);
            if (field == null) throw FieldNotFound.BadRequest();

            object oldSqlValue;
            object newSqlValue;
            if (((EnumFormType)field.InputField.FormTypeId).IsSelectForm())
            {
                var refTableTitle = field.InputField.RefTableTitle.Split(',')[0];
                var categoryFields = await _httpCategoryHelperService.GetReferFields(new List<string>() { field.InputField.RefTableCode }, new List<string>() { refTableTitle, field.InputField.RefTableField });
                var refField = categoryFields.FirstOrDefault(f => f.CategoryFieldName == field.InputField.RefTableField);
                var refTitleField = categoryFields.FirstOrDefault(f => f.CategoryFieldName == refTableTitle);
                if (refField == null || refTitleField == null) throw FieldRefNotFound.BadRequest();
                var selectSQL = $"SELECT {field.InputField.RefTableField} FROM v{field.InputField.RefTableCode} WHERE {refTableTitle} = @ValueParam";

                if (oldValue != null)
                {
                    var oldResult = await _accountancyDBContext.QueryDataTable(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldValue)) });
                    if (oldResult == null || oldResult.Rows.Count == 0) throw OldValueIsInvalid.BadRequest();
                    oldSqlValue = ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldResult.Rows[0][0]);
                }
                else
                {
                    oldSqlValue = DBNull.Value;
                }

                if (newValue != null)
                {
                    var newResult = await _accountancyDBContext.QueryDataTable(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(newValue)) });
                    if (newResult == null || newResult.Rows.Count == 0) throw NewValueIsInvalid.BadRequest();
                    newSqlValue = ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(newResult.Rows[0][0]);
                }
                else
                {
                    newSqlValue = DBNull.Value;
                }
            }
            else
            {
                oldSqlValue = ((EnumDataType)field.InputField.DataTypeId).GetSqlValue(oldValue);
                newSqlValue = ((EnumDataType)field.InputField.DataTypeId).GetSqlValue(newValue);
            }

            var singleFields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
                select f.FieldName).ToListAsync()).ToHashSet();

            // Get bills by old value
            var sqlParams = new List<SqlParameter>();
            var dataSql = new StringBuilder(@$"

                SELECT     r.*
                FROM {INPUTVALUEROW_TABLE} r 

                WHERE r.InputTypeId = {inputTypeId} AND r.IsDeleted = 0 AND r.InputBill_F_Id IN ({string.Join(',', fIds)}) AND {GlobalFilter()}");


            if (oldValue == null)
            {
                dataSql.Append($" AND r.{fieldName} IS NULL");
            }
            else
            {
                var paramName = $"@{fieldName}";
                dataSql.Append($" AND r.{fieldName} = {paramName}");
                sqlParams.Add(new SqlParameter(paramName, oldSqlValue));
            }

            var data = await _accountancyDBContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());
            var updateBillIds = new HashSet<long>();

            // Update new value
            var dataTable = new DataTable(INPUTVALUEROW_TABLE);
            foreach (DataColumn column in data.Columns)
            {
                if (column.ColumnName != "F_Id")
                    dataTable.Columns.Add(column.ColumnName, column.DataType);
            }

            var oldBillDates = new Dictionary<long, DateTime?>();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];

                var billId = (long)row["InputBill_F_Id"];
                if (!updateBillIds.Contains(billId))
                {
                    updateBillIds.Add(billId);
                    oldBillDates.Add(billId, null);
                }

                var newRow = dataTable.NewRow();
                foreach (DataColumn column in data.Columns)
                {
                    var v = row[column];

                    if (column.ColumnName.Equals(AccountantConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) && !v.IsNullObject())
                    {
                        oldBillDates[billId] = v as DateTime?;
                    }

                    switch (column.ColumnName)
                    {
                        case "F_Id":
                            continue;
                        case "BillVersion":
                            newRow[column.ColumnName] = (int)v + 1;
                            break;
                        case "CreatedByUserId":
                        case "UpdatedByUserId":
                            newRow[column.ColumnName] = _currentContextService.UserId;
                            break;
                        case "CreatedDatetimeUtc":
                        case "UpdatedDatetimeUtc":
                            newRow[column.ColumnName] = DateTime.UtcNow;
                            break;
                        default:
                            newRow[column.ColumnName] = v;
                            break;
                    }
                }
                newRow[fieldName] = newSqlValue;
                dataTable.Rows.Add(newRow);
            }

            foreach (var oldBillDate in oldBillDates)
            {
                var newDate = fieldName.Equals(AccountantConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) ? (newSqlValue as DateTime?) : null;

                await ValidateAccountantConfig(newDate ?? oldBillDate.Value, oldBillDate.Value);
            }

            var bills = _accountancyDBContext.InputBill.Where(b => updateBillIds.Contains(b.FId) && b.SubsidiaryId == _currentContextService.SubsidiaryId).ToList();
            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Created bill version
                await _accountancyDBContext.InsertDataTable(dataTable, true);

                using (var batch = _inputDataActivityLog.BeginBatchLog())
                {
                    foreach (var bill in bills)
                    {
                        // Delete bill version
                        await DeleteBillVersion(inputTypeId, bill.FId, bill.LatestBillVersion);


                        await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.UpdateMulti)
                            .MessageResourceFormatDatas(inputTypeInfo.Title, field?.Title + " (" + field?.Title + ")", bill.BillCode)
                            .BillTypeId(inputTypeId)
                            .ObjectId(bill.FId)
                            .JsonData(new { inputTypeId, fieldName, oldValue, newValue, fIds }.JsonSerialize().JsonSerialize())
                            .CreateLog();

                        // Update last bill version
                        bill.LatestBillVersion++;
                    }

                    await _accountancyDBContext.SaveChangesAsync();
                    trans.Commit();

                }


                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateBill");
                throw;
            }
        }

        private string[] CompareRow(NonCamelCaseDictionary currentRow, NonCamelCaseDictionary futureRow, List<ValidateField> fields)
        {
            if (currentRow == null || futureRow == null)
            {
                return null;
            }
            List<string> changeFieldIndexes = new List<string>();
            foreach (var field in fields)
            {
                currentRow.TryGetValue(field.FieldName, out object currentValue);
                futureRow.TryGetValue(field.FieldName, out object updateValue);

                if (((EnumDataType)field.DataTypeId).CompareValue(currentValue, updateValue) != 0)
                {
                    changeFieldIndexes.Add(field.FieldName);
                }
            }
            return changeFieldIndexes.ToArray();
        }

        private async Task<ITypeExecData> GetInputTypExecInfo(int inputTypeId)
        {
            var global = await _inputConfigService.GetInputGlobalSetting();
            var inputTypeInfo = await _accountancyDBContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);
            if (inputTypeInfo == null) throw InputTypeNotFound.BadRequest();
            var info = _mapper.Map<InputTypeExecData>(inputTypeInfo);
            info.GlobalSetting = global;
            return info;
        }

        public async Task<bool> DeleteBill(int inputTypeId, long inputBill_F_Id)
        {
            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputBill_F_Id && b.SubsidiaryId == _currentContextService.SubsidiaryId);

                if (billInfo == null) throw BillNotFound.BadRequest();

                var inputAreaFields = new List<ValidateField>();

                // Get current data
                BillInfoModel data = new BillInfoModel();
                // Lấy thông tin field
                inputAreaFields = await GetInputFields(inputTypeId);

                // Get changed row info
                var infoSQL = new StringBuilder("SELECT TOP 1 ");
                var singleFields = inputAreaFields.Where(f => !f.IsMultiRow).ToList();
                for (int indx = 0; indx < singleFields.Count; indx++)
                {
                    if (indx > 0)
                    {
                        infoSQL.Append(", ");
                    }
                    infoSQL.Append(singleFields[indx].FieldName);
                }
                infoSQL.Append($" FROM vInputValueRow r WHERE InputBill_F_Id = {inputBill_F_Id} AND {GlobalFilter()}");
                var infoLst = (await _accountancyDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();

                data.Info = infoLst.Count != 0 ? infoLst[0].ToNonCamelCaseDictionary(f => f.Key, f => f.Value) : new NonCamelCaseDictionary();
                if (!string.IsNullOrEmpty(inputTypeInfo.BeforeSaveActionExec) || !string.IsNullOrEmpty(inputTypeInfo.AfterSaveActionExec))
                {
                    var rowsSQL = new StringBuilder("SELECT ");
                    var multiFields = inputAreaFields.Where(f => f.IsMultiRow).ToList();
                    for (int indx = 0; indx < multiFields.Count; indx++)
                    {
                        if (indx > 0)
                        {
                            rowsSQL.Append(", ");
                        }
                        rowsSQL.Append(multiFields[indx].FieldName);
                    }
                    rowsSQL.Append($" FROM vInputValueRow r WHERE InputBill_F_Id = {inputBill_F_Id} AND {GlobalFilter()}");
                    var currentRows = (await _accountancyDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
                    data.Rows = currentRows.Select(r => r.ToNonCamelCaseDictionary(f => f.Key, f => f.Value.ToString())).ToArray();
                }
                await ValidateAccountantConfig(null, data?.Info);

                // Get all fields
                var inputFields = _accountancyDBContext.InputField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, data, inputFields, EnumActionType.Delete, inputBill_F_Id);

                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);

                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Delete, inputBill_F_Id);

                await _outsideMappingHelperService.MappingObjectDelete(EnumObjectType.InputBill, billInfo.FId);

                trans.Commit();

                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Delete)
                           .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                           .BillTypeId(inputTypeId)
                           .ObjectId(billInfo.FId)
                           .JsonData(data.JsonSerialize().JsonSerialize())
                           .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteBill");
                throw;
            }
        }

        private async Task FillGenerateColumn(long? fId, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues, Dictionary<string, ValidateField> fields, IList<NonCamelCaseDictionary> rows)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                foreach (var infoField in fields)
                {
                    var field = infoField.Value;

                    if ((EnumFormType)field.FormTypeId == EnumFormType.Generate &&
                        (!row.TryGetValue(field.FieldName, out var value) || value.IsNullObject())
                    )
                    {

                        var code = rows.FirstOrDefault(r => r.ContainsKey(AccountantConstants.BILL_CODE))?[AccountantConstants.BILL_CODE]?.ToString();

                        var ngayCt = rows.FirstOrDefault(r => r.ContainsKey(AccountantConstants.BILL_DATE))?[AccountantConstants.BILL_DATE]?.ToString();

                        long? ngayCtValue = null;
                        if (long.TryParse(ngayCt, out var v))
                        {
                            ngayCtValue = v;
                        }

                        CustomGenCodeOutputModel currentConfig;
                        try
                        {
                            currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.InputTypeRow, EnumObjectType.InputAreaField, field.InputAreaFieldId, fId, code, ngayCtValue);

                            if (currentConfig == null)
                            {
                                throw GenerateCodeConfigForFieldNotFound.BadRequestFormat(field.Title);
                            }
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw badRequest.Code.BadRequestFormat(GenerateCodeFieldBadRequest, field.Title, badRequest.Message);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        var generateType = $"{currentConfig.CustomGenCodeId}_{currentConfig.CurrentLastValue.BaseValue}";

                        if (!generateTypeLastValues.ContainsKey(generateType))
                        {
                            generateTypeLastValues.Add(generateType, currentConfig.CurrentLastValue);
                        }

                        var lastTypeValue = generateTypeLastValues[generateType];


                        try
                        {

                            var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, lastTypeValue.LastValue, fId, code, ngayCtValue);
                            if (generated == null)
                            {
                                throw GeneralCode.InternalError.BadRequestFormat(GenerateCodeFieldError, field.Title);
                            }


                            value = generated.CustomCode;
                            lastTypeValue.LastValue = generated.LastValue;
                            lastTypeValue.LastCode = generated.CustomCode;
                            lastTypeValue.BaseValue = generated.BaseValue;
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw badRequest.Code.BadRequestFormat(GenerateCodeFieldBadRequest, field.Title, badRequest.Message);
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        if (!row.ContainsKey(field.FieldName))
                        {
                            row.Add(field.FieldName, value);
                        }
                        else
                        {
                            row[field.FieldName] = value;
                        }
                    }
                }
            }
        }

        private async Task ConfirmCustomGenCode(Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues)
        {
            foreach (var (_, lasValue) in generateTypeLastValues)
            {
                await _customGenCodeHelperService.ConfirmCode(lasValue);
            }
        }

        private async Task CreateBillVersion(int inputTypeId, InputBill billInfo, BillInfoModel data, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues)
        {

            var fields = (await GetInputFields(inputTypeId)).ToDictionary(f => f.FieldName, f => f);


            var infoFields = fields.Where(f => !f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(billInfo.FId, generateTypeLastValues, infoFields, new[] { data.Info });

            if (data.Info.TryGetValue(AccountantConstants.BILL_CODE, out var sct))
            {
                Utils.ValidateCodeSpecialCharactors(sct);
                sct = sct?.ToUpper();
                data.Info[AccountantConstants.BILL_CODE] = sct;
                billInfo.BillCode = sct;
            }

            var rowFields = fields.Where(f => f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(billInfo.FId, generateTypeLastValues, rowFields, data.Rows);

            var insertColumns = new HashSet<string>();

            var removeKeys = new HashSet<string>();
            foreach (var item in data.Info)
            {
                if (!fields.ContainsKey(item.Key) || fields[item.Key].IsMultiRow)
                {
                    removeKeys.Add(item.Key);
                }
                else
                {
                    insertColumns.Add(item.Key);
                }
            }


            foreach (var key in removeKeys)
            {
                data.Info.Remove(key);
            }

            foreach (var row in data.Rows)
            {
                removeKeys.Clear();

                foreach (var item in row)
                {
                    if (!fields.ContainsKey(item.Key) || !fields[item.Key].IsMultiRow)
                    {
                        removeKeys.Add(item.Key);
                    }
                    else
                    {
                        if (!insertColumns.Contains(item.Key))
                        {
                            insertColumns.Add(item.Key);
                        }
                    }
                }

                foreach (var key in removeKeys)
                {
                    row.Remove(key);
                }
            }

            var dataTable = new DataTable(INPUTVALUEROW_TABLE);

            dataTable.Columns.Add("InputTypeId", typeof(int));
            dataTable.Columns.Add("InputBill_F_Id", typeof(long));
            dataTable.Columns.Add("BillVersion", typeof(int));
            dataTable.Columns.Add("IsBillEntry", typeof(bool));
            dataTable.Columns.Add("CreatedByUserId", typeof(int));
            dataTable.Columns.Add("CreatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("IsDeleted", typeof(bool));
            dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("SubsidiaryId", typeof(int));

            var sumReciprocals = new Dictionary<string, decimal>();
            foreach (var column in insertColumns)
            {
                var field = fields[column];

                dataTable.Columns.Add(column, ((EnumDataType)field.DataTypeId).GetColumnDataType());

                if (column.IsVndColumn())
                {
                    var sumColumn = column.VndSumName();
                    dataTable.Columns.Add(sumColumn, EnumDataType.Decimal.GetColumnDataType());
                    sumReciprocals.Add(sumColumn, 0);
                }
            }


            var requireFields = fields.Values.Where(f => f.IsRequire).Select(f => f.FieldName).Distinct().ToHashSet();

            var ignoreCopyInfoValues = new HashSet<string>();
            //Create rows
            foreach (var row in data.Rows)
            {
                var dataRow = NewBillVersionRow(dataTable, inputTypeId, billInfo.FId, billInfo.LatestBillVersion, false);

                foreach (var item in data.Info)
                {
                    if (ignoreCopyInfoValues.Contains(item.Key))
                        continue;

                    if (item.Key.IsVndColumn() || item.Key.IsNgoaiTeColumn())
                    {
                        ignoreCopyInfoValues.Add(item.Key);
                        continue;
                    }

                    if (item.Key.IsTkCoColumn())
                    {
                        var butToan = item.Key.Substring(AccountantConstants.TAI_KHOAN_CO_PREFIX.Length);
                        var tkNo = AccountantConstants.TAI_KHOAN_NO_PREFIX + butToan;
                        if (data.Info.Keys.Any(k => k.Equals(tkNo, StringComparison.OrdinalIgnoreCase)))
                        {
                            ignoreCopyInfoValues.Add(item.Key);
                            continue;
                        }
                    }

                    if (item.Key.IsTkNoColumn())
                    {
                        var butToan = item.Key.Substring(AccountantConstants.TAI_KHOAN_NO_PREFIX.Length);
                        var tkCo = AccountantConstants.TAI_KHOAN_CO_PREFIX + butToan;
                        if (data.Info.Keys.Any(k => k.Equals(tkCo, StringComparison.OrdinalIgnoreCase)))
                        {
                            ignoreCopyInfoValues.Add(item.Key);
                            continue;
                        }
                    }

                    var field = fields[item.Key];
                    dataRow[item.Key] = ((EnumDataType)field.DataTypeId).GetSqlValue(item.Value);
                }

                foreach (var item in row)
                {
                    var field = fields[item.Key];
                    var value = ((EnumDataType)field.DataTypeId).GetSqlValue(item.Value);
                    dataRow[item.Key] = value;

                    if (item.Key.IsVndColumn() && !value.IsNullObject())
                    {
                        var deValue = Convert.ToDecimal(value);
                        var colName = item.Key.VndSumName();

                        sumReciprocals[colName] += deValue;

                        //ignore sum_vnd on detail row
                        //dataRow[colName] = deValue;
                        dataRow[colName] = DBNull.Value;
                    }

                }

                var inValidReciprocalColumn = GetInValidReciprocalColumn(dataTable, dataRow, requireFields);
                if (!string.IsNullOrWhiteSpace(inValidReciprocalColumn))
                {
                    var key = fields.Keys.FirstOrDefault(k => k.Equals(inValidReciprocalColumn, StringComparison.OrdinalIgnoreCase));
                    var fieldTitle = "";
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        fieldTitle = fields[key].Title;
                    }

                    throw PairAccountError.BadRequestFormat(fieldTitle);
                }

                dataTable.Rows.Add(dataRow);
            }



            //Create addition reciprocal accounting
            if (data.Info.Any(k => k.Key.IsVndColumn() && decimal.TryParse(k.Value?.ToString(), out var value) && value != 0))
            {
                var dataRow = NewBillVersionRow(dataTable, inputTypeId, billInfo.FId, billInfo.LatestBillVersion, true);

                foreach (var item in data.Info)
                {
                    var field = fields[item.Key];
                    dataRow[item.Key] = ((EnumDataType)field.DataTypeId).GetSqlValue(item.Value);
                }
                foreach (var sum in sumReciprocals)
                {
                    dataRow[sum.Key] = sum.Value;
                }

                var inValidReciprocalColumn = GetInValidReciprocalColumn(dataTable, dataRow, requireFields);
                if (!string.IsNullOrWhiteSpace(inValidReciprocalColumn))
                {
                    var key = fields.Keys.FirstOrDefault(k => k.Equals(inValidReciprocalColumn, StringComparison.OrdinalIgnoreCase));
                    var fieldTitle = "";
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        fieldTitle = fields[key].Title;
                    }

                    throw PairAccountError.BadRequestFormat(fieldTitle);
                }

                dataTable.Rows.Add(dataRow);
            }

            await _accountancyDBContext.InsertDataTable(dataTable);

        }

        private DataRow NewBillVersionRow(DataTable dataTable, int inputTypeId, long inputBill_F_Id, int billVersionId, bool isBillEntry)
        {
            var dataRow = dataTable.NewRow();

            dataRow["InputTypeId"] = inputTypeId;
            dataRow["InputBill_F_Id"] = inputBill_F_Id;
            dataRow["BillVersion"] = billVersionId;
            dataRow["IsBillEntry"] = isBillEntry;
            dataRow["CreatedByUserId"] = _currentContextService.UserId;
            dataRow["CreatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["IsDeleted"] = false;
            dataRow["DeletedDatetimeUtc"] = DBNull.Value;
            dataRow["SubsidiaryId"] = _currentContextService.SubsidiaryId;
            return dataRow;
        }

        private string GetInValidReciprocalColumn(DataTable dataTable, DataRow dataRow, HashSet<string> requireFields)
        {
            for (var i = 0; i <= AccountantConstants.MAX_COUPLE_RECIPROCAL; i++)
            {
                var credit_column_name = AccountantConstants.TAI_KHOAN_CO_PREFIX + i;
                var debit_column_name = AccountantConstants.TAI_KHOAN_NO_PREFIX + i;
                var money_column_name = AccountantConstants.THANH_TIEN_VND_PREFIX + i;

                object tk_co = null;
                object tk_no = null;
                decimal vnd = 0;

                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var column = dataTable.Columns[j];
                    if (dataRow[column] == null || string.IsNullOrWhiteSpace(dataRow[column]?.ToString())) continue;

                    if (column.ColumnName.Equals(debit_column_name, StringComparison.OrdinalIgnoreCase))
                    {
                        debit_column_name = column.ColumnName;

                        tk_no = dataRow[column];
                    }

                    if (column.ColumnName.Equals(credit_column_name, StringComparison.OrdinalIgnoreCase))
                    {
                        credit_column_name = column.ColumnName;

                        tk_co = dataRow[column];
                    }

                    if (column.ColumnName.Equals(money_column_name, StringComparison.OrdinalIgnoreCase))
                    {
                        money_column_name = column.ColumnName;

                        vnd = Convert.ToDecimal(dataRow[column]);
                    }
                }

                if (vnd > 0)
                {
                    var strTkCo = tk_co?.ToString();
                    var strTkNo = tk_no?.ToString();

                    if (requireFields.Contains(credit_column_name) && (string.IsNullOrWhiteSpace(strTkCo) || int.TryParse(strTkCo, out var tk_co_id) && tk_co_id <= 0)) return credit_column_name;

                    if (requireFields.Contains(debit_column_name) && (string.IsNullOrWhiteSpace(strTkNo) || int.TryParse(strTkNo, out var tk_no_id) && tk_no_id <= 0)) return debit_column_name;
                }

            }
            return null;
        }

        private async Task DeleteBillVersion(int inputTypeId, long inputBill_F_Id, int billVersionId)
        {
            await _accountancyDBContext.ExecuteStoreProcedure("asp_InputValueRow_Delete_Version", new[] {
                    new SqlParameter("@InputTypeId", inputTypeId),
                    new SqlParameter("@InputBill_F_Id", inputBill_F_Id),
                    new SqlParameter("@BillVersion", billVersionId),
                    new SqlParameter("@UserId", _currentContextService.UserId),
                    new SqlParameter("@ResStatus", inputTypeId){ Direction = ParameterDirection.Output },
                }, true);
        }

        public async Task<List<ValidateField>> GetInputFields(int inputTypeId, int? areaId = null)
        {
            var area = _accountancyDBContext.InputArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.InputAreaId == areaId);
            }

            return await (from af in _accountancyDBContext.InputAreaField
                          join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                          join a in area on af.InputAreaId equals a.InputAreaId
                          where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly //&& f.FieldName != AccountantConstants.F_IDENTITY
                          orderby a.SortOrder, af.SortOrder
                          select new ValidateField
                          {
                              InputAreaFieldId = af.InputAreaFieldId,
                              Title = af.Title,
                              IsAutoIncrement = af.IsAutoIncrement,
                              IsHidden = af.IsHidden,
                              IsReadOnly = f.IsReadOnly,
                              IsRequire = af.IsRequire,
                              IsUnique = af.IsUnique,
                              Filters = af.Filters,
                              FieldName = f.FieldName,
                              DataTypeId = f.DataTypeId,
                              FormTypeId = f.FormTypeId,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              RegularExpression = af.RegularExpression,
                              IsMultiRow = a.IsMultiRow,
                              RequireFilters = af.RequireFilters,
                              AreaTitle = a.Title,
                          }).ToListAsync();
        }


        public async Task<CategoryNameModel> GetFieldDataForMapping(int inputTypeId, int? areaId)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);


            // Lấy thông tin field
            var fields = await GetInputFields(inputTypeId, areaId);

            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.InputTypeId,
                CategoryCode = inputTypeInfo.InputTypeCode,
                CategoryTitle = inputTypeInfo.Title,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            fields = fields
                .Where(f => !f.IsHidden && !f.IsAutoIncrement && f.FieldName != AccountantConstants.F_IDENTITY && !f.IsReadOnly)
                .ToList();

            var referTableNames = fields.Select(f => f.RefTableCode).ToList();

            var referFields = await _httpCategoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            foreach (var field in fields)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    //CategoryFieldId = field.InputAreaFieldId,
                    FieldName = field.FieldName,
                    FieldTitle = GetTitleCategoryField(field),
                    RefCategory = null,
                    IsRequired = field.IsRequire && string.IsNullOrEmpty(field.RequireFilters),
                    GroupName = field.AreaTitle,
                    DataTypeId = (EnumDataType)field.DataTypeId,
                    IsMultiRow = field.IsMultiRow
                };

                if (!string.IsNullOrWhiteSpace(field.RefTableCode))
                {
                    if (!refCategoryFields.TryGetValue(field.RefTableCode, out var refCategory))
                    {
                        throw RefTableNotFound.BadRequestFormat(field.RefTableCode);
                    }


                    fileData.RefCategory = new CategoryNameModel()
                    {
                        //CategoryId = 0,
                        CategoryCode = refCategory.FirstOrDefault()?.CategoryCode,
                        CategoryTitle = refCategory.FirstOrDefault()?.CategoryTitle,
                        IsTreeView = false,

                        Fields = GetRefFields(refCategory)
                        .Select(f => new CategoryFieldNameModel()
                        {
                            //CategoryFieldId = f.id,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = GetTitleCategoryField(f),
                            RefCategory = null,
                            IsRequired = false,

                            DataTypeId = (EnumDataType)f.DataTypeId
                        }).ToList()
                    };
                }

                result.Fields.Add(fileData);
            }

            result.Fields.Add(new CategoryFieldNameModel
            {
                FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                FieldTitle = "Cột kiểm tra",
            });

            return result;
        }



        public async Task<bool> ImportBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream)
        {
            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            var reader = new ExcelReader(stream);
            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            // Lấy thông tin field
            var fields = await GetInputFields(inputTypeId);

            // var requiredField = fields.FirstOrDefault(f => f.IsRequire && string.IsNullOrWhiteSpace(f.RequireFilters) && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

            // if (requiredField != null) throw FieldRequired.BadRequestFormat(requiredField.Title);

            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
            var referTableNames = fields.Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();
            var referFields = await _httpCategoryHelperService.GetReferFields(referTableNames, referMapingFields.Select(f => f.RefFieldName).ToList());

            var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == AccountantConstants.BILL_CODE);
            if (columnKey == null)
                throw FieldRequired.BadRequestFormat("Số chứng từ");

            var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();

            var groups = data.Rows.Select((r, i) => new ImportExcelRowModel
            {
                Data = r,
                Index = i + mapping.FromRow
            })
            .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
            .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
            .GroupBy(r => r.Data[columnKey.Column])
            .ToDictionary(g => g.Key, g => g.ToList());



            // Lấy danh sách key
            var keys = groups.Select(g => g.Key).ToList();
            var existKeys = new Dictionary<string, long>();
            if (keys.Count > 0)
            {
                // Checkin unique trong db
                var existKeySql = $"SELECT DISTINCT InputBill_F_Id, {columnKey.FieldName} FROM vInputValueRow WHERE InputTypeId = {inputTypeId} AND {columnKey.FieldName} IN (";

                List<SqlParameter> existKeyParams = new List<SqlParameter>();
                var keySuffix = 0;
                foreach (var key in keys)
                {
                    var paramName = $"@{columnKey.FieldName}_{keySuffix}";
                    if (keySuffix > 0)
                    {
                        existKeySql += ",";
                    }
                    existKeySql += paramName;
                    existKeyParams.Add(new SqlParameter(paramName, key));
                    keySuffix++;
                }
                existKeySql += ")";

                var existKeyResult = await _accountancyDBContext.QueryDataTable(existKeySql, existKeyParams.ToArray());
                if (existKeyResult != null && existKeyResult.Rows.Count > 0)
                {
                    foreach (DataRow row in existKeyResult.Rows)
                    {
                        existKeys.Add(row[columnKey.FieldName].ToString(), Convert.ToInt64(row["InputBill_F_Id"]));
                    }
                }
            }

            var createGroups = new Dictionary<string, List<ImportExcelRowModel>>();
            var updateGroups = new Dictionary<string, List<ImportExcelRowModel>>();

            // lựa chọn trùng dữ liệu là Denied
            if (existKeys.Count > 0)
            {
                switch (mapping.ImportDuplicateOptionId)
                {
                    case EnumImportDuplicateOption.Denied:
                        var errField = fields.First(f => f.FieldName == columnKey.FieldName);
                        throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { errField.Title, string.Join(", ", existKeys.Select(c => c.Key).ToArray()), "" });
                    case EnumImportDuplicateOption.Ignore:
                        createGroups = groups.Where(g => !existKeys.ContainsKey(g.Key)).ToDictionary(g => g.Key, g => g.Value);
                        break;
                    case EnumImportDuplicateOption.Update:
                        createGroups = groups.Where(g => !existKeys.ContainsKey(g.Key)).ToDictionary(g => g.Key, g => g.Value);
                        updateGroups = groups.Where(g => existKeys.ContainsKey(g.Key)).ToDictionary(g => g.Key, g => g.Value);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                createGroups = groups;
            }

            string typeTitleField = AccountantConstants.INPUT_TYPE_TITLE;

            // Validate unique field cho chứng từ tạo mới
            foreach (var field in fields.Where(f => f.IsUnique || f.FieldName == AccountantConstants.BILL_CODE))
            {
                var mappingField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                if (mappingField == null) continue;

                var values = field.IsMultiRow
                ? createGroups.SelectMany(b => b.Value.Select(r => r.Data[mappingField.Column]?.ToString())).ToList()
                : createGroups.Where(b => b.Value.Count() > 0).Select(b => b.Value.First().Data[mappingField.Column]?.ToString()).ToList();

                // Check unique trong danh sách values thêm mới
                if (values.Distinct().Count() < values.Count)
                {
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(",", values), "" });
                }
                // Checkin unique trong db
                if (values.Count == 0) continue;
                var existSql = $"SELECT F_Id, {typeTitleField}, {field.FieldName} FROM vInputValueRow WHERE InputTypeId = {inputTypeId} ";

                if (field.FieldName == AccountantConstants.BILL_CODE)//ignore bill type
                    existSql = $"SELECT F_Id, {typeTitleField}, {field.FieldName} FROM vInputValueRow WHERE 1 = 1";

                existSql += $" AND {field.FieldName} IN (";
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                var suffix = 0;
                foreach (var value in values)
                {
                    var paramName = $"@{field.FieldName}_{suffix}";
                    if (suffix > 0)
                    {
                        existSql += ",";
                    }
                    existSql += paramName;
                    sqlParams.Add(new SqlParameter(paramName, value));
                    suffix++;
                }
                existSql += ")";
                var result = await _accountancyDBContext.QueryDataTable(existSql, sqlParams.ToArray());
                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                {
                    var dupValues = new List<string>();
                    var inputType_Title = "";
                    for (var i = 0; i < result.Rows.Count; i++)
                    {
                        var value = result.Rows[i][field.FieldName]?.ToString();
                        if (!dupValues.Contains(value))
                            dupValues.Add(value);
                        if (string.IsNullOrWhiteSpace(inputType_Title))
                            inputType_Title = result.Rows[i][typeTitleField]?.ToString();
                    }
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(", ", dupValues.ToArray()), inputType_Title });
                }
            }


            var createBills = new List<BillInfoModel>();
            var updateBills = new Dictionary<long, BillInfoModel>();

            if (createGroups.Count > 0)
            {
                var requiredField = fields.FirstOrDefault(f => f.IsRequire && string.IsNullOrWhiteSpace(f.RequireFilters) && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

                if (requiredField != null) throw FieldRequired.BadRequestFormat(requiredField.Title);

                foreach (var bill in createGroups)
                {
                    var billInfo = await GetBillFromRows(bill, mapping, fields, referFields);
                    createBills.Add(billInfo);
                }
            }

            foreach (var bill in updateGroups)
            {
                var billInfo = await GetBillFromRows(bill, mapping, fields, referFields);
                if (updateBills.ContainsKey(existKeys[bill.Key]))
                {
                    updateBills[existKeys[bill.Key]] = billInfo;
                }
                else
                {
                    updateBills.Add(existKeys[bill.Key], billInfo);
                }
            }

            bool EqualityBetweenTwoNomCamel(NonCamelCaseDictionary f1, NonCamelCaseDictionary f2, ValidateField[] u)
            {
                for (int i = 0; i < u.Length; i++)
                {
                    var key = u[i].FieldName;

                    var f1Value = f1[key].ToString().ToLower();
                    var f2Value = f2[key].ToString().ToLower();
                    if (((EnumDataType)u[i].DataTypeId).CompareValue(f1Value, f2Value) != 0) return false;
                }

                return true;
            }

            // Get all fields
            var inputFields = _accountancyDBContext.InputField
             .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
             .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

            var fieldIdentityDetails = mapping.MappingFields.Where(x => fields.Where(f => f.IsMultiRow).Any(f => f.FieldName == x.FieldName) && x.IsIdentityDetail)
                          .Select(x => x.FieldName)
                          .Distinct()
                          .ToArray();

            var validateFieldInfos = fields.Where(x => fieldIdentityDetails.Contains(x.FieldName)).ToArray();


            //Check duplicate rows in details
            foreach (var bill in updateBills)
            {
                foreach (var row in bill.Value.Rows)
                {
                    var duplicateRows = bill.Value.Rows.Where(x => EqualityBetweenTwoNomCamel(x, row, validateFieldInfos)).ToList();
                    if (duplicateRows.Count > 1)
                    {
                        var oldBillInfo = await GetBillInfo(inputTypeId, bill.Key);

                        var excelRowNumbers = bill.Value.GetExcelRowNumbers();

                        var excelRowNumber = excelRowNumbers[row];

                        throw new BadRequestException(GeneralCode.InvalidParams, $"Dòng {excelRowNumber}. Định danh chi tiết chưa đúng, tìm thấy nhiều hơn 1 dòng chi tiết trong excel {oldBillInfo.Info[AccountantConstants.BILL_CODE]}");
                    }
                }
            }

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();



                    // Thêm mới chứng từ
                    foreach (var bill in createBills)
                    {
                        var excelRowsIndexs = bill.GetExcelRowNumbers();

                        // validate require
                        ValidateRowModel checkInfo = new ValidateRowModel(bill.Info, null, excelRowsIndexs?.Count > 0 ? (int?)excelRowsIndexs.First().Value : null);

                        List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
                        checkRows = bill.Rows.Select(r => new ValidateRowModel(r, null, excelRowsIndexs?.ContainsKey(r) == true ? (int?)excelRowsIndexs[r] : null)).ToList();

                        // Validate info
                        var requiredFields = fields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
                        // Check field required
                        await CheckRequired(checkInfo, checkRows, requiredFields, fields);

                        // Before saving action (SQL)
                        var result = await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, bill, inputFields, EnumActionType.Add);
                        if (result.Code != 0)
                        {
                            if (string.IsNullOrWhiteSpace(result.Message))
                                throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                            else
                            {
                                throw result.Message.BadRequest();

                            }
                        }

                        var billInfo = new InputBill()
                        {
                            InputTypeId = inputTypeId,
                            LatestBillVersion = 1,
                            SubsidiaryId = _currentContextService.SubsidiaryId,
                            IsDeleted = false
                        };

                        await _accountancyDBContext.InputBill.AddAsync(billInfo);

                        await _accountancyDBContext.SaveChangesAsync();

                        await CreateBillVersion(inputTypeId, billInfo, bill, generateTypeLastValues);

                        // After saving action (SQL)
                        await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, bill, inputFields, EnumActionType.Add);
                    }


                    // Cập nhật chứng từ
                    foreach (var bill in updateBills)
                    {
                        var oldBillInfo = await GetBillInfo(inputTypeId, bill.Key);

                        var newBillInfo = new BillInfoModel
                        {
                            Info = oldBillInfo.Info,
                            OutsideImportMappingData = oldBillInfo.OutsideImportMappingData,
                            Rows = oldBillInfo.Rows
                        };

                        foreach (var item in bill.Value.Info)
                        {
                            if (newBillInfo.Info.ContainsKey(item.Key))
                            {
                                newBillInfo.Info[item.Key] = item.Value;
                            }
                        }



                        var excelRowNumbers = bill.Value.GetExcelRowNumbers();
                        var newExcelRows = new Dictionary<NonCamelCaseDictionary, int>();
                        foreach (var row in bill.Value.Rows)
                        {
                            if (row.Count == 0) continue;

                            if (validateFieldInfos.Count() == 0)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Phải chọn cột làm định danh dòng chi tiết");

                            var existsRows = newBillInfo.Rows.Where(x => EqualityBetweenTwoNomCamel(x, row, validateFieldInfos)).ToList();

                            var excelRowNumber = excelRowNumbers[row];

                            if (existsRows.Count > 1)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Dòng {excelRowNumber}. Định danh chi tiết chưa đúng, tìm thấy nhiều hơn 1 dòng chi tiết trong chứng từ {oldBillInfo.Info[AccountantConstants.BILL_CODE]}");


                            if (existsRows.Count == 0)
                            {
                                newBillInfo.Rows.Add(row);
                                newExcelRows.Add(row, excelRowNumber);
                            }
                            else
                            {
                                var existsRow = existsRows.First();
                                if (!newExcelRows.ContainsKey(existsRow))
                                {
                                    newExcelRows.Add(existsRow, excelRowNumber);
                                }
                                else
                                {
                                    throw new BadRequestException(GeneralCode.InvalidParams, $"Dòng {excelRowNumber}. Định danh chi tiết chưa đúng, tìm thấy nhiều hơn 1 dòng chi tiết trong excel {oldBillInfo.Info[AccountantConstants.BILL_CODE]}");
                                }

                                foreach (var item in row)
                                {
                                    if (existsRow.ContainsKey(item.Key))
                                    {
                                        existsRow[item.Key] = item.Value;
                                    }
                                }
                            }

                        }

                        // Get changed info
                        var singleFields = fields.Where(f => !f.IsMultiRow).ToList();

                        await ValidateAccountantConfig(newBillInfo.Info, oldBillInfo.Info);

                        NonCamelCaseDictionary futureInfo = newBillInfo.Info;


                        ValidateRowModel checkInfo = new ValidateRowModel(newBillInfo.Info, CompareRow(oldBillInfo.Info, futureInfo, singleFields), newExcelRows?.Count > 0 ? (int?)newExcelRows.First().Value : null);

                        // Get changed rows
                        List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
                        var multiFields = fields.Where(f => f.IsMultiRow).ToList();
                        foreach (var futureRow in newBillInfo.Rows)
                        {
                            futureRow.TryGetValue("F_Id", out string futureValue);
                            NonCamelCaseDictionary curRow = oldBillInfo.Rows.FirstOrDefault(r => futureValue != null && r["F_Id"].ToString() == futureValue);

                            var exelRow = newExcelRows?.ContainsKey(futureRow) == true ? (int?)newExcelRows[futureRow] : null;

                            if (curRow == null)
                            {
                                checkRows.Add(new ValidateRowModel(futureRow, null, exelRow));
                            }
                            else
                            {
                                string[] changeFieldIndexes = CompareRow(curRow, futureRow, multiFields);
                                checkRows.Add(new ValidateRowModel(futureRow, changeFieldIndexes, exelRow));
                            }
                        }

                        // Lấy thông tin field
                        var requiredFields = fields.Where(f => !f.IsAutoIncrement && f.IsRequire && (f.IsMultiRow || f.FieldName == AccountantConstants.BILL_CODE)).ToList();
                        var uniqueFields = fields.Where(f => (!f.IsAutoIncrement && f.IsUnique) || f.FieldName == AccountantConstants.BILL_CODE).ToList();
                        var selectFields = fields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

                        // Check field required
                        await CheckRequired(checkInfo, checkRows, requiredFields, fields);

                        // Check unique
                        await CheckUniqueAsync(inputTypeId, checkInfo, checkRows, uniqueFields, bill.Key);

                        var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.InputTypeId == inputTypeId && b.FId == bill.Key && b.SubsidiaryId == _currentContextService.SubsidiaryId);
                        if (billInfo == null) throw BillNotFound.BadRequest();

                        // Before saving action (SQL)
                        var result = await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, newBillInfo, inputFields, EnumActionType.Update, billInfo.FId);
                        if (result.Code != 0)
                        {
                            if (string.IsNullOrWhiteSpace(result.Message))
                                throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                            else
                            {
                                throw result.Message.BadRequest();

                            }
                        }
                        await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);

                        billInfo.LatestBillVersion++;

                        await CreateBillVersion(inputTypeId, billInfo, newBillInfo, generateTypeLastValues);

                        await _accountancyDBContext.SaveChangesAsync();

                        // After saving action (SQL)
                        await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, newBillInfo, inputFields, EnumActionType.Update, billInfo.FId);
                    }

                    await ConfirmCustomGenCode(generateTypeLastValues);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.TryRollbackTransaction();
                    _logger.LogError(ex, "Import");
                    throw;
                }
            }
            return true;
        }


        private async Task<BillInfoModel> GetBillFromRows(KeyValuePair<string, List<ImportExcelRowModel>> bill, ImportExcelMapping mapping, List<ValidateField> fields, List<ReferFieldModel> referFields)
        {
            var info = new NonCamelCaseDictionary();
            var rows = new List<NonCamelCaseDictionary>();
            int count = bill.Value.Count();

            var rowIndexs = new Dictionary<NonCamelCaseDictionary, int>();
            for (int rowIndx = 0; rowIndx < count; rowIndx++)
            {
                var mapRow = new NonCamelCaseDictionary();
                var row = bill.Value.ElementAt(rowIndx);
                foreach (var mappingField in mapping.MappingFields)
                {
                    var field = fields.FirstOrDefault(f => f.FieldName == mappingField.FieldName);

                    // Validate mapping required
                    if (field == null && mappingField.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty)
                    {
                        throw FieldNameNotFound.BadRequestFormat(mappingField.FieldName);
                    }

                    if (field == null) continue;
                    if (!field.IsMultiRow && rowIndx > 0 && info.ContainsKey(field.FieldName)) continue;

                    string value = null;
                    if (row.Data.ContainsKey(mappingField.Column))
                        value = row.Data[mappingField.Column]?.ToString();


                    if (string.IsNullOrWhiteSpace(value)) continue;
                    value = value.Trim();

                    if (value.StartsWith(PREFIX_ERROR_CELL))
                    {
                        throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(row.Index, mappingField.Column, $"\"{field.Title}\" {value}");
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)field.DataTypeId))
                    {
                        if (!DateTime.TryParse(value.ToString(), out DateTime date))
                            throw CannotConvertValueInRowFieldToDateTime.BadRequestFormat(value?.JsonSerialize(), row.Index, field.Title);
                        value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix().ToString();
                    }

                    // Validate refer
                    if (!((EnumFormType)field.FormTypeId).IsSelectForm())
                    {
                        // Validate value
                        if (!field.IsAutoIncrement && !string.IsNullOrEmpty(value))
                        {
                            string regex = ((EnumDataType)field.DataTypeId).GetRegex();
                            if ((field.DataSize > 0 && value.Length > field.DataSize)
                                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
                            {
                                throw new BadRequestException(InputErrorCode.InputValueInValid, new object[] { value?.JsonSerialize(), row.Index, field.Title });
                            }
                        }
                    }
                    else
                    {
                        int suffix = 0;
                        var paramName = $"@{mappingField.RefFieldName}_{suffix}";
                        var referField = referFields.FirstOrDefault(f => f.CategoryCode == field.RefTableCode && f.CategoryFieldName == mappingField.RefFieldName);
                        if (referField == null)
                        {
                            throw RefFieldNotExisted.BadRequestFormat(field.Title, mappingField.FieldName);
                        }
                        var referSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                        var referParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                        suffix++;
                        if (!string.IsNullOrEmpty(field.Filters))
                        {
                            var filters = field.Filters;
                            var pattern = @"@{(?<word>\w+)}\((?<start>\d*),(?<length>\d*)\)";
                            Regex rx = new Regex(pattern);
                            MatchCollection match = rx.Matches(field.Filters);

                            for (int i = 0; i < match.Count; i++)
                            {
                                var fieldName = match[i].Groups["word"].Value;
                                var startText = match[i].Groups["start"].Value;
                                var lengthText = match[i].Groups["length"].Value;
                                mapRow.TryGetValue(fieldName, out string filterValue);
                                if (string.IsNullOrEmpty(filterValue))
                                {
                                    info.TryGetValue(fieldName, out filterValue);
                                }


                                if (!string.IsNullOrWhiteSpace(filterValue) && !string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                                {
                                    if (filterValue.Length < start)
                                    {
                                        //TODO: Validate message
                                        throw new BadRequestException($"Invalid value sustring {filterValue} start {start}, length {length}");
                                    }
                                    filterValue = filterValue.Substring(start, length);
                                }


                                if (string.IsNullOrEmpty(filterValue))
                                {
                                    var beforeField = fields?.FirstOrDefault(f => f.FieldName == fieldName)?.Title;
                                    throw RequireFieldBeforeField.BadRequestFormat(beforeField, field.Title);
                                }
                                filters = filters.Replace(match[i].Value, filterValue);
                            }

                            Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                            if (filterClause != null)
                            {
                                var whereCondition = new StringBuilder();
                                filterClause.FilterClauseProcess($"v{field.RefTableCode}", $"v{field.RefTableCode}", ref whereCondition, ref referParams, ref suffix);
                                if (whereCondition.Length > 0) referSql += $" AND {whereCondition.ToString()}";
                            }
                        }

                        var referData = await _accountancyDBContext.QueryDataTable(referSql, referParams.ToArray());
                        if (referData == null || referData.Rows.Count == 0)
                        {
                            // Check tồn tại
                            var checkExistedReferSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                            var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                            referData = await _accountancyDBContext.QueryDataTable(checkExistedReferSql, checkExistedReferParams.ToArray());
                            if (referData == null || referData.Rows.Count == 0)
                            {
                                throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { row.Index, field.Title + ": " + value });
                            }
                            else
                            {
                                throw new BadRequestException(InputErrorCode.ReferValueNotValidFilter, new object[] { row.Index, field.Title + ": " + value });
                            }
                        }
                        value = referData.Rows[0][field.RefTableField]?.ToString() ?? string.Empty;
                    }
                    if (!field.IsMultiRow)
                    {
                        info.Add(field.FieldName, value);
                    }
                    else
                    {
                        mapRow.Add(field.FieldName, value);
                    }
                }

                rowIndexs.Add(mapRow, row.Index);
                if (mapRow.Count > 0)
                    rows.Add(mapRow);
            }
            var billInfo = new BillInfoModel
            {
                Info = info,
                Rows = rows.ToArray()
            };

            billInfo.SetExcelRowNumbers(rowIndexs);

            foreach (var (fieldName, v) in billInfo.Info)
            {
                var value = v?.ToString();
                var field = fields.FirstOrDefault(f => f.FieldName == fieldName);

                // Validate require
                if (string.IsNullOrWhiteSpace(value) && field.IsRequire && string.IsNullOrWhiteSpace(field.RequireFilters))
                    throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { bill.Value.First().Index, field.Title });
            }

            for (var i = 0; i < billInfo.Rows.Count; i++)
            {

                foreach (var (fieldName, v) in billInfo.Rows[i])
                {
                    var value = v?.ToString();
                    var field = fields.FirstOrDefault(f => f.FieldName == fieldName);

                    // Validate require
                    if (string.IsNullOrWhiteSpace(value) && field.IsRequire && string.IsNullOrWhiteSpace(field.RequireFilters))
                        throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { rowIndexs[billInfo.Rows[i]], field.Title });
                }
            }
            // await ValidateAccountantConfig(billInfo?.Info, null);

            return billInfo;
        }

        public async Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId)
        {

            var dataSql = @$"
                SELECT     r.*
                FROM {INPUTVALUEROW_VIEW} r 
                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 0
            ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());
            var billEntryInfoSql = $"SELECT r.* FROM { INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 1";
            var billEntryInfo = await _accountancyDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

            var info = (billEntryInfo.Rows.Count > 0 ? billEntryInfo.ConvertFirstRowData() : data.ConvertFirstRowData()).ToNonCamelCaseDictionary();
            var rows = data.ConvertData();

            var inputType = _accountancyDBContext.InputType
                .Include(i => i.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(f => f.InputField)
                .Where(i => i.InputTypeId == inputTypeId)
                .FirstOrDefault();

            if (inputType == null) throw new BadRequestException(InputErrorCode.InputTypeNotFound);

            //var refDataTypes = (from iaf in _accountancyDBContext.InputAreaField.Where(iaf => iaf.InputTypeId == inputTypeId)
            //                    join itf in _accountancyDBContext.InputField on iaf.InputFieldId equals itf.InputFieldId
            //                    join c in _accountancyDBContext.Category on itf.RefTableCode equals c.CategoryCode
            //                    join f in _accountancyDBContext.CategoryField on c.CategoryId equals f.CategoryId
            //                    where itf.RefTableTitle.StartsWith(f.CategoryFieldName) && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)itf.FormTypeId)
            //                    select new
            //                    {
            //                        f.CategoryFieldName,
            //                        f.DataTypeId,
            //                        c.CategoryCode
            //                    }).Distinct()
            //                    .ToDictionary(f => new { f.CategoryFieldName, f.CategoryCode }, f => (EnumDataType)f.DataTypeId);

            var selectFormFields = (from iaf in _accountancyDBContext.InputAreaField
                                    join itf in _accountancyDBContext.InputField on iaf.InputFieldId equals itf.InputFieldId
                                    where iaf.InputTypeId == inputTypeId && DataTypeConstants.SELECT_FORM_TYPES.Contains((EnumFormType)itf.FormTypeId)
                                    select new
                                    {
                                        itf.RefTableTitle,
                                        itf.RefTableCode
                                    }).ToList();

            var refDataTypes = (await _httpCategoryHelperService.GetReferFields(selectFormFields.Select(f => f.RefTableCode).ToList(), selectFormFields.Select(f => f.RefTableTitle.Split(',')[0]).ToList()))
                .Distinct().ToDictionary(f => new { f.CategoryFieldName, f.CategoryCode }, f => (EnumDataType)f.DataTypeId);

            var writer = new ExcelWriter();
            int endRow = 0;

            var billCode = string.Empty;
            // Write area
            foreach (var area in inputType.InputArea.OrderBy(a => a.SortOrder))
            {
                ExcelData table = new ExcelData();
                if (!area.IsMultiRow)
                {
                    // Write info
                    for (int collumIndx = 0; collumIndx < area.Columns; collumIndx++)
                    {
                        table.AddColumn();
                        table.AddColumn();
                        int rowIndx = 0;
                        foreach (var field in area.InputAreaField.Where(f => f.Column == (collumIndx + 1)).OrderBy(f => f.SortOrder))
                        {
                            ExcelRow row;
                            if (table.Rows.Count <= rowIndx)
                            {
                                row = table.NewRow();
                                table.Rows.Add(row);
                            }
                            else
                            {
                                row = table.Rows[rowIndx];
                            }
                            row[collumIndx * 2] = new ExcelCell
                            {
                                Value = $"{field.Title}:",
                                Type = EnumExcelType.String
                            };
                            var fieldName = ((EnumFormType)field.InputField.FormTypeId).IsJoinForm() ? $"{field.InputField.FieldName}_{field.InputField.RefTableTitle.Split(",")[0]}" : field.InputField.FieldName;
                            var dataType = ((EnumFormType)field.InputField.FormTypeId).IsJoinForm() ? refDataTypes[new { CategoryFieldName = field.InputField.RefTableTitle.Split(",")[0], CategoryCode = field.InputField.RefTableCode }] : (EnumDataType)field.InputField.DataTypeId;
                            if (info.ContainsKey(fieldName))
                                row[collumIndx * 2 + 1] = new ExcelCell
                                {
                                    Value = dataType.GetSqlValue(info[fieldName]),
                                    Type = dataType.GetExcelType()
                                };
                            rowIndx++;
                        }
                    }

                    var uniqField = area.InputAreaField.FirstOrDefault(f => f.IsUnique)?.InputField.FieldName ?? AccountantConstants.BILL_CODE;
                    info.TryGetValue(uniqField, out billCode);
                }
                else
                {
                    foreach (var field in area.InputAreaField.OrderBy(f => f.SortOrder))
                    {
                        table.Columns.Add(field.Title);
                    }
                    var sumCalc = new List<int>();
                    foreach (var row in rows)
                    {
                        ExcelRow tbRow = table.NewRow();
                        int columnIndx = 0;
                        foreach (var field in area.InputAreaField.OrderBy(f => f.SortOrder))
                        {
                            if (field.IsCalcSum) sumCalc.Add(columnIndx);
                            var fieldName = ((EnumFormType)field.InputField.FormTypeId).IsJoinForm() ? $"{field.InputField.FieldName}_{field.InputField.RefTableTitle.Split(",")[0]}" : field.InputField.FieldName;
                            var dataType = ((EnumFormType)field.InputField.FormTypeId).IsJoinForm() ? refDataTypes[new { CategoryFieldName = field.InputField.RefTableTitle.Split(",")[0], CategoryCode = field.InputField.RefTableCode }] : (EnumDataType)field.InputField.DataTypeId;
                            if (row.ContainsKey(fieldName))
                                tbRow[columnIndx] = new ExcelCell
                                {
                                    Value = dataType.GetSqlValue(row[fieldName]),
                                    Type = dataType.GetExcelType()
                                };
                            columnIndx++;
                        }
                        table.Rows.Add(tbRow);
                    }
                    if (sumCalc.Count > 0)
                    {
                        ExcelRow sumRow = table.NewRow();
                        foreach (int columnIndx in sumCalc)
                        {
                            var columnName = (columnIndx + 1).GetExcelColumnName();
                            sumRow[columnIndx] = new ExcelCell
                            {
                                Value = $"SUM({columnName}{endRow + 3}:{columnName}{endRow + rows.Count + 2})",
                                Type = EnumExcelType.Formula
                            };
                        }
                        table.Rows.Add(sumRow);
                    }
                }

                byte[] headerRgb = new byte[3] { 60, 120, 216 };

                writer.WriteToSheet(table, "Data", out endRow, area.IsMultiRow, headerRgb, 0, endRow + 1);
            }

            var fileName = $"{inputType.InputTypeCode}_{billCode}.xlsx";

            MemoryStream stream = await writer.WriteToStream();
            return (stream, fileName);
        }

        public async Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow)
        {
            var inputReferToFields = _accountancyDBContext.InputField
                .Where(f => f.RefTableCode == categoryCode && fieldNames.Contains(f.RefTableField)).ToList();

            if (categoryRow == null)
            {
                // Check khi xóa cả danh mục
                return _accountancyDBContext.InputField.Any(f => f.RefTableCode == categoryCode);
            }
            else
            {
                // Check khi xóa dòng trong danh mục
                // check bill refer
                foreach (var field in fieldNames)
                {
                    categoryRow.TryGetValue(field, out object value);
                    if (value == null) continue;
                    foreach (var referToField in inputReferToFields.Where(f => f.RefTableField == field))
                    {
                        var referToValue = new SqlParameter("@RefValue", value?.ToString());
                        var existSql = $"SELECT tk.F_Id FROM {INPUTVALUEROW_VIEW} tk WHERE tk.{referToField.FieldName} = @RefValue;";
                        var result = await _accountancyDBContext.QueryDataTable(existSql, new[] { referToValue });
                        bool isExisted = result != null && result.Rows.Count > 0;
                        if (isExisted)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId)
        {
            var sql = $"SELECT DISTINCT v.InputTypeId ObjectTypeId, v.InputBill_F_Id ObjectBill_F_Id, v.so_ct ObjectBillCode FROM {INPUTVALUEROW_TABLE} v WHERE (v.CensorStatusId IS NULL OR  v.CensorStatusId <> {(int)EnumCensorStatus.Approved}) AND v.InputTypeId = @InputTypeId AND v.IsDeleted = 0";

            return (await _accountancyDBContext.QueryDataTable(sql, new[] { new SqlParameter("@InputTypeId", inputTypeId) }))
                    .ConvertData<ObjectBillSimpleInfoModel>()
                    .ToList();
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            var sql = $"SELECT DISTINCT v.InputTypeId ObjectTypeId, v.InputBill_F_Id ObjectBill_F_Id, v.so_ct ObjectBillCode FROM {INPUTVALUEROW_TABLE} v WHERE (v.CheckStatusId IS NULL OR  v.CheckStatusId <> {(int)EnumCheckStatus.CheckedSuccess}) AND v.InputTypeId = @InputTypeId AND v.IsDeleted = 0";

            return (await _accountancyDBContext.QueryDataTable(sql, new[] { new SqlParameter("@InputTypeId", inputTypeId) }))
                    .ConvertData<ObjectBillSimpleInfoModel>()
                    .ToList();
        }

        public async Task<bool> CheckAllBillInList(IList<ObjectBillSimpleInfoModel> models)
        {
            if (models.Count > 0)
            {
                var sql = $"UPDATE {INPUTVALUEROW_TABLE} SET CheckStatusId = {(int)EnumCheckStatus.CheckedSuccess} WHERE InputBill_F_Id IN (";
                var sqlParams = new List<SqlParameter>();
                var prefixColumn = "@InputBill_F_Id_";
                foreach (var item in models.Select((item, index) => new { item, index }))
                {
                    if (item.index > 0)
                        sql += ", ";
                    sql += prefixColumn + $"{item.index}";
                    sqlParams.Add(new SqlParameter(prefixColumn + $"{item.index}", item.item.ObjectBill_F_Id));
                }
                sql += ")";

                await _accountancyDBContext.Database.ExecuteSqlRawAsync(sql, sqlParams);
            }
            return true;
        }

        public async Task<bool> ApproveAllBillInList(IList<ObjectBillSimpleInfoModel> models)
        {
            if (models.Count > 0)
            {
                var sql = $"UPDATE {INPUTVALUEROW_TABLE} SET CensorStatusId = {(int)EnumCensorStatus.Approved} WHERE InputBill_F_Id IN (";
                var sqlParams = new List<SqlParameter>();
                var prefixColumn = "@InputBill_F_Id_";
                foreach (var item in models.Select((item, index) => new { item, index }))
                {
                    if (item.index > 0)
                        sql += ", ";
                    sql += prefixColumn + $"{item.index}";
                    sqlParams.Add(new SqlParameter(prefixColumn + $"{item.index}", item.item.ObjectBill_F_Id));
                }
                sql += ")";

                await _accountancyDBContext.Database.ExecuteSqlRawAsync(sql, sqlParams);
            }
            return true;
        }

        private object ExtractBillDate(NonCamelCaseDictionary info)
        {
            object oldDateValue = null;

            info?.TryGetValue(AccountantConstants.BILL_DATE, out oldDateValue);
            return EnumDataType.Date.GetSqlValue(oldDateValue);
        }


        private async Task ValidateAccountantConfig(NonCamelCaseDictionary info, NonCamelCaseDictionary oldInfo)
        {
            var billDate = ExtractBillDate(info);
            var oldDate = ExtractBillDate(oldInfo);

            await ValidateAccountantConfig(billDate, oldDate);
        }

        private async Task ValidateAccountantConfig(object billDate, object oldDate)
        {
            if (billDate != null || oldDate != null)
            {
                var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
                var sqlParams = new List<SqlParameter>
                {
                    result
                };
                sqlParams.Add(new SqlParameter("@OldDate", SqlDbType.DateTime2) { Value = oldDate });
                sqlParams.Add(new SqlParameter("@BillDate", SqlDbType.DateTime2) { Value = billDate });
                await _accountancyDBContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw BillDateMustBeGreaterThanClosingDate.BadRequest();
            }
        }

        private string GlobalFilter()
        {
            return $"r.SubsidiaryId = { _currentContextService.SubsidiaryId}";
        }

        private IList<ReferFieldModel> GetRefFields(IList<ReferFieldModel> fields)
        {
            return fields.Where(x => !x.IsHidden && x.DataTypeId != (int)EnumDataType.Boolean && !((EnumDataType)x.DataTypeId).IsTimeType())
                 .ToList();
        }

        private string GetTitleCategoryField(ValidateField field)
        {
            var rangeValue = ((EnumDataType)field.DataTypeId).GetRangeValue();
            if (rangeValue.Length > 0)
            {
                return $"{field.Title} ({string.Join(", ", ((EnumDataType)field.DataTypeId).GetRangeValue())})";
            }

            return field.Title;
        }

        private string GetTitleCategoryField(ReferFieldModel field)
        {
            var rangeValue = ((EnumDataType)field.DataTypeId).GetRangeValue();
            if (rangeValue.Length > 0)
            {
                return $"{field.CategoryFieldTitle} ({string.Join(", ", ((EnumDataType)field.DataTypeId).GetRangeValue())})";
            }

            return field.CategoryFieldTitle;
        }

        protected class DataEqualityComparer : IEqualityComparer<object>
        {
            private readonly EnumDataType dataType;

            public DataEqualityComparer(EnumDataType dataType)
            {
                this.dataType = dataType;
            }

            public new bool Equals(object x, object y)
            {
                return dataType.CompareValue(x, y) == 0;
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

        protected class ValidateRowModel
        {
            public NonCamelCaseDictionary Data { get; set; }
            public string[] CheckFields { get; set; }

            public int? ExcelRow { get; set; }
            public ValidateRowModel(NonCamelCaseDictionary data, string[] checkFields, int? excelRow)
            {
                this.Data = data;
                this.CheckFields = checkFields;
                this.ExcelRow = excelRow;
            }
        }

        private class ImportExcelRowModel
        {
            public NonCamelCaseDictionary<string> Data { get; set; }
            public int Index { get; set; }
        }

        public class ValidateField
        {
            public int InputAreaFieldId { get; set; }
            public string Title { get; set; }
            public bool IsAutoIncrement { get; set; }
            public bool IsHidden { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsRequire { get; set; }
            public bool IsUnique { get; set; }
            public string Filters { get; set; }
            public string FieldName { get; set; }
            public int DataTypeId { get; set; }
            public int FormTypeId { get; set; }
            public int DataSize { get; set; }
            public string RefTableCode { get; set; }
            public string RefTableField { get; set; }
            public string RefTableTitle { get; set; }
            public string RegularExpression { get; set; }
            public bool IsMultiRow { get; set; }
            public string RequireFilters { get; set; }

            public string AreaTitle { get; set; }
        }
    }
}
