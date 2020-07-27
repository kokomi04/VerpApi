﻿using AutoMapper;
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

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputDataService : IInputDataService
    {
        private const string INPUTVALUEROW_TABLE = AccountantConstants.INPUTVALUEROW_TABLE;
        private const string INPUTVALUEROW_VIEW = AccountantConstants.INPUTVALUEROW_VIEW;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IOutsideImportMappingService _outsideImportMappingService;

        public InputDataService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , IOutsideImportMappingService outsideImportMappingService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _outsideImportMappingService = outsideImportMappingService;
        }


        public async Task<PageDataTable> GetBills(int inputTypeId, string keyword, Dictionary<int, object> filters, string orderByFieldName, bool asc, int page, int size)
        {
            var viewInfo = await _accountancyDBContext.InputTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var inputTypeViewId = viewInfo?.InputTypeViewId;

            var fields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                select new { a.InputAreaId, af.InputAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.FormTypeId, f.DataTypeId, a.IsMultiRow }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);

            var viewFields = await (
                from f in _accountancyDBContext.InputTypeViewField
                where f.InputTypeViewId == inputTypeViewId
                select f
            ).ToListAsync();

            var whereCondition = new StringBuilder();

            whereCondition.Append($"r.InputTypeId = {inputTypeId}");

            var sqlParams = new List<SqlParameter>();

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.InputTypeViewFieldId == filter.Key);
                    if (viewField == null) continue;

                    var value = filter.Value;

                    if (value.IsNullObject()) continue;

                    if ((EnumDataType)viewField.DataTypeId == EnumDataType.Date)
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

                            int suffix = 0;
                            filterClause.FilterClauseProcess("r", ref whereCondition, ref sqlParams, ref suffix, false, ((EnumDataType)viewField.DataTypeId).GetSqlValue(value));
                        }
                    }
                }
            }

            var mainColumns = fields.Values.Where(f => !f.IsMultiRow).SelectMany(f =>
            {
                var refColumns = new List<string>()
                {
                    f.FieldName
                };

                if (AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)
                && !string.IsNullOrWhiteSpace(f.RefTableTitle)
                && !string.IsNullOrWhiteSpace(f.RefTableTitle))
                {
                    refColumns.AddRange(f.RefTableTitle.Split(',').Select(c => f.FieldName + "_" + c.Trim()));
                }
                return refColumns;
            }).ToList();

            if (!mainColumns.Contains(orderByFieldName))
            {
                orderByFieldName = "F_Id";
                asc = false;
            }

            var totalSql = @$"SELECT COUNT(DISTINCT r.InputBill_F_Id) as Total FROM {INPUTVALUEROW_VIEW} r WHERE {whereCondition}";

            var table = await _accountancyDBContext.QueryDataTable(totalSql, sqlParams.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            var selectColumn = string.Join(",", mainColumns.Select(c => $"r.[{c}]"));

            var dataSql = @$"
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

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
                ";
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
               select f.FieldName
          ).ToListAsync()
          ).ToHashSet();

            var totalSql = @$"SELECT COUNT(0) as Total FROM {INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 0";

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

                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 0

                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
            ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, new SqlParameter[0]);

            var billEntryInfoSql = $"SELECT r.* FROM { INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 1";

            var billEntryInfo = await _accountancyDBContext.QueryDataTable(billEntryInfoSql, new SqlParameter[0]);

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
               select f.FieldName
          ).ToListAsync()
          ).ToHashSet();

            var result = new BillInfoModel();

            var dataSql = @$"

                SELECT     r.*
                FROM {INPUTVALUEROW_VIEW} r 

                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 0
            ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, new SqlParameter[0]);

            var billEntryInfoSql = $"SELECT r.* FROM { INPUTVALUEROW_VIEW} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 1";

            var billEntryInfo = await _accountancyDBContext.QueryDataTable(billEntryInfoSql, new SqlParameter[0]);

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


        public async Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId)
        {
            var mappingInfo = await _outsideImportMappingService.MappingObjectInfo(mappingFunctionKey, objectId);
            if (mappingInfo == null)
            {
                return null;
            }
            return await GetBillInfoRows(mappingInfo.InputTypeId, mappingInfo.InputBillFId, "", false, 1, int.MaxValue);

        }

        public async Task<long> CreateBill(int inputTypeId, BillInfoModel data)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0)
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            // Lấy thông tin field
            var inputAreaFields = await GetInputFields(inputTypeId);
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, null);

            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            checkRows = data.Rows.Select(r => new ValidateRowModel(r, null)).ToList();

            // Validate info
            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, inputAreaFields);
            // Check refer
            await CheckReferAsync(checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkInfo, checkRows, uniqueFields);
            // Check value
            await CheckValue(checkInfo, checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Before saving action (SQL)
                var result = await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields, EnumAction.Add);

                if (result != 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Thông tin chứng từ không hợp lệ. Mã lỗi {result}");
                }

                var billInfo = new InputBill()
                {
                    InputTypeId = inputTypeId,
                    LatestBillVersion = 1,
                    IsDeleted = false
                };
                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                await _accountancyDBContext.SaveChangesAsync();

                var areaFieldGenCodes = new Dictionary<int, CustomGenCodeOutputModelOut>();

                await CreateBillVersion(inputTypeId, billInfo.FId, 1, data, areaFieldGenCodes);

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields, EnumAction.Add);

                if (!string.IsNullOrWhiteSpace(data?.OutsideImportMappingData?.MappingFunctionKey))
                {
                    await _outsideImportMappingService.MappingObjectCreate(data.OutsideImportMappingData.MappingFunctionKey, data.OutsideImportMappingData.ObjectId, billInfo.FId);
                }

                await ConfirmCustomGenCode(areaFieldGenCodes);

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Thêm chứng từ {inputTypeInfo.Title}", data.JsonSerialize());
                return billInfo.FId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateBill");
                throw;
            }
        }

        private async Task<bool> CheckRequireFilter(Clause clause, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> inputAreaFields, int? rowIndex, bool not = false)
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
                        var rowValues = rows.Select(r => r.Data.ContainsKey(field.FieldName) ? r.Data[field.FieldName] : null).ToList();
                        switch (singleClause.Operator)
                        {
                            case EnumOperator.Equal:
                                isRequire = rowValues.Any(v => v == singleClause.Value);
                                break;
                            case EnumOperator.NotEqual:
                                isRequire = rowValues.Any(v => v != singleClause.Value);
                                break;
                            case EnumOperator.Contains:
                                isRequire = rowValues.Any(v => v != null && v.ToString().Contains(singleClause.Value.ToString()));
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
                                isRequire = rowValues.Any(v => v != null && v.ToString().StartsWith(singleClause.Value.ToString()));
                                break;
                            case EnumOperator.EndsWith:
                                isRequire = rowValues.Any(v => v != null && v.ToString().EndsWith(singleClause.Value.ToString()));
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

                        switch (singleClause.Operator)
                        {
                            case EnumOperator.Equal:
                                isRequire = value == singleClause.Value;
                                break;
                            case EnumOperator.NotEqual:
                                isRequire = value != singleClause.Value;
                                break;
                            case EnumOperator.Contains:
                                isRequire = value != null && value.ToString().Contains(singleClause.Value.ToString());
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
                                isRequire = value != null && value.ToString().StartsWith(singleClause.Value.ToString());
                                break;
                            case EnumOperator.EndsWith:
                                isRequire = value != null && value.ToString().EndsWith(singleClause.Value.ToString());
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
                        bool clauseResult = await CheckRequireFilter(arrClause.Rules.ElementAt(indx), info, rows, inputAreaFields, rowIndex, isNot);
                        isRequire = isRequire.HasValue ? isOr ? isRequire.Value || clauseResult : isRequire.Value && clauseResult : clauseResult;
                    }
                }
            }
            return true;
        }


        private async Task<int> ProcessActionAsync(string script, BillInfoModel data, List<ValidateField> fields, EnumAction action)
        {
            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            if (!string.IsNullOrEmpty(script))
            {
                var parammeters = new List<SqlParameter>() {
                    new SqlParameter("@Action", (int)action),
                    resultParam
                };
                var pattern = @"@{(?<word>\w+)}";
                Regex rx = new Regex(pattern);
                var match = rx.Matches(script).Select(m => m.Groups["word"].Value).Distinct().ToList();

                for (int i = 0; i < match.Count; i++)
                {
                    var fieldName = match[i];
                    var field = fields.First(f => f.FieldName == fieldName);
                    if (!field.IsMultiRow)
                    {
                        var paramName = $"@{match[i]}";
                        script = script.Replace($"@{{{match[i]}}}", paramName);
                        data.Info.TryGetValue(fieldName, out string value);
                        parammeters.Add(new SqlParameter(paramName, value.ConvertValueByType((EnumDataType)field.DataTypeId) ?? DBNull.Value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() });
                    }
                    else
                    {
                        var paramNames = new List<string>();
                        for (int rowIndx = 0; rowIndx < data.Rows.Count; rowIndx++)
                        {
                            var paramName = $"@{match[i]}_{rowIndx}";
                            paramNames.Add($"({paramName})");
                            data.Rows[rowIndx].TryGetValue(fieldName, out string value);
                            parammeters.Add(new SqlParameter(paramName, value.ConvertValueByType((EnumDataType)field.DataTypeId) ?? DBNull.Value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() });
                        }
                        var valueParams = paramNames.Count > 0 ? $"VALUES {string.Join(",", paramNames)}" : "SELECT TOP 0 1";
                        script = script.Replace($"@{{{match[i]}}}", $"( {valueParams}) {match[i]}(value)");
                    }
                }
                await _accountancyDBContext.Database.ExecuteSqlRawAsync(script, parammeters);
            }
            return (resultParam.Value as int?).GetValueOrDefault();
        }

        private async Task CheckRequired(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> requiredFields, List<ValidateField> inputAreaFields)
        {
            foreach (var field in requiredFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    if (info.CheckFields != null && !info.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(field.RequireFilters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(field.RequireFilters);
                        if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, inputAreaFields, null)))
                        {
                            continue;
                        }
                    }

                    info.Data.TryGetValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { "thông tin chung", field.Title });
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
                            if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, inputAreaFields, rowIndx - 1)))
                            {
                                continue;
                            }
                        }

                        row.Data.TryGetValue(field.FieldName, out string value);
                        if (string.IsNullOrEmpty(value))
                        {
                            throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { rowIndx, field.Title });
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
                        await ValidUniqueAsync(inputTypeId, new List<object>() { value.ToString().ConvertValueByType((EnumDataType)field.DataTypeId) }, field, inputValueBillId);
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
                            values.Add(value.ToString().ConvertValueByType((EnumDataType)field.DataTypeId));
                        }
                        // Check unique trong danh sách values thêm mới/sửa
                        if (values.Count != values.Distinct().Count())
                        {
                            throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
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
            var existSql = $"SELECT F_Id FROM vInputValueRow WHERE InputTypeId = {inputTypeId} ";
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
                throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
            }
        }

        private async Task CheckReferAsync(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    await ValidReferAsync(info, info, field, null);
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        await ValidReferAsync(row, info, field, rowIndx);
                    }
                }
            }
        }

        private async Task ValidReferAsync(ValidateRowModel checkData, ValidateRowModel info, ValidateField field, int? rowIndex)
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
            var value = textValue.ConvertValueByType((EnumDataType)field.DataTypeId);
            var whereCondition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            int suffix = 0;
            var paramName = $"@{field.RefTableField}_{suffix}";
            var existSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
            sqlParams.Add(new SqlParameter(paramName, value));
            if (!string.IsNullOrEmpty(field.Filters))
            {
                var filters = field.Filters;
                var pattern = @"@{(?<word>\w+)}";
                Regex rx = new Regex(pattern);
                MatchCollection match = rx.Matches(field.Filters);
                for (int i = 0; i < match.Count; i++)
                {
                    var fieldName = match[i].Groups["word"].Value;
                    checkData.Data.TryGetValue(fieldName, out string filterValue);
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        info.Data.TryGetValue(fieldName, out filterValue);
                    }
                    filters = filters.Replace(match[i].Value, filterValue);
                }

                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                if (filterClause != null)
                {
                    filterClause.FilterClauseProcess(tableName, ref whereCondition, ref sqlParams, ref suffix);
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
                throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title });
            }
        }

        private async Task CheckValue(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                // Validate info
                if (!field.IsMultiRow)
                {
                    await ValidValueAsync(info, field, null);
                }
                else // Validate rows
                {
                    int rowIndx = 0;
                    foreach (var row in rows)
                    {
                        rowIndx++;
                        await ValidValueAsync(row, field, rowIndx);
                    }
                }
            }
        }

        private async Task ValidValueAsync(ValidateRowModel checkData, ValidateField field, int? rowIndex)
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
            if ((AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId)) || field.IsAutoIncrement || string.IsNullOrEmpty(value))
            {
                return;
            }
            string regex = ((EnumDataType)field.DataTypeId).GetRegex();
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
            {
                throw new BadRequestException(InputErrorCode.InputValueInValid, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title });
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
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

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
            infoSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputValueBillId}");
            var infoLst = (await _accountancyDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            NonCamelCaseDictionary currentInfo = null;
            if (infoLst.Count != 0)
            {
                currentInfo = infoLst[0];
            }
            NonCamelCaseDictionary futureInfo = data.Info;
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, CompareRow(currentInfo, futureInfo, singleFields));

            // Get changed rows
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            var rowsSQL = new StringBuilder("SELECT F_Id,");
            var multiFields = inputAreaFields.Where(f => f.IsMultiRow).ToList();
            AppendSelectFields(ref rowsSQL, multiFields);
            rowsSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputValueBillId}");
            var currentRows = (await _accountancyDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            foreach (var futureRow in data.Rows)
            {
                futureRow.TryGetValue("F_Id", out string futureValue);
                NonCamelCaseDictionary curRow = currentRows.FirstOrDefault(r => futureValue != null && r["F_Id"].ToString() == futureValue);
                if (curRow == null)
                {
                    checkRows.Add(new ValidateRowModel(futureRow, null));
                }
                else
                {
                    string[] changeFieldIndexes = CompareRow(curRow, futureRow, multiFields);
                    checkRows.Add(new ValidateRowModel(futureRow, changeFieldIndexes));
                }
            }

            // Lấy thông tin field
            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, inputAreaFields);
            // Check refer
            await CheckReferAsync(checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkInfo, checkRows, uniqueFields, inputValueBillId);
            // Check value
            await CheckValue(checkInfo, checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields, EnumAction.Update);

                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputValueBillId);

                if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ");


                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);


                var areaFieldGenCodes = new Dictionary<int, CustomGenCodeOutputModelOut>();

                await CreateBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion + 1, data, areaFieldGenCodes);

                billInfo.LatestBillVersion++;

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields, EnumAction.Update);

                await ConfirmCustomGenCode(areaFieldGenCodes);

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Thêm chứng từ {inputTypeInfo.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
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
                var currentValue = currentRow[field.FieldName].ToString();
                var updateValue = futureRow[field.FieldName];
                if (currentValue != updateValue?.ToString())
                {
                    changeFieldIndexes.Add(field.FieldName);
                }
            }
            return changeFieldIndexes.ToArray();
        }

        public async Task<bool> DeleteBill(int inputTypeId, long inputBill_F_Id)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputBill_F_Id);

                if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ");

                var inputAreaFields = new List<ValidateField>();
                // Get current data
                BillInfoModel data = new BillInfoModel();
                if (!string.IsNullOrEmpty(inputTypeInfo.BeforeSaveAction) || !string.IsNullOrEmpty(inputTypeInfo.AfterSaveAction))
                {
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
                    infoSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputBill_F_Id}");
                    var infoLst = (await _accountancyDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();

                    data.Info = infoLst.Count != 0 ? infoLst[0].ToNonCamelCaseDictionary(f => f.Key, f => f.Value) : new NonCamelCaseDictionary();

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
                    rowsSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputBill_F_Id}");
                    var currentRows = (await _accountancyDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
                    data.Rows = currentRows.Select(r => r.ToNonCamelCaseDictionary(f => f.Key, f => f.Value.ToString())).ToArray();
                }

                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields, EnumAction.Delete);

                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);


                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields, EnumAction.Delete);

                await _outsideImportMappingService.MappingObjectDelete(billInfo.FId);

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Xóa chứng từ {inputTypeInfo.Title}", new { inputTypeId, inputBill_F_Id }.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "DeleteBill");
                throw;
            }
        }


        private async Task FillGenerateColumn(Dictionary<int, CustomGenCodeOutputModelOut> areaFieldGenCodes, Dictionary<string, ValidateField> fields, IList<NonCamelCaseDictionary> rows)
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
                        CustomGenCodeOutputModelOut currentConfig;
                        try
                        {
                            currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.InputType, field.InputAreaFieldId);

                            if (currentConfig == null)
                            {
                                throw new BadRequestException(GeneralCode.ItemNotFound, "Thiết định cấu hình sinh mã null " + field.Title);
                            }
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw new BadRequestException(badRequest.Code, "Cấu hình sinh mã " + field.Title + " => " + badRequest.Message);
                        }
                        catch (Exception)
                        {
                            throw;
                        }


                        if (!areaFieldGenCodes.ContainsKey(field.InputAreaFieldId))
                        {
                            areaFieldGenCodes.Add(field.InputAreaFieldId, currentConfig);
                        }

                        var genCodeInfo = areaFieldGenCodes[field.InputAreaFieldId];

                        try
                        {

                            var generated = await _customGenCodeHelperService.GenerateCode(genCodeInfo.CustomGenCodeId, genCodeInfo.LastValue);
                            if (generated == null)
                            {
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã " + field.Title);
                            }


                            value = generated.CustomCode;
                            genCodeInfo.LastValue = generated.LastValue;
                            genCodeInfo.LastCode = generated.CustomCode;
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw new BadRequestException(badRequest.Code, "Sinh mã " + field.Title + " => " + badRequest.Message);
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

        private async Task ConfirmCustomGenCode(Dictionary<int, CustomGenCodeOutputModelOut> areaFieldGenCodes)
        {
            foreach (var (key, value) in areaFieldGenCodes)
            {
                await _customGenCodeHelperService.ConfirmCode(EnumObjectType.InputType, key);
            }
        }

        private async Task CreateBillVersion(int inputTypeId, long inputBill_F_Id, int billVersionId, BillInfoModel data, Dictionary<int, CustomGenCodeOutputModelOut> areaFieldGenCodes)
        {
            var fields = (await GetInputFields(inputTypeId)).ToDictionary(f => f.FieldName, f => f);


            var infoFields = fields.Where(f => !f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(areaFieldGenCodes, infoFields, new[] { data.Info });

            var rowFields = fields.Where(f => f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(areaFieldGenCodes, rowFields, data.Rows);

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

            //Create rows
            foreach (var row in data.Rows)
            {
                var dataRow = NewBillVersionRow(dataTable, inputTypeId, inputBill_F_Id, billVersionId, false);

                foreach (var item in data.Info)
                {
                    if (item.Key.IsVndColumn() || item.Key.IsNgoaiTeColumn())
                    {
                        continue;
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
                        dataRow[colName] = deValue;
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

                    throw new BadRequestException(GeneralCode.InvalidParams, $"Vui lòng nhập đầy đủ tài khoản đối ứng tương ứng {fieldTitle}");
                }

                dataTable.Rows.Add(dataRow);
            }

            //Create addition reciprocal accounting
            if (data.Info.Any(k => k.Key.IsVndColumn() && decimal.TryParse(k.Value?.ToString(), out var value) && value != 0))
            {
                var dataRow = NewBillVersionRow(dataTable, inputTypeId, inputBill_F_Id, billVersionId, true);

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
                    var fieldName = "";
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        fieldName = fields[key].FieldName;
                    }

                    throw new BadRequestException(GeneralCode.InvalidParams, $"Vui lòng nhập đầy đủ tài khoản đối ứng tương ứng {fieldName}");
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
                });
        }


        private async Task<List<ValidateField>> GetInputFields(int inputTypeId)
        {
            return await (from af in _accountancyDBContext.InputAreaField
                          join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                          join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                          where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly //&& f.FieldName != AccountantConstants.F_IDENTITY
                          select new ValidateField
                          {
                              InputAreaFieldId = af.InputAreaFieldId,
                              Title = af.Title,
                              IsAutoIncrement = af.IsAutoIncrement,
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
                              RequireFilters = af.RequireFilters
                          }).ToListAsync();
        }

        public async Task<bool> ImportBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream)
        {
            var inputType = _accountancyDBContext.InputType.FirstOrDefault(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = await GetInputFields(inputTypeId);

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            if (mapping.MappingFields.Where(mf => mf.IsRequire).Any(mf => !fields.Exists(f => f.FieldName == mf.FieldName))) throw new BadRequestException(GeneralCode.ItemNotFound, $"Trường dữ liệu không tìm thấy");

            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefTableField)).ToList();
            var referTableNames = fields.Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();
            var referFields = (from f in _accountancyDBContext.CategoryField
                               join c in _accountancyDBContext.Category on f.CategoryId equals c.CategoryId
                               where referTableNames.Contains(c.CategoryCode) && referMapingFields.Select(f => f.RefTableField).Contains(f.CategoryFieldName)
                               select new
                               {
                                   c.CategoryCode,
                                   f.CategoryFieldName,
                                   f.DataTypeId
                               }).ToList();

            var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == mapping.Key);
            if (columnKey == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Định danh mã chứng từ không đúng, vui lòng chọn lại");
            }
            var groups = data.Rows.Select((r, i) => new
            {
                Data = r,
                Index = i + mapping.FromRow
            }).Where(r => r.Data[columnKey.Column] != null).GroupBy(r => r.Data[columnKey.Column]);
            List<BillInfoModel> bills = new List<BillInfoModel>();

            // Validate unique single field
            foreach (var field in fields.Where(f => f.IsUnique))
            {
                var mappingField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                if (mappingField == null) continue;

                var values = field.IsMultiRow ? groups.SelectMany(b => b.Select(r => r.Data[mappingField.Column]?.ToString())).ToList() : groups.Where(b => b.Count() > 0).Select(b => b.First().Data[mappingField.Column]?.ToString()).ToList();

                // Check unique trong danh sách values thêm mới
                if (values.Distinct().Count() < values.Count)
                {
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }
                // Checkin unique trong db
                var existSql = $"SELECT F_Id FROM vInputValueRow WHERE InputTypeId = {inputTypeId} ";

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
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }
            }

            foreach (var bill in groups)
            {
                var info = new NonCamelCaseDictionary();
                var rows = new List<NonCamelCaseDictionary>();
                int count = bill.Count();
                for (int rowIndx = 0; rowIndx < count; rowIndx++)
                {
                    var mapRow = new NonCamelCaseDictionary();
                    var row = bill.ElementAt(rowIndx);
                    foreach (var field in fields)
                    {
                        if (!field.IsMultiRow && rowIndx > 0) continue;
                        var mappingField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                        // Validate mapping required
                        if (mappingField == null && field.IsRequire) throw new BadRequestException(GeneralCode.ItemNotFound, $"Trường dữ liệu {field.FieldName} không tìm thấy");
                        if (mappingField == null) continue;
                        string value = null;
                        if (row.Data.ContainsKey(mappingField.Column))
                            value = row.Data[mappingField.Column]?.ToString();
                        // Validate require
                        if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire) throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { row.Index, field.Title });

                        if (string.IsNullOrWhiteSpace(value)) continue;
                        value = value.Trim();
                        if (field.DataTypeId == (int)EnumDataType.Date)
                        {
                            if (!DateTime.TryParse(value.ToString(), out DateTime date))
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value}, dòng {row.Index}, trường {field.Title} sang kiểu ngày tháng");
                            value = date.AddHours(-7).GetUnix().ToString();
                        }

                        // Validate refer
                        if (string.IsNullOrEmpty(field.RefTableCode))
                        {
                            // Validate value
                            if (!AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId) && !field.IsAutoIncrement && !string.IsNullOrEmpty(value))
                            {
                                string regex = ((EnumDataType)field.DataTypeId).GetRegex();
                                if ((field.DataSize > 0 && value.Length > field.DataSize)
                                    || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                                    || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
                                {
                                    throw new BadRequestException(InputErrorCode.InputValueInValid, new object[] { row.Index, field.Title });
                                }
                            }
                        }
                        else
                        {
                            int suffix = 0;
                            var paramName = $"@{mappingField.RefTableField}_{suffix}";
                            var referField = referFields.First(f => f.CategoryCode == field.RefTableCode && f.CategoryFieldName == mappingField.RefTableField);
                            var referSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefTableField} = {paramName}";
                            var referParams = new List<SqlParameter>() { new SqlParameter(paramName, value.ConvertValueByType((EnumDataType)referField.DataTypeId)) };
                            suffix++;
                            if (!string.IsNullOrEmpty(field.Filters))
                            {
                                var filters = field.Filters;
                                var pattern = @"@{(?<word>\w+)}";
                                Regex rx = new Regex(pattern);
                                MatchCollection match = rx.Matches(field.Filters);
                                for (int i = 0; i < match.Count; i++)
                                {
                                    var fieldName = match[i].Groups["word"].Value;
                                    mapRow.TryGetValue(fieldName, out string filterValue);
                                    if (string.IsNullOrEmpty(filterValue))
                                    {
                                        info.TryGetValue(fieldName, out filterValue);
                                    }
                                    if (string.IsNullOrEmpty(filterValue)) throw new BadRequestException(GeneralCode.InvalidParams, $"Cần thông tin {fieldName} trước thông tin {field.FieldName}");
                                    filters = filters.Replace(match[i].Value, filterValue);
                                }

                                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                                if (filterClause != null)
                                {
                                    var whereCondition = new StringBuilder();
                                    filterClause.FilterClauseProcess($"v{field.RefTableCode}", ref whereCondition, ref referParams, ref suffix);
                                    if (whereCondition.Length > 0) referSql += $" AND {whereCondition.ToString()}";
                                }
                            }

                            var referData = await _accountancyDBContext.QueryDataTable(referSql, referParams.ToArray());
                            if (referData == null || referData.Rows.Count == 0)
                            {
                                throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { row.Index, field.Title });
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
                    rows.Add(mapRow);
                }

                bills.Add(new BillInfoModel
                {
                    Info = info,
                    Rows = rows.ToArray()
                });
            }

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var areaFieldGenCodes = new Dictionary<int, CustomGenCodeOutputModelOut>();

                    foreach (var bill in bills)
                    {
                        // validate require
                        ValidateRowModel checkInfo = new ValidateRowModel(bill.Info, null);

                        List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
                        checkRows = bill.Rows.Select(r => new ValidateRowModel(r, null)).ToList();

                        // Validate info
                        var requiredFields = fields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
                        // Check field required
                        await CheckRequired(checkInfo, checkRows, requiredFields, fields);

                        // Before saving action (SQL)
                        await ProcessActionAsync(inputType.BeforeSaveAction, bill, fields, EnumAction.Add);

                        var billInfo = new InputBill()
                        {
                            InputTypeId = inputTypeId,
                            LatestBillVersion = 1,
                            IsDeleted = false
                        };

                        await _accountancyDBContext.InputBill.AddAsync(billInfo);

                        await _accountancyDBContext.SaveChangesAsync();

                        await CreateBillVersion(inputTypeId, billInfo.FId, 1, bill, areaFieldGenCodes);

                        // After saving action (SQL)
                        await ProcessActionAsync(inputType.AfterSaveAction, bill, fields, EnumAction.Add);
                    }

                    await ConfirmCustomGenCode(areaFieldGenCodes);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Import");
                    throw;
                }
            }
            return true;
        }


        protected class ValidateRowModel
        {
            public NonCamelCaseDictionary Data { get; set; }
            public string[] CheckFields { get; set; }

            public ValidateRowModel(NonCamelCaseDictionary Data, string[] CheckFields)
            {
                this.Data = Data;
                this.CheckFields = CheckFields;
            }
        }

        protected class ValidateField
        {
            public int InputAreaFieldId { get; set; }
            public string Title { get; set; }
            public bool IsAutoIncrement { get; set; }
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
        }
    }
}