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

        public InputDataService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
        }


        public async Task<PageDataTable> GetBills(int inputTypeId, string keyword, IList<InputValueFilterModel> fieldFilters, string orderByFieldName, bool asc, int page, int size)
        {
            var fields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId
                select new { a.InputAreaId, af.InputAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.DataTypeId, a.IsMultiRow }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);

            var whereCondition = new StringBuilder();

            whereCondition.Append($"InputTypeId = {inputTypeId}");

            var sqlParams = new List<SqlParameter>();

            var idx = 0;
            foreach (var filter in fieldFilters)
            {
                if (!filter.FieldName.ValidateValidSqlObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên cột không được phép"); };

                //if (!fields.ContainsKey(filter.FieldName)) continue;

                //var field = fields[filter.FieldName];

                if (filter.Values != null && filter.Values.Length > 0 && !string.IsNullOrWhiteSpace(filter.Values[0]))//field != null && 
                {
                    object objectValue = objectValue = filter.DataTypeId.GetSqlValue(filter.Values[0]);

                    whereCondition.Append(" AND ");

                    idx++;

                    var paramName = "@" + filter.FieldName + "" + idx;

                    switch (filter.Operator)
                    {
                        //text
                        case EnumOperator.Equal:
                            whereCondition.Append($"[{filter.FieldName}] = {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));

                            break;

                        case EnumOperator.StartsWith:
                        case EnumOperator.EndsWith:
                        case EnumOperator.Contains:
                            switch (filter.Operator)
                            {
                                case EnumOperator.StartsWith:
                                    objectValue = $"{objectValue}%";
                                    break;
                                case EnumOperator.EndsWith:
                                    objectValue = $"%{objectValue}";
                                    break;
                                case EnumOperator.Contains:
                                    objectValue = $"%{objectValue}%";
                                    break;
                            }

                            whereCondition.Append($"[{filter.FieldName}] LIKE {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));

                            break;

                        case EnumOperator.InList:

                            var nvalues = new DataTable("_NVALUES");
                            nvalues.Columns.Add("NValue", typeof(string));
                            foreach (var value in filter.Values)
                            {
                                var row = nvalues.NewRow();
                                row["NValue"] = value;
                                nvalues.Rows.Add(row);
                            }

                            whereCondition.Append($"[{filter.FieldName}] IN (SELECT NValue FROM {paramName})");
                            sqlParams.Add(new SqlParameter(paramName, nvalues) { SqlDbType = SqlDbType.Structured, TypeName = "_NVALUES" });

                            break;


                        //number
                        case EnumOperator.NotEqual:
                            whereCondition.Append($"[{filter.FieldName}] != {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));

                            break;

                        case EnumOperator.Greater:
                            whereCondition.Append($"[{filter.FieldName}] > {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));
                            break;

                        case EnumOperator.GreaterOrEqual:
                            whereCondition.Append($"[{filter.FieldName}] >= {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));
                            break;

                        case EnumOperator.LessThan:
                            whereCondition.Append($"[{filter.FieldName}] < {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));
                            break;

                        case EnumOperator.LessThanOrEqual:
                            whereCondition.Append($"[{filter.FieldName}] <= {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, objectValue));
                            break;
                    }
                }
            }


            var mainColumns = fields.Values.Where(f => !f.IsMultiRow).SelectMany(f =>
            {
                var refColumns = new List<string>()
                {
                    f.FieldName
                };

                if (!string.IsNullOrWhiteSpace(f.RefTableTitle) && !string.IsNullOrWhiteSpace(f.RefTableTitle))
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
                    t.InputBill_F_Id AS F_Id,
                    {selectColumn}
                FROM tmp t JOIN {INPUTVALUEROW_VIEW} r ON t.F_Id = r.F_Id
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
                ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return (data, total);
        }


        public async Task<PageDataTable> GetBillInfo(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size)
        {
            var totalSql = @$"SELECT COUNT(0) as Total FROM {INPUTVALUEROW_VIEW} r WHERE InputBill_F_Id = @F_Id AND r.InputTypeId = @InputTypeId";

            var table = await _accountancyDBContext.QueryDataTable(totalSql, new[] { new SqlParameter("@InputTypeId", inputTypeId), new SqlParameter("@F_Id", fId) });

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

                WHERE r.InputBill_F_Id = @F_Id AND r.InputTypeId = @InputTypeId    

                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
            ";
            var data = await _accountancyDBContext.QueryDataTable(dataSql, new[] { new SqlParameter("@InputTypeId", inputTypeId), new SqlParameter("@F_Id", fId) });

            return (data, total);
        }


        public async Task<long> CreateBill(int inputTypeId, BillInfoModel data)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Length == 0)
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            // Lấy thông tin field
            var inputAreaFields = (from af in _accountancyDBContext.InputAreaField
                                   join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                                   join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                                   where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                                   select new ValidateField
                                   {
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
                                       IsMultiRow = a.IsMultiRow
                                   }).ToList();


            // Validate info
            var requiredFields = inputAreaFields.Where(f => !f.IsMultiRow && !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = inputAreaFields.Where(f => !f.IsMultiRow && !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = inputAreaFields.Where(f => !f.IsMultiRow && !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>
            {
                new ValidateRowModel(data.Info, null)
            };

            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            await CheckReferAsync(checkRows, selectFields, data.Info);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkRows, uniqueFields);
            // Check value
            CheckValue(checkRows, inputAreaFields);

            // Validate rows
            requiredFields = inputAreaFields.Where(f => f.IsMultiRow && !f.IsAutoIncrement && f.IsRequire).ToList();
            uniqueFields = inputAreaFields.Where(f => f.IsMultiRow && !f.IsAutoIncrement && f.IsUnique).ToList();
            selectFields = inputAreaFields.Where(f => f.IsMultiRow && !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();
            checkRows = data.Rows.Select(r => new ValidateRowModel(r, null)).ToList();

            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            await CheckReferAsync(checkRows, selectFields, data.Info);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkRows, uniqueFields);
            // Check value
            CheckValue(checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields);


                var billInfo = new InputBill()
                {
                    InputTypeId = inputTypeId,
                    LatestBillVersion = 1,
                    IsDeleted = false
                };
                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                await _accountancyDBContext.SaveChangesAsync();

                await CreateBillVersion(inputTypeId, billInfo.FId, 1, data);

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields);


                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Thêm chứng từ {inputTypeInfo.Title}", data.JsonSerialize());
                return billInfo.FId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateBill");
                throw ex;
            }
        }

        private async Task ProcessActionAsync(string script, BillInfoModel data, List<ValidateField> fields)
        {
            if (!string.IsNullOrEmpty(script))
            {
                var parammeters = new List<SqlParameter>();
                var pattern = @"@{(?<word>\w+)}";
                Regex rx = new Regex(pattern);
                MatchCollection match = rx.Matches(script);
                int suffix = 0;
                for (int i = 0; i < match.Count; i++)
                {
                    var fieldName = match[i].Groups["word"].Value;
                    var field = fields.First(f => f.FieldName == fieldName);
                    if (!field.IsMultiRow)
                    {
                        var paramName = $"{match[i].Value}_{suffix}";
                        script = script.Replace(match[i].Value, paramName);
                        parammeters.Add(new SqlParameter(paramName, data.Info[fieldName].ConvertValueByType((EnumDataType)field.DataTypeId)));
                        suffix++;
                    }
                    else
                    {
                        var paramNames = new StringBuilder();
                        for (int rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
                        {
                            if (rowIndx > 0)
                            {
                                paramNames.Append(", ");
                            }
                            var paramName = $"{match[i].Value}_{suffix}";
                            paramNames.Append(paramName);
                            parammeters.Add(new SqlParameter(paramName, data.Rows[rowIndx][fieldName].ConvertValueByType((EnumDataType)field.DataTypeId)));
                            suffix++;
                        }
                        script = script.Replace(match[i].Value, paramNames.ToString());
                    }
                }
                await _accountancyDBContext.Database.ExecuteSqlRawAsync(script, parammeters);
            }
        }

        private void CheckRequired(List<ValidateRowModel> rows, List<ValidateField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                foreach (var row in rows)
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    row.Data.TryGetValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                    }
                }
            }
        }

        private async Task CheckUniqueAsync(int inputTypeId, List<ValidateRowModel> data, List<ValidateField> uniqueFields, long[] deleteInputValueRowId = null)
        {
            if (deleteInputValueRowId is null)
            {
                deleteInputValueRowId = Array.Empty<long>();
            }
            // Check unique
            foreach (var field in uniqueFields)
            {
                // Get list change value
                List<object> values = new List<object>();
                foreach (var row in data)
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    row.Data.TryGetValue(field.FieldName, out string value);
                    if (!string.IsNullOrEmpty(value))
                    {
                        values.Add(value.ConvertValueByType((EnumDataType)field.DataTypeId));
                    }
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
                var existSql = $"SELECT F_Id FROM vInputValueRow WHERE InputTypeId = {inputTypeId} ";
                if (deleteInputValueRowId.Length > 0)
                {
                    existSql += $"AND F_Id NOT IN {string.Join(",", deleteInputValueRowId)}";
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
        }

        private async Task CheckReferAsync(List<ValidateRowModel> data, List<ValidateField> selectFields, Dictionary<string, string> info)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                string tableName = $"v{field.RefTableCode}";
                foreach (var row in data)
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    row.Data.TryGetValue(field.FieldName, out string textValue);
                    if (string.IsNullOrEmpty(textValue))
                    {
                        continue;
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
                            row.Data.TryGetValue(fieldName, out string filterValue);
                            if (string.IsNullOrEmpty(filterValue))
                            {
                                info.TryGetValue(fieldName, out filterValue);
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
                        existSql += $" AND {whereCondition.ToString()}";
                    }

                    var result = await _accountancyDBContext.QueryDataTable(existSql, sqlParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (!isExisted)
                    {
                        throw new BadRequestException(InputErrorCode.ReferValueNotFound, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckValue(List<ValidateRowModel> data, List<ValidateField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                foreach (var row in data)
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    row.Data.TryGetValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    if ((AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId)) || field.IsAutoIncrement || string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    string regex = ((EnumDataType)field.DataTypeId).GetRegex();
                    if ((field.DataSize > 0 && value.Length > field.DataSize)
                        || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                        || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
                    {
                        throw new BadRequestException(InputErrorCode.InputValueInValid, new string[] { field.Title });
                    }
                }
            }
        }

        public async Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Length == 0)
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            // Lấy thông tin field
            var inputAreaFields = (from af in _accountancyDBContext.InputAreaField
                                   join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                                   join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                                   where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                                   select new ValidateField
                                   {
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
                                       IsMultiRow = a.IsMultiRow
                                   }).ToList();

            // Validate info
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
            infoSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputValueBillId}");
            var infoLst = (await _accountancyDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            NonCamelCaseDictionary currentInfo = null;
            if (infoLst.Count != 0)
            {
                currentInfo = infoLst[0];
            }
            Dictionary<string, string> futureInfo = data.Info;
            string[] changeFields = CompareRow(currentInfo, futureInfo, singleFields);
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>
            {
                new ValidateRowModel(data.Info, changeFields)
            };
            // Lấy thông tin field
            var requiredFields = singleFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = singleFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = singleFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();
            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            await CheckReferAsync(checkRows, selectFields, data.Info);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkRows, uniqueFields);
            // Check value
            CheckValue(checkRows, inputAreaFields);

            // Validate rows
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
            rowsSQL.Append($" FROM vInputValueRow WHERE InputBill_F_Id = {inputValueBillId}");
            var currentRows = (await _accountancyDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            checkRows.Clear();
            foreach (var futureRow in data.Rows)
            {
                NonCamelCaseDictionary curRow = currentRows.FirstOrDefault(r => r["F_Id"].ToString() == futureRow["F_Id"]);
                if (curRow == null)
                {
                    checkRows.Add(new ValidateRowModel(futureRow, null));
                }
                else
                {
                    string[] changeFieldIndexes = CompareRow(curRow, futureRow, multiFields);
                    if (changeFieldIndexes.Length > 0)
                    {
                        checkRows.Add(new ValidateRowModel(futureRow, changeFieldIndexes));
                    }
                }
            }
            // Lấy thông tin field
            requiredFields = multiFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            uniqueFields = multiFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            selectFields = multiFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId)).ToList();
            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            await CheckReferAsync(checkRows, selectFields, data.Info);
            // Check unique
            await CheckUniqueAsync(inputTypeId, checkRows, uniqueFields);
            // Check value
            CheckValue(checkRows, inputAreaFields);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields);

                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputValueBillId);

                if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ");


                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);

                await CreateBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion + 1, data);

                billInfo.LatestBillVersion++;

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields);

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Thêm chứng từ {inputTypeInfo.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateBill");
                throw ex;
            }
        }

        private string[] CompareRow(NonCamelCaseDictionary currentRow, Dictionary<string, string> futureRow, List<ValidateField> fields)
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
                if (currentValue != updateValue)
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
                    inputAreaFields = (from af in _accountancyDBContext.InputAreaField
                                       join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                                       join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                                       where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                                       select new ValidateField
                                       {
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
                                           IsMultiRow = a.IsMultiRow
                                       }).ToList();


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

                    data.Info = infoLst.Count != 0 ? infoLst[0].ToDictionary(f => f.Key, f => f.Value.ToString()) : new Dictionary<string, string>();

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
                    data.Rows = currentRows.Select(r => r.ToDictionary(f => f.Key, f => f.Value.ToString())).ToArray();
                }

                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.BeforeSaveAction, data, inputAreaFields);

                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);


                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeInfo.AfterSaveAction, data, inputAreaFields);

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputTypeRow, billInfo.FId, $"Xóa chứng từ {inputTypeInfo.Title}", new { inputTypeId, inputBill_F_Id }.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "DeleteBill");
                throw ex;
            }
        }


        private async Task CreateBillVersion(int inputTypeId, long inputBill_F_Id, int billVersionId, BillInfoModel data)
        {
            var fields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId
                select new { a.InputAreaId, af.InputAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.DataTypeId, a.IsMultiRow }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);


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
            dataTable.Columns.Add("CreatedByUserId", typeof(int));
            dataTable.Columns.Add("CreatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("IsDeleted", typeof(bool));
            dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));

            foreach (var column in insertColumns)
            {
                var field = fields[column];

                dataTable.Columns.Add(column, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }


            foreach (var row in data.Rows)
            {
                var dataRow = dataTable.NewRow();
                dataRow["InputTypeId"] = inputTypeId;
                dataRow["InputBill_F_Id"] = inputBill_F_Id;
                dataRow["BillVersion"] = billVersionId;
                dataRow["CreatedByUserId"] = _currentContextService.UserId;
                dataRow["CreatedDatetimeUtc"] = DateTime.UtcNow;
                dataRow["UpdatedByUserId"] = _currentContextService.UserId;
                dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;
                dataRow["IsDeleted"] = false;
                dataRow["DeletedDatetimeUtc"] = DBNull.Value;

                foreach (var item in data.Info)
                {
                    var field = fields[item.Key];
                    dataRow[item.Key] = ((EnumDataType)field.DataTypeId).GetSqlValue(item.Value);
                }

                foreach (var item in row)
                {
                    var field = fields[item.Key];
                    dataRow[item.Key] = ((EnumDataType)field.DataTypeId).GetSqlValue(item.Value);
                }

                dataTable.Rows.Add(dataRow);
            }

            await _accountancyDBContext.InsertDataTable(dataTable);

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



        public async Task<bool> ImportBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream)
        {
            var inputType = _accountancyDBContext.InputType.FirstOrDefault(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = (from af in _accountancyDBContext.InputAreaField
                          join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                          join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                          where af.InputTypeId == inputTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FieldName != AccountantConstants.F_IDENTITY
                          select new ValidateField
                          {
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
                              IsMultiRow = a.IsMultiRow
                          }).ToList();

            var infoData = reader.ReadSheets(mapping.SheetInfo, mapping.FromInfo, mapping.ToInfo, null).FirstOrDefault();
            var info = new Dictionary<string, string>();
            var rows = new List<Dictionary<string, string>>();
            var singleFields = fields.Where(f => !f.IsMultiRow).ToList();
            if (infoData.Rows.Length > 2)
            {
                var row = infoData.Rows[1];
                for (int fieldIndx = 0; fieldIndx < mapping.MappingInfoFields.Count; fieldIndx++)
                {
                    var mappingField = mapping.MappingInfoFields[fieldIndx];
                    var field = fields.FirstOrDefault(f => f.FieldName == mappingField.FieldName);

                    if (field == null && !string.IsNullOrEmpty(mappingField.FieldName)) throw new BadRequestException(GeneralCode.ItemNotFound, "Trường dữ liệu không tìm thấy");

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire) throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });

                    if (string.IsNullOrWhiteSpace(value)) continue;

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        info.Add(field.FieldName, value);
                    }
                    else
                    {
                        var referSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefTableField} = {value}";
                        var referData = await _accountancyDBContext.QueryDataTable(referSql, Array.Empty<SqlParameter>());
                        if (referData != null && referData.Rows.Count > 0)
                        {
                            var referValue = referData.Rows[0][field.RefTableField]?.ToString() ?? string.Empty;
                            info.Add(field.FieldName, referValue);
                        }
                        else
                        {
                            throw new BadRequestException(InputErrorCode.ReferValueNotFound, new string[] { field.Title });
                        }
                    }
                }
            }

            var rowData = reader.ReadSheets(mapping.SheetInfo, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();
            for (var rowIndx = 1; rowIndx < rowData.Rows.Length; rowIndx++)
            {
                var map = new Dictionary<string, string>();
                var row = rowData.Rows[rowIndx];
                for (int fieldIndx = 0; fieldIndx < mapping.MappingRowFields.Count; fieldIndx++)
                {
                    var mappingField = mapping.MappingRowFields[fieldIndx];
                    var field = fields.FirstOrDefault(f => f.FieldName == mappingField.FieldName);

                    if (field == null && !string.IsNullOrEmpty(mappingField.FieldName)) throw new BadRequestException(GeneralCode.ItemNotFound, "Trường dữ liệu không tìm thấy");

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire) throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });

                    if (string.IsNullOrWhiteSpace(value)) continue;

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        info.Add(field.FieldName, value);
                    }
                    else
                    {
                        var referSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefTableField} = {value}";
                        var referData = await _accountancyDBContext.QueryDataTable(referSql, Array.Empty<SqlParameter>());
                        if (referData != null && referData.Rows.Count > 0)
                        {
                            var referValue = referData.Rows[0][field.RefTableField]?.ToString() ?? string.Empty;
                            info.Add(field.FieldName, referValue);
                        }
                        else
                        {
                            throw new BadRequestException(InputErrorCode.ReferValueNotFound, new string[] { field.Title });
                        }
                    }
                }

                rows.Add(map);
            }

            BillInfoModel data = new BillInfoModel
            {
                Info = info,
                Rows = rows.ToArray()
            };

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Before saving action (SQL)
                    await ProcessActionAsync(inputType.BeforeSaveAction, data, fields);

                    var billInfo = new InputBill()
                    {
                        InputTypeId = inputTypeId,
                        LatestBillVersion = 1,
                        IsDeleted = false
                    };

                    await _accountancyDBContext.InputBill.AddAsync(billInfo);

                    await _accountancyDBContext.SaveChangesAsync();

                    await CreateBillVersion(inputTypeId, billInfo.FId, 1, data);

                    // After saving action (SQL)
                    await ProcessActionAsync(inputType.AfterSaveAction, data, fields);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Import");
                    throw ex;
                }
            }
            return true;
        }



        protected class ValidateRowModel
        {
            public Dictionary<string, string> Data { get; set; }
            public string[] CheckFields { get; set; }

            public ValidateRowModel(Dictionary<string, string> Data, string[] CheckFields)
            {
                this.Data = Data;
                this.CheckFields = CheckFields;
            }
        }

        protected class ValidateField
        {
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
        }
    }
}
