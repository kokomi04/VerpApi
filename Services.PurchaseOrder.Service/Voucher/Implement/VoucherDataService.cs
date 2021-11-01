﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.Voucher;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using System.Data;
using VErp.Infrastructure.EF.EFExtensions;
using Verp.Cache.RedisCache;
using VErp.Commons.Library;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Constants;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.PurchaseOrder.Voucher;
using static Verp.Resources.PurchaseOrder.Voucher.VoucherDataValidationMessage;
using AutoMapper.QueryableExtensions;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherDataService : IVoucherDataService
    {
        private const string VOUCHERVALUEROW_TABLE = PurchaseOrderConstants.VOUCHERVALUEROW_TABLE;
        private const string VOUCHERVALUEROW_VIEW = PurchaseOrderConstants.VOUCHERVALUEROW_VIEW;

        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly IVoucherConfigService _voucherConfigService;

        private readonly ObjectActivityLogFacade _voucherDataActivityLog;

        public VoucherDataService(PurchaseOrderDBContext purchaseOrderDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<VoucherConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , ICategoryHelperService httpCategoryHelperService
            , IOutsideMappingHelperService outsideMappingHelperService
            , IVoucherConfigService voucherConfigService
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _voucherConfigService = voucherConfigService;
            _voucherDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.VoucherBill);
        }

        public async Task<PageDataTable> GetVoucherBills(int voucherTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var viewInfo = await _purchaseOrderDBContext.VoucherTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();
            var voucherTypeViewId = viewInfo?.VoucherTypeViewId;
            var fields = (await (
                from af in _purchaseOrderDBContext.VoucherAreaField
                join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                where af.VoucherTypeId == voucherTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                select new { a.VoucherAreaId, af.VoucherAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.FormTypeId, f.DataTypeId, a.IsMultiRow }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);
            var viewFields = await (
                from f in _purchaseOrderDBContext.VoucherTypeViewField
                where f.VoucherTypeViewId == voucherTypeViewId
                select f
            ).ToListAsync();
            var whereCondition = new StringBuilder();
            whereCondition.Append($"r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()}");
            var sqlParams = new List<SqlParameter>();
            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.VoucherTypeViewFieldId == filter.Key);
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
                            filterClause.FilterClauseProcess(VOUCHERVALUEROW_VIEW, "r", ref whereCondition, ref sqlParams, ref suffix, false, value);
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
                columnsFilters.FilterClauseProcess(VOUCHERVALUEROW_VIEW, "r", ref whereCondition, ref sqlParams, ref suffix);
            }
            var mainColumns = fields.Values.Where(f => !f.IsMultiRow).SelectMany(f =>
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
            if (!mainColumns.Contains(orderByFieldName))
            {
                orderByFieldName = mainColumns.Contains("ngay_ct") ? "ngay_ct" : "F_Id";
                asc = false;
            }
            var totalSql = @$"SELECT COUNT(DISTINCT r.VoucherBill_F_Id) as Total FROM {VOUCHERVALUEROW_VIEW} r WHERE {whereCondition}";
            var table = await _purchaseOrderDBContext.QueryDataTable(totalSql, sqlParams.ToArray());
            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }
            var selectColumn = string.Join(",", mainColumns.Select(c => $"r.[{c}]"));
            var dataSql = @$"
                 ;WITH tmp AS (
                    SELECT r.VoucherBill_F_Id, MAX(F_Id) as F_Id
                    FROM {VOUCHERVALUEROW_VIEW} r
                    WHERE {whereCondition}
                    GROUP BY r.VoucherBill_F_Id    
                )
                SELECT 
                    t.VoucherBill_F_Id AS F_Id
                    {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                FROM tmp t JOIN {VOUCHERVALUEROW_VIEW} r ON t.F_Id = r.F_Id
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            if (size >= 0)
            {
                dataSql += @$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY";
            }
            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            return (data, total);
        }

        public async Task<PageDataTable> GetVoucherBillInfoRows(int voucherTypeId, long fId, string orderByFieldName, bool asc, int page, int size)
        {
            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var totalSql = @$"SELECT COUNT(0) as Total FROM {VOUCHERVALUEROW_VIEW} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0";

            var table = await _purchaseOrderDBContext.QueryDataTable(totalSql, new SqlParameter[0]);

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
                FROM {VOUCHERVALUEROW_VIEW} r 

                WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0

                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}

            ";
            if (size > 0)
            {
                dataSql += @$"
                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
            ";
            }
            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM { VOUCHERVALUEROW_VIEW} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

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

        public async Task<BillInfoModel> GetVoucherBillInfo(int voucherTypeId, long fId)
        {
            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
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
                FROM {VOUCHERVALUEROW_VIEW} r 

                WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM { VOUCHERVALUEROW_VIEW} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

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

        public async Task<long> CreateVoucherBill(int voucherTypeId, BillInfoModel data)
        {
            await ValidateSaleVoucherConfig(data?.Info, null);

            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0) data.Rows = new List<NonCamelCaseDictionary>(){
                new NonCamelCaseDictionary()
            };

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            // Lấy thông tin field
            var voucherAreaFields = await GetVoucherFields(voucherTypeId);
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, null);

            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            checkRows = data.Rows.Select(r => new ValidateRowModel(r, null)).ToList();

            // Validate info
            var requiredFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, voucherAreaFields);
            // Check refer
            await CheckReferAsync(voucherAreaFields, checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(voucherTypeId, checkInfo, checkRows, uniqueFields);
            // Check value
            CheckValue(checkInfo, checkRows, voucherAreaFields);

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get all fields
                var voucherFields = _purchaseOrderDBContext.VoucherField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Add);
                if (result.Code != 0)
                {
                    if (string.IsNullOrWhiteSpace(result.Message))
                        throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                    else
                    {
                        throw result.Message.BadRequest();

                    }
                }

                var billInfo = new VoucherBill()
                {
                    VoucherTypeId = voucherTypeId,
                    LatestBillVersion = 1,
                    SubsidiaryId = _currentContextService.SubsidiaryId,
                    IsDeleted = false
                };
                await _purchaseOrderDBContext.VoucherBill.AddAsync(billInfo);

                await _purchaseOrderDBContext.SaveChangesAsync();

                var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                await CreateBillVersion(voucherTypeId, billInfo, data, generateTypeLastValues);

                // After saving action (SQL)
                await ProcessActionAsync(voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Add);


                if (!string.IsNullOrWhiteSpace(data?.OutsideImportMappingData?.MappingFunctionKey))
                {
                    await _outsideMappingHelperService.MappingObjectCreate(data.OutsideImportMappingData.MappingFunctionKey, data.OutsideImportMappingData.ObjectId, EnumObjectType.VoucherBill, billInfo.FId);
                }

                await ConfirmCustomGenCode(generateTypeLastValues);

                trans.Commit();

                await _voucherDataActivityLog.LogBuilder(() => VoucherBillActivityLogMessage.Create)
                 .MessageResourceFormatDatas(voucherTypeInfo.Title, billInfo.BillCode)
                 .BillTypeId(voucherTypeId)
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

        private async Task<bool> CheckRequireFilter(Clause clause, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateVoucherField> voucherAreaFields, Dictionary<string, Dictionary<object, object>> sfValues, int? rowIndex, bool not = false)
        {
            bool? isRequire = null;
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    var field = voucherAreaFields.First(f => f.FieldName == singleClause.FieldName);
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
                                var result = await _purchaseOrderDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
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
                                var result = await _purchaseOrderDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
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
                        bool clauseResult = await CheckRequireFilter(arrClause.Rules.ElementAt(indx), info, rows, voucherAreaFields, sfValues, rowIndex, isNot);
                        isRequire = isRequire.HasValue ? isOr ? isRequire.Value || clauseResult : isRequire.Value && clauseResult : clauseResult;
                    }
                }
            }
            return isRequire.Value;
        }

        private async Task<(int Code, string Message, List<NonCamelCaseDictionary> ResultData)> ProcessActionAsync(string script, BillInfoModel data, Dictionary<string, EnumDataType> fields, EnumActionType action, long voucherBillId = 0)
        {
            List<NonCamelCaseDictionary> resultData = null;
            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(script))
            {
                DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                var parammeters = new List<SqlParameter>() {
                    new SqlParameter("@Action", (int)action),
                    resultParam,
                    messageParam,
                    new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.VoucherTableType" },
                    new SqlParameter("@VoucherBillId", voucherBillId)
                };

                resultData = (await _purchaseOrderDBContext.QueryDataTable(script, parammeters)).ConvertData();
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

        private async Task CheckRequired(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateVoucherField> requiredFields, List<ValidateVoucherField> voucherAreaFields)
        {
            var filters = requiredFields
                .Where(f => !string.IsNullOrEmpty(f.RequireFilters))
                .ToDictionary(f => f.FieldName, f => JsonConvert.DeserializeObject<Clause>(f.RequireFilters));

            string[] filterFieldNames = GetFieldInFilter(filters.Select(f => f.Value).ToArray());
            var sfFields = voucherAreaFields.Where(f => ((EnumFormType)f.FormTypeId).IsSelectForm() && filterFieldNames.Contains(f.FieldName)).ToList();
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
                    var data = await _purchaseOrderDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
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
                        if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, voucherAreaFields, sfValues, null)))
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
                            if (filterClause != null && !(await CheckRequireFilter(filterClause, info, rows, voucherAreaFields, sfValues, rowIndx - 1)))
                            {
                                continue;
                            }
                        }

                        row.Data.TryGetValue(field.FieldName, out string value);
                        if (string.IsNullOrEmpty(value))
                        {
                            throw new BadRequestException(VoucherErrorCode.RequiredFieldIsEmpty, new object[] { rowIndx, field.Title });
                        }
                    }
                }
            }
        }

        private async Task CheckUniqueAsync(int voucherTypeId, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateVoucherField> uniqueFields, long? voucherValueBillId = null)
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
                        await ValidUniqueAsync(voucherTypeId, new List<object>() { ((EnumDataType)field.DataTypeId).GetSqlValue(value) }, field, voucherValueBillId);
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
                            throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                        }
                        if (values.Count == 0)
                        {
                            continue;
                        }
                        // Checkin unique trong db
                        await ValidUniqueAsync(voucherTypeId, values, field, voucherValueBillId);
                    }
                }
            }
        }

        private async Task ValidUniqueAsync(int voucherTypeId, List<object> values, ValidateVoucherField field, long? voucherValueBillId = null)
        {
            var existSql = $"SELECT F_Id FROM vVoucherValueRow WHERE VoucherTypeId = {voucherTypeId} ";
            if (voucherValueBillId.HasValue)
            {
                existSql += $"AND VoucherBill_F_Id != {voucherValueBillId}";
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
            var result = await _purchaseOrderDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;

            if (isExisted)
            {
                throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
            }
        }

        private async Task CheckReferAsync(List<ValidateVoucherField> allFields, ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateVoucherField> selectFields)
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

        private async Task ValidReferAsync(List<ValidateVoucherField> allFields, ValidateRowModel checkData, ValidateRowModel info, ValidateVoucherField field, int? rowIndex)
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
                    if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                    {
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

            var result = await _purchaseOrderDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, value) };
                result = await _purchaseOrderDBContext.QueryDataTable(checkExistedReferSql, checkExistedReferParams.ToArray());
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(VoucherErrorCode.ReferValueNotFound, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(VoucherErrorCode.ReferValueNotValidFilter, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title + ": " + value });
                }
            }
        }

        private void CheckValue(ValidateRowModel info, List<ValidateRowModel> rows, List<ValidateVoucherField> categoryFields)
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

        private void ValidValueAsync(ValidateRowModel checkData, ValidateVoucherField field, int? rowIndex)
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
                throw new BadRequestException(VoucherErrorCode.VoucherValueInValid, new object[] { value?.JsonSerialize(), rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title });
            }


        }

        private void AppendSelectFields(ref StringBuilder sql, List<ValidateVoucherField> fields)
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

        public async Task<bool> UpdateVoucherBill(int voucherTypeId, long voucherValueBillId, BillInfoModel data)
        {
            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);

            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0) data.Rows = new List<NonCamelCaseDictionary>(){
                new NonCamelCaseDictionary()
            };

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));

            // Lấy thông tin field
            var voucherAreaFields = await GetVoucherFields(voucherTypeId);

            // Get changed info
            var infoSQL = new StringBuilder("SELECT TOP 1 ");
            var singleFields = voucherAreaFields.Where(f => !f.IsMultiRow).ToList();
            AppendSelectFields(ref infoSQL, singleFields);
            infoSQL.Append($" FROM {VOUCHERVALUEROW_VIEW} r WHERE VoucherTypeId = {voucherTypeId} AND VoucherBill_F_Id = {voucherValueBillId} AND {GlobalFilter()}");
            var currentInfo = (await _purchaseOrderDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData().FirstOrDefault();

            if (currentInfo == null)
            {
                throw BillNotFound.BadRequest();
            }

            await ValidateSaleVoucherConfig(data?.Info, currentInfo);

            NonCamelCaseDictionary futureInfo = data.Info;
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, CompareRow(currentInfo, futureInfo, singleFields));

            // Get changed rows
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            var rowsSQL = new StringBuilder("SELECT F_Id");
            var multiFields = voucherAreaFields.Where(f => f.IsMultiRow).ToList();
            if (multiFields.Count > 0) rowsSQL.Append(",");
            AppendSelectFields(ref rowsSQL, multiFields);
            rowsSQL.Append($" FROM {VOUCHERVALUEROW_VIEW} r WHERE VoucherBill_F_Id = {voucherValueBillId} AND {GlobalFilter()}");
            var currentRows = (await _purchaseOrderDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
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
            var requiredFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = voucherAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkInfo, checkRows, requiredFields, voucherAreaFields);
            // Check refer
            await CheckReferAsync(voucherAreaFields, checkInfo, checkRows, selectFields);
            // Check unique
            await CheckUniqueAsync(voucherTypeId, checkInfo, checkRows, uniqueFields, voucherValueBillId);
            // Check value
            CheckValue(checkInfo, checkRows, voucherAreaFields);

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _purchaseOrderDBContext.VoucherBill.FirstOrDefaultAsync(b => b.VoucherTypeId == voucherTypeId && b.FId == voucherValueBillId && b.SubsidiaryId == _currentContextService.SubsidiaryId);

                if (billInfo == null) throw BillNotFound.BadRequest();

                // Get all fields
                var voucherFields = _purchaseOrderDBContext.VoucherField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Update, billInfo.FId);
                if (result.Code != 0)
                {
                    if (string.IsNullOrWhiteSpace(result.Message))
                        throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                    else
                    {
                        throw result.Message.BadRequest();

                    }
                }

                await DeleteVoucherBillVersion(voucherTypeId, billInfo.FId, billInfo.LatestBillVersion);


                var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                billInfo.LatestBillVersion++;

                await CreateBillVersion(voucherTypeId, billInfo, data, generateTypeLastValues);

                await _purchaseOrderDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Update);

                await ConfirmCustomGenCode(generateTypeLastValues);

                trans.Commit();

                await _voucherDataActivityLog.LogBuilder(() => VoucherBillActivityLogMessage.Update)
                .MessageResourceFormatDatas(voucherTypeInfo.Title, billInfo.BillCode)
                .BillTypeId(voucherTypeId)
                .ObjectId(billInfo.FId)
                .JsonData(data.JsonSerialize())
                .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateVoucherBill");
                throw;
            }
        }

        public async Task<bool> UpdateMultipleVoucherBills(int voucherTypeId, string fieldName, object oldValue, object newValue, long[] fIds)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);

            if (fIds.Length == 0) throw ListBillsToUpdateIsEmpty.BadRequest();

            // Get field
            var field = _purchaseOrderDBContext.VoucherAreaField.Include(f => f.VoucherField).FirstOrDefault(f => f.VoucherField.FieldName == fieldName);
            if (field == null) throw FieldNotFound.BadRequest();

            object oldSqlValue;
            object newSqlValue;
            if (((EnumFormType)field.VoucherField.FormTypeId).IsSelectForm())
            {
                var refTableTitle = field.VoucherField.RefTableTitle.Split(',')[0];
                var categoryFields = await _httpCategoryHelperService.GetReferFields(new List<string>() { field.VoucherField.RefTableCode }, new List<string>() { refTableTitle, field.VoucherField.RefTableField });
                var refField = categoryFields.FirstOrDefault(f => f.CategoryFieldName == field.VoucherField.RefTableField);
                var refTitleField = categoryFields.FirstOrDefault(f => f.CategoryFieldName == refTableTitle);
                if (refField == null || refTitleField == null) throw FieldRefNotFound.BadRequest();
                var selectSQL = $"SELECT {field.VoucherField.RefTableField} FROM v{field.VoucherField.RefTableCode} WHERE {refTableTitle} = @ValueParam";

                if (oldValue != null)
                {
                    var oldResult = await _purchaseOrderDBContext.QueryDataTable(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldValue)) });
                    if (oldResult == null || oldResult.Rows.Count == 0) throw OldValueIsInvalid.BadRequest();
                    oldSqlValue = ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldResult.Rows[0][0]);
                }
                else
                {
                    oldSqlValue = DBNull.Value;
                }

                if (newValue != null)
                {
                    var newResult = await _purchaseOrderDBContext.QueryDataTable(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(newValue)) });
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
                oldSqlValue = ((EnumDataType)field.VoucherField.DataTypeId).GetSqlValue(oldValue);
                newSqlValue = ((EnumDataType)field.VoucherField.DataTypeId).GetSqlValue(newValue);
            }

            var singleFields = (await (
                from af in _purchaseOrderDBContext.VoucherAreaField
                join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
                select f.FieldName).ToListAsync()).ToHashSet();

            // Get bills by old value
            var sqlParams = new List<SqlParameter>();
            var dataSql = new StringBuilder(@$"

                SELECT     r.*
                FROM {VOUCHERVALUEROW_TABLE} r 

                WHERE r.VoucherTypeId = {voucherTypeId} AND r.IsDeleted = 0 AND r.VoucherBill_F_Id IN ({string.Join(',', fIds)}) AND {GlobalFilter()}");


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

            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());
            var updateBillIds = new HashSet<long>();

            // Update new value
            var dataTable = new DataTable(VOUCHERVALUEROW_TABLE);
            foreach (DataColumn column in data.Columns)
            {
                if (column.ColumnName != "F_Id")
                    dataTable.Columns.Add(column.ColumnName, column.DataType);
            }

            var oldBillDates = new Dictionary<long, DateTime?>();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];

                var billId = (long)row["VoucherBill_F_Id"];
                if (!updateBillIds.Contains(billId))
                {
                    updateBillIds.Add(billId);
                    oldBillDates.Add(billId, null);
                }

                var newRow = dataTable.NewRow();
                foreach (DataColumn column in data.Columns)
                {
                    var v = row[column];

                    if (column.ColumnName.Equals(PurchaseOrderConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) && !v.IsNullObject())
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
                var newDate = fieldName.Equals(PurchaseOrderConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) ? (newSqlValue as DateTime?) : null;

                await ValidateSaleVoucherConfig(newDate ?? oldBillDate.Value, oldBillDate.Value);
            }

            var bills = _purchaseOrderDBContext.VoucherBill.Where(b => updateBillIds.Contains(b.FId) && b.SubsidiaryId == _currentContextService.SubsidiaryId).ToList();
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                // Created bill version
                await _purchaseOrderDBContext.InsertDataTable(dataTable, true);
                using (var batch = _voucherDataActivityLog.BeginBatchLog())
                {
                    foreach (var bill in bills)
                    {
                        // Delete bill version
                        await DeleteVoucherBillVersion(voucherTypeId, bill.FId, bill.LatestBillVersion);

                        await _voucherDataActivityLog.LogBuilder(() => VoucherBillActivityLogMessage.UpdateMulti)
                         .MessageResourceFormatDatas(voucherTypeInfo.Title, field?.Title + " (" + field?.Title + ")", bill.BillCode)
                         .BillTypeId(voucherTypeId)
                         .ObjectId(bill.FId)
                         .JsonData(new { voucherTypeId, fieldName, oldValue, newValue, fIds }.JsonSerialize().JsonSerialize())
                         .CreateLog();

                        // Update last bill version
                        bill.LatestBillVersion++;
                    }

                    await _purchaseOrderDBContext.SaveChangesAsync();
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

        private string[] CompareRow(NonCamelCaseDictionary currentRow, NonCamelCaseDictionary futureRow, List<ValidateVoucherField> fields)
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


        private async Task<ITypeExecData> GetVoucherTypExecInfo(int voucherTypeId)
        {
            var global = await _voucherConfigService.GetVoucherGlobalSetting();
            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking().FirstOrDefaultAsync(t => t.VoucherTypeId == voucherTypeId);
            if (voucherTypeInfo == null) throw VoucherTypeNotFound.BadRequest();
            var info = _mapper.Map<VoucherTypeExecData>(voucherTypeInfo);
            info.GlobalSetting = global;
            return info;
        }

        public async Task<bool> DeleteVoucherBill(int voucherTypeId, long voucherBill_F_Id)
        {
            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);


            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _purchaseOrderDBContext.VoucherBill.FirstOrDefaultAsync(b => b.FId == voucherBill_F_Id && b.SubsidiaryId == _currentContextService.SubsidiaryId);

                if (billInfo == null) throw BillNotFound.BadRequest();

                var voucherAreaFields = new List<ValidateVoucherField>();

                // Get current data
                BillInfoModel data = new BillInfoModel();
                // Lấy thông tin field
                voucherAreaFields = await GetVoucherFields(voucherTypeId);

                // Get changed row info
                var infoSQL = new StringBuilder("SELECT TOP 1 ");
                var singleFields = voucherAreaFields.Where(f => !f.IsMultiRow).ToList();
                for (int indx = 0; indx < singleFields.Count; indx++)
                {
                    if (indx > 0)
                    {
                        infoSQL.Append(", ");
                    }
                    infoSQL.Append(singleFields[indx].FieldName);
                }
                infoSQL.Append($" FROM {VOUCHERVALUEROW_VIEW} r WHERE VoucherBill_F_Id = {voucherBill_F_Id} AND {GlobalFilter()}");
                var infoLst = (await _purchaseOrderDBContext.QueryDataTable(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();

                data.Info = infoLst.Count != 0 ? infoLst[0].ToNonCamelCaseDictionary(f => f.Key, f => f.Value) : new NonCamelCaseDictionary();
                if (!string.IsNullOrEmpty(voucherTypeInfo.BeforeSaveActionExec) || !string.IsNullOrEmpty(voucherTypeInfo.AfterSaveActionExec))
                {
                    var rowsSQL = new StringBuilder("SELECT ");
                    var multiFields = voucherAreaFields.Where(f => f.IsMultiRow).ToList();
                    for (int indx = 0; indx < multiFields.Count; indx++)
                    {
                        if (indx > 0)
                        {
                            rowsSQL.Append(", ");
                        }
                        rowsSQL.Append(multiFields[indx].FieldName);
                    }
                    rowsSQL.Append($" FROM {VOUCHERVALUEROW_VIEW} r WHERE VoucherBill_F_Id = {voucherBill_F_Id} AND {GlobalFilter()}");
                    var currentRows = (await _purchaseOrderDBContext.QueryDataTable(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
                    data.Rows = currentRows.Select(r => r.ToNonCamelCaseDictionary(f => f.Key, f => f.Value.ToString())).ToArray();
                }
                await ValidateSaleVoucherConfig(null, data?.Info);

                // Get all fields
                var voucherFields = _purchaseOrderDBContext.VoucherField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result =  await ProcessActionAsync(voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Delete, billInfo.FId);

                if (result.Code != 0)
                {
                    if (string.IsNullOrWhiteSpace(result.Message))
                        throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);
                    else
                    {
                        throw result.Message.BadRequest();

                    }
                }

                await DeleteVoucherBillVersion(voucherTypeId, billInfo.FId, billInfo.LatestBillVersion);

                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Delete);

                //await _outsideImportMappingService.MappingObjectDelete(billInfo.FId);

                trans.Commit();

                await _voucherDataActivityLog.LogBuilder(() => VoucherBillActivityLogMessage.Delete)
                        .MessageResourceFormatDatas(voucherTypeInfo.Title, billInfo.BillCode)
                        .BillTypeId(voucherTypeId)
                        .ObjectId(billInfo.FId)
                        .JsonData(data.JsonSerialize().JsonSerialize())
                        .CreateLog();

                await _outsideMappingHelperService.MappingObjectDelete(EnumObjectType.VoucherBill, billInfo.FId);

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteBill");
                throw;
            }
        }

        private async Task FillGenerateColumn(long? fId, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues, Dictionary<string, ValidateVoucherField> fields, IList<NonCamelCaseDictionary> rows)
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
                            currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.VoucherTypeRow, EnumObjectType.VoucherAreaField, field.VoucherAreaFieldId, fId, code, ngayCtValue);

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
            foreach (var (_, value) in generateTypeLastValues)
            {
                await _customGenCodeHelperService.ConfirmCode(value);
            }
        }

        private async Task CreateBillVersion(int voucherTypeId, VoucherBill billInfo, BillInfoModel data, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues)
        {
            var fields = (await GetVoucherFields(voucherTypeId)).ToDictionary(f => f.FieldName, f => f);

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

            var dataTable = new DataTable(VOUCHERVALUEROW_TABLE);

            dataTable.Columns.Add("VoucherTypeId", typeof(int));
            dataTable.Columns.Add("VoucherBill_F_Id", typeof(long));
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
                var dataRow = NewVoucherBillVersionRow(dataTable, voucherTypeId, billInfo.FId, billInfo.LatestBillVersion, false);

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
                        var butToan = item.Key.Substring(PurchaseOrderConstants.TAI_KHOAN_CO_PREFIX.Length);
                        var tkNo = PurchaseOrderConstants.TAI_KHOAN_NO_PREFIX + butToan;
                        if (data.Info.Keys.Any(k => k.Equals(tkNo, StringComparison.OrdinalIgnoreCase)))
                        {
                            ignoreCopyInfoValues.Add(item.Key);
                            continue;
                        }
                    }

                    if (item.Key.IsTkNoColumn())
                    {
                        var butToan = item.Key.Substring(PurchaseOrderConstants.TAI_KHOAN_NO_PREFIX.Length);
                        var tkCo = PurchaseOrderConstants.TAI_KHOAN_CO_PREFIX + butToan;
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

            //Create addition reciprocal sales
            if (data.Info.Any(k => k.Key.IsVndColumn() && decimal.TryParse(k.Value?.ToString(), out var value) && value != 0))
            {
                var dataRow = NewVoucherBillVersionRow(dataTable, voucherTypeId, billInfo.FId, billInfo.LatestBillVersion, true);

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

            await _purchaseOrderDBContext.InsertDataTable(dataTable);
        }

        private DataRow NewVoucherBillVersionRow(DataTable dataTable, int voucherTypeId, long voucherBill_F_Id, int billVersionId, bool isBillEntry)
        {
            var dataRow = dataTable.NewRow();

            dataRow["VoucherTypeId"] = voucherTypeId;
            dataRow["VoucherBill_F_Id"] = voucherBill_F_Id;
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
            for (var i = 0; i <= PurchaseOrderConstants.MAX_COUPLE_RECIPROCAL; i++)
            {
                var credit_column_name = PurchaseOrderConstants.TAI_KHOAN_CO_PREFIX + i;
                var debit_column_name = PurchaseOrderConstants.TAI_KHOAN_NO_PREFIX + i;
                var money_column_name = PurchaseOrderConstants.THANH_TIEN_VND_PREFIX + i;

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

        private async Task DeleteVoucherBillVersion(int voucherTypeId, long voucherBill_F_Id, int billVersionId)
        {
            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherValueRow_Delete_Version", new[] {
                    new SqlParameter("@VoucherTypeId", voucherTypeId),
                    new SqlParameter("@VoucherBill_F_Id", voucherBill_F_Id),
                    new SqlParameter("@BillVersion", billVersionId),
                    new SqlParameter("@UserId", _currentContextService.UserId),
                    new SqlParameter("@ResStatus", voucherTypeId){ Direction = ParameterDirection.Output },
                }, true);
        }

        private async Task<List<ValidateVoucherField>> GetVoucherFields(int voucherTypeId, int? areaId = null)
        {
            var area = _purchaseOrderDBContext.VoucherArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.VoucherAreaId == areaId);

            }
            return await (from af in _purchaseOrderDBContext.VoucherAreaField
                          join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                          join a in area on af.VoucherAreaId equals a.VoucherAreaId
                          where af.VoucherTypeId == voucherTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly //&& f.FieldName != PurchaseOrderConstants.F_IDENTITY
                          orderby a.SortOrder, af.SortOrder
                          select new ValidateVoucherField
                          {
                              VoucherAreaFieldId = af.VoucherAreaFieldId,
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
                              RequireFilters = af.RequireFilters,
                              IsReadOnly = f.IsReadOnly,
                              IsHidden = af.IsHidden
                          }).ToListAsync();
        }


        public async Task<CategoryNameModel> GetFieldDataForMapping(int voucherTypeId, int? areaId)
        {
            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking().FirstOrDefaultAsync(t => t.VoucherTypeId == voucherTypeId);


            // Lấy thông tin field
            var fields = await GetVoucherFields(voucherTypeId, areaId);

            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.InputTypeId,
                CategoryCode = voucherTypeInfo.VoucherTypeCode,
                CategoryTitle = voucherTypeInfo.Title,
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
                    IsRequired = field.IsRequire
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
                            IsRequired = false
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
        public async Task<bool> ImportVoucherBillFromMapping(int voucherTypeId, ImportExcelMapping mapping, Stream stream)
        {
            var voucherType = await GetVoucherTypExecInfo(voucherTypeId);

            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = await GetVoucherFields(voucherTypeId);

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var requiredField = fields.FirstOrDefault(f => f.IsRequire && string.IsNullOrWhiteSpace(f.RequireFilters) && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

            if (requiredField != null) throw FieldRequired.BadRequestFormat(requiredField.Title);

            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
            var referTableNames = fields.Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();

            var referFields = await _httpCategoryHelperService.GetReferFields(referTableNames, referMapingFields.Select(f => f.RefFieldName).ToList());


            var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == AccountantConstants.BILL_CODE);
            if (columnKey == null)
            {
                throw BillCodeError.BadRequest();
            }

            var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();

            var groups = data.Rows.Select((r, i) => new
            {
                Data = r,
                Index = i + mapping.FromRow
            })
                .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                .GroupBy(r => r.Data[columnKey.Column]);
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
                    throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }
                // Checkin unique trong db
                var existSql = $"SELECT F_Id FROM {VOUCHERVALUEROW_VIEW} WHERE VoucherTypeId = {voucherTypeId} ";

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
                var result = await _purchaseOrderDBContext.QueryDataTable(existSql, sqlParams.ToArray());
                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                {
                    throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
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
                    foreach (var mappingField in mapping.MappingFields)
                    {
                        var field = fields.FirstOrDefault(f => f.FieldName == mappingField.FieldName);

                        // Validate mapping required
                        if (field == null && mappingField.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty) throw new BadRequestException(GeneralCode.ItemNotFound, $"Trường dữ liệu {mappingField.FieldName} không tìm thấy");
                        if (field == null) continue;
                        if (!field.IsMultiRow && rowIndx > 0) continue;

                        string value = null;
                        if (row.Data.ContainsKey(mappingField.Column))
                            value = row.Data[mappingField.Column]?.ToString();
                        // Validate require
                        if (string.IsNullOrWhiteSpace(value) && field.IsRequire && string.IsNullOrWhiteSpace(field.RequireFilters)) throw new BadRequestException(VoucherErrorCode.RequiredFieldIsEmpty, new object[] { row.Index, field.Title });

                        if (string.IsNullOrWhiteSpace(value)) continue;
                        value = value.Trim();
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
                                    throw new BadRequestException(VoucherErrorCode.VoucherValueInValid, new object[] { value?.JsonSerialize(), row.Index, field.Title });
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
                                    if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                                    {
                                        filterValue = filterValue.Substring(start, length);
                                    }
                                    if (string.IsNullOrEmpty(filterValue)) throw new BadRequestException(GeneralCode.InvalidParams, $"Cần thông tin {fieldName} trước thông tin {field.FieldName}");
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

                            var referData = await _purchaseOrderDBContext.QueryDataTable(referSql, referParams.ToArray());
                            if (referData == null || referData.Rows.Count == 0)
                            {
                                // Check tồn tại
                                var checkExistedReferSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                                referData = await _purchaseOrderDBContext.QueryDataTable(checkExistedReferSql, checkExistedReferParams.ToArray());
                                if (referData == null || referData.Rows.Count == 0)
                                {
                                    throw new BadRequestException(VoucherErrorCode.ReferValueNotFound, new object[] { row.Index, field.Title + ": " + value });
                                }
                                else
                                {
                                    throw new BadRequestException(VoucherErrorCode.ReferValueNotValidFilter, new object[] { row.Index, field.Title + ": " + value });
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
                    rows.Add(mapRow);
                }
                var billInfo = new BillInfoModel
                {
                    Info = info,
                    Rows = rows.Count > 0 ? rows.ToArray() : new NonCamelCaseDictionary[]
                    {
                        new NonCamelCaseDictionary()
                    }
                };

                await ValidateSaleVoucherConfig(billInfo?.Info, null);

                bills.Add(billInfo);
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                    // Get all fields
                    var voucherFields = _purchaseOrderDBContext.VoucherField
                     .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                     .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

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
                        await ProcessActionAsync(voucherType.BeforeSaveActionExec, bill, voucherFields, EnumActionType.Add);

                        var billInfo = new VoucherBill()
                        {
                            VoucherTypeId = voucherTypeId,
                            LatestBillVersion = 1,
                            SubsidiaryId = _currentContextService.SubsidiaryId,
                            IsDeleted = false
                        };

                        await _purchaseOrderDBContext.VoucherBill.AddAsync(billInfo);

                        await _purchaseOrderDBContext.SaveChangesAsync();

                        await CreateBillVersion(voucherTypeId, billInfo, bill, generateTypeLastValues);

                        // After saving action (SQL)
                        await ProcessActionAsync(voucherType.AfterSaveActionExec, bill, voucherFields, EnumActionType.Add);
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

        public async Task<(MemoryStream Stream, string FileName)> ExportVoucherBill(int voucherTypeId, long fId)
        {
            var dataSql = @$"
                SELECT     r.*
                FROM {VOUCHERVALUEROW_VIEW} r 
                WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());
            var billEntryInfoSql = $"SELECT r.* FROM { VOUCHERVALUEROW_VIEW} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND r.IsBillEntry = 1";
            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

            var info = (billEntryInfo.Rows.Count > 0 ? billEntryInfo.ConvertFirstRowData() : data.ConvertFirstRowData()).ToNonCamelCaseDictionary();
            var rows = data.ConvertData();

            var voucherType = _purchaseOrderDBContext.VoucherType
                .Include(i => i.VoucherArea)
                .ThenInclude(a => a.VoucherAreaField)
                .ThenInclude(f => f.VoucherField)
                .Where(i => i.VoucherTypeId == voucherTypeId)
                .FirstOrDefault();

            if (voucherType == null) throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);

            var selectFormFields = (from iaf in _purchaseOrderDBContext.VoucherAreaField
                                    join itf in _purchaseOrderDBContext.VoucherField on iaf.VoucherFieldId equals itf.VoucherFieldId
                                    where iaf.VoucherTypeId == voucherTypeId && DataTypeConstants.SELECT_FORM_TYPES.Contains((EnumFormType)itf.FormTypeId)
                                    select new
                                    {
                                        itf.RefTableTitle,
                                        itf.RefTableCode
                                    }).ToList();

            var refDataTypes = (await _httpCategoryHelperService.GetReferFields(selectFormFields.Select(f => f.RefTableCode).ToList(), selectFormFields.Select(f => f.RefTableTitle.Split(',')[0]).ToList()))
                .Distinct()
                .ToDictionary(f => new { f.CategoryFieldName, f.CategoryCode }, f => (EnumDataType)f.DataTypeId);

            var writer = new ExcelWriter();
            int endRow = 0;

            var billCode = string.Empty;
            // Write area
            foreach (var area in voucherType.VoucherArea.OrderBy(a => a.SortOrder))
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
                        foreach (var field in area.VoucherAreaField.Where(f => f.Column == (collumIndx + 1)).OrderBy(f => f.SortOrder))
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
                            var fieldName = ((EnumFormType)field.VoucherField.FormTypeId).IsJoinForm() ? $"{field.VoucherField.FieldName}_{field.VoucherField.RefTableTitle.Split(",")[0]}" : field.VoucherField.FieldName;
                            var dataType = ((EnumFormType)field.VoucherField.FormTypeId).IsJoinForm() ? refDataTypes[new { CategoryFieldName = field.VoucherField.RefTableTitle.Split(",")[0], CategoryCode = field.VoucherField.RefTableCode }] : (EnumDataType)field.VoucherField.DataTypeId;
                            if (info.ContainsKey(fieldName))
                                row[collumIndx * 2 + 1] = new ExcelCell
                                {
                                    Value = dataType.GetSqlValue(info[fieldName]),
                                    Type = dataType.GetExcelType()
                                };
                            rowIndx++;
                        }
                    }

                    var uniqField = area.VoucherAreaField.FirstOrDefault(f => f.IsUnique)?.VoucherField.FieldName ?? PurchaseOrderConstants.BILL_CODE;
                    info.TryGetValue(uniqField, out billCode);
                }
                else
                {
                    foreach (var field in area.VoucherAreaField.OrderBy(f => f.SortOrder))
                    {
                        table.Columns.Add(field.Title);
                    }
                    var sumCalc = new List<int>();
                    foreach (var row in rows)
                    {
                        ExcelRow tbRow = table.NewRow();
                        int columnIndx = 0;
                        foreach (var field in area.VoucherAreaField.OrderBy(f => f.SortOrder))
                        {
                            if (field.IsCalcSum) sumCalc.Add(columnIndx);
                            var fieldName = ((EnumFormType)field.VoucherField.FormTypeId).IsJoinForm() ? $"{field.VoucherField.FieldName}_{field.VoucherField.RefTableTitle.Split(",")[0]}" : field.VoucherField.FieldName;
                            var dataType = ((EnumFormType)field.VoucherField.FormTypeId).IsJoinForm() ? refDataTypes[new { CategoryFieldName = field.VoucherField.RefTableTitle.Split(",")[0], CategoryCode = field.VoucherField.RefTableCode }] : (EnumDataType)field.VoucherField.DataTypeId;
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

            var fileName = $"{voucherType.VoucherTypeCode}_{billCode}.xlsx";

            MemoryStream stream = await writer.WriteToStream();
            return (stream, fileName);
        }

        public async Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow)
        {
            var voucherReferToFields = _purchaseOrderDBContext.VoucherField
                .Where(f => f.RefTableCode == categoryCode && fieldNames.Contains(f.RefTableField)).ToList();
            if (categoryRow == null)
            {
                // Check khi xóa cả danh mục
                return _purchaseOrderDBContext.VoucherField.Any(f => f.RefTableCode == categoryCode);
            }
            else
            {
                // Check khi xóa dòng trong danh mục
                // check bill refer
                foreach (var field in fieldNames)
                {
                    categoryRow.TryGetValue(field, out object value);
                    if (value == null) continue;

                    foreach (var referToField in voucherReferToFields.Where(f => f.RefTableField == field))
                    {
                        //var v = ((EnumDataType)referToField.DataTypeId).GetSqlValue(value);

                        var existSql = $"SELECT tk.F_Id FROM {VOUCHERVALUEROW_VIEW} tk WHERE tk.{referToField.FieldName} = @value;";
                        var result = await _purchaseOrderDBContext.QueryDataTable(existSql, new[] { new SqlParameter("@value", value) });
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

        private IList<ReferFieldModel> GetRefFields(IList<ReferFieldModel> fields)
        {
            return fields.Where(x => !x.IsHidden && x.DataTypeId != (int)EnumDataType.Boolean && !((EnumDataType)x.DataTypeId).IsTimeType())
                 .ToList();
        }

        private string GetTitleCategoryField(ValidateVoucherField field)
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
        private object ExtractBillDate(NonCamelCaseDictionary info)
        {
            object oldDateValue = null;

            info?.TryGetValue(PurchaseOrderConstants.BILL_DATE, out oldDateValue);
            return EnumDataType.Date.GetSqlValue(oldDateValue);
        }

        private async Task ValidateSaleVoucherConfig(NonCamelCaseDictionary info, NonCamelCaseDictionary oldInfo)
        {
            var billDate = ExtractBillDate(info);
            var oldDate = ExtractBillDate(oldInfo);

            await ValidateSaleVoucherConfig(billDate, oldDate);
        }

        private async Task ValidateSaleVoucherConfig(object billDate, object oldDate)
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
                await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw BillDateMustBeGreaterThanClosingDate.BadRequest();
            }
        }

        private string GlobalFilter()
        {
            return $"r.SubsidiaryId = { _currentContextService.SubsidiaryId}";
        }

        public async Task<BillInfoModel> GetPackingListInfo(int voucherTypeId, long voucherBill_BHXKId)
        {
            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly
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
                FROM {VOUCHERVALUEROW_VIEW} r 

                WHERE r.so_bh_xk = {voucherBill_BHXKId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM { VOUCHERVALUEROW_VIEW} r WHERE r.VoucherBill_F_Id = {voucherBill_BHXKId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTable(billEntryInfoSql, Array.Empty<SqlParameter>());

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


        public async Task<PageDataTable> OrderDetailByPurchasingRequest(string keyword, long? fromDate, long? toDate, bool? isCreatedPurchasingRequest, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var total = new SqlParameter("@Total", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var data = await _purchaseOrderDBContext.ExecuteDataProcedure("asp_OrderDetailByPurchasingRequest",
                new[]
                {
                   new SqlParameter("@Keyword", EnumDataType.Text.GetSqlValue(keyword)),
                   new SqlParameter("@FromDate",  EnumDataType.Date.GetSqlValue(fromDate?.UnixToDateTime())),
                   new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate?.UnixToDateTime())),
                   new SqlParameter("@IsCreatedPurchasingRequest", EnumDataType.Boolean.GetSqlValue(isCreatedPurchasingRequest)),
                   new SqlParameter("@Page", page),
                   new SqlParameter("@Size", size),
                   total
                });

            return (data, (total.Value as long?).GetValueOrDefault());
        }


        public async Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes(IList<string> orderCodes)
        {
            // var total = new SqlParameter("@Total", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var data = await _purchaseOrderDBContext.ExecuteDataProcedure("asp_OrderGetByCodes",
                new[]
                {
                   orderCodes.ToSqlParameter("@OrderCodes")
                });

            return data.ConvertData<VoucherOrderDetailSimpleEntity>().AsQueryable().ProjectTo<VoucherOrderDetailSimpleModel>(_mapper.ConfigurationProvider).ToList();
        }

        public async Task<IList<NonCamelCaseDictionary>> OrderRowsByCodes(IList<string> orderCodes)
        {
            // var total = new SqlParameter("@Total", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var data = await _purchaseOrderDBContext.ExecuteDataProcedure("asp_OrderRowsGetByCodes",
                new[]
                {
                   orderCodes.ToSqlParameter("@OrderCodes")
                });

            return data.ConvertData();
        }

        public async Task<IList<NonCamelCaseDictionary>> OrderDetails(IList<long> fIds)
        {
            // var total = new SqlParameter("@Total", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var data = await _purchaseOrderDBContext.ExecuteDataProcedure("asp_OrderDetailInfo_ByFIds",
                new[]
                {
                   fIds.ToSqlParameter("@F_Ids")
                });

            return data.ConvertData();
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

            public ValidateRowModel(NonCamelCaseDictionary Data, string[] CheckFields)
            {
                this.Data = Data;
                this.CheckFields = CheckFields;
            }
        }

        protected class ValidateVoucherField
        {
            public int VoucherAreaFieldId { get; set; }
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
            public bool IsReadOnly { get; set; }
            public bool IsHidden { get; set; }
        }
    }
}
