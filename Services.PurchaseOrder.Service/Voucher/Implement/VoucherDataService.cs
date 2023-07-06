using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.GlobalObject;
using Verp.Resources.PurchaseOrder.Voucher;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.Voucher;
using static Verp.Resources.PurchaseOrder.Voucher.VoucherDataValidationMessage;
using static VErp.Commons.Library.EvalUtils;
using static VErp.Commons.Library.ExcelReader;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherDataService : BillDateValidateionServiceAbstract, IVoucherDataService
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
        private readonly ILongTaskResourceLockService longTaskResourceLockService;
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
            , ILongTaskResourceLockService longTaskResourceLockService
            ) : base(purchaseOrderDBContext)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _voucherConfigService = voucherConfigService;
            this.longTaskResourceLockService = longTaskResourceLockService;
            _voucherDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.VoucherBill);
        }

        public async Task<PageDataTable> GetVoucherBills(int voucherTypeId, bool isMultiRow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var viewName = await GetVoucherViewName(voucherTypeId);

            var viewInfo = await _purchaseOrderDBContext.VoucherTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();
            var voucherTypeViewId = viewInfo?.VoucherTypeViewId;
            var fields = (await (
                from af in _purchaseOrderDBContext.VoucherAreaField
                join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                where af.VoucherTypeId == voucherTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                select new { a.VoucherAreaId, af.VoucherAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.FormTypeId, f.DataTypeId, a.IsMultiRow, af.IsCalcSum }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);
            var viewFields = await (
                from f in _purchaseOrderDBContext.VoucherTypeViewField
                where f.VoucherTypeViewId == voucherTypeViewId
                select f
            ).ToListAsync();
            var whereCondition = new StringBuilder();

            var sqlParams = new List<SqlParameter>() {
                new SqlParameter("@VoucherTypeId",voucherTypeId)
            };

            whereCondition.Append($" r.VoucherTypeId = @VoucherTypeId AND {GlobalFilter()}");

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
                    var viewField = viewFields.FirstOrDefault(f => f.VoucherTypeViewFieldId == filter.Key);
                    if (viewField == null) continue;
                    var value = filter.Value;
                    if (value.IsNullOrEmptyObject()) continue;
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
                            suffix = filterClause.FilterClauseProcess(viewName, "r", whereCondition, sqlParams, suffix, false, value);
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
                suffix = columnsFilters.FilterClauseProcess(viewName, "r", whereCondition, sqlParams, suffix);
            }


            var fieldToSelect = fields.Values.Where(f => f.IsMultiRow == isMultiRow || isMultiRow).ToList();

            var sumCols = fieldToSelect.Where(c => c.IsCalcSum).ToList();

            var sumSql = string.Join(", ", sumCols.Select(c => $"SUM(r.{c.FieldName}) AS {c.FieldName}").ToArray());
            if (!string.IsNullOrWhiteSpace(sumSql))
            {
                sumSql = ", " + sumSql;
            }

            string totalSql;

            if (isMultiRow)
            {
                totalSql = @$"SELECT COUNT(0) as Total {sumSql} FROM {viewName} r WHERE {whereCondition}";
            }
            else
            {
                totalSql = @$"
                    SELECT COUNT(0) Total {sumSql} FROM (
                        SELECT r.VoucherBill_F_Id {sumSql} FROM {viewName} r WHERE {whereCondition}
                        GROUP BY r.VoucherBill_F_Id
                    ) r
                ";
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

            var table = await _purchaseOrderDBContext.QueryDataTableRaw(totalSql, sqlParams.ToArray());
            var total = 0;
            var additionResults = new Dictionary<string, decimal>();
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
                foreach (var col in sumCols)
                {
                    var sum = (table.Rows[0][col.FieldName] as decimal?).GetValueOrDefault();
                    additionResults.Add(col.FieldName, sum);
                }
            }
            var selectColumn = string.Join(",", selectColumns.Select(c => $"r.[{c}]"));

            string dataSql;
            if (isMultiRow)
            {
                dataSql = @$"
                 
                    SELECT r.VoucherBill_F_Id, r.F_Id BillDetailId {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                    FROM {viewName} r
                    WHERE {whereCondition}
               
                    ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }
            else
            {
                dataSql = @$"
                 ;WITH tmp AS (
                    SELECT r.VoucherBill_F_Id, MAX(F_Id) as F_Id
                    FROM {viewName} r
                    WHERE {whereCondition}
                    GROUP BY r.VoucherBill_F_Id    
                )
                SELECT 
                    t.VoucherBill_F_Id
                    {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                FROM tmp t JOIN {viewName} r ON t.F_Id = r.F_Id
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }


            if (size >= 0)
            {
                dataSql += @$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size}
                ROWS ONLY";
            }
            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            return (data, total, additionResults);
        }

        public async Task<PageDataTable> GetVoucherBillInfoRows(int voucherTypeId, long fId, string orderByFieldName, bool asc, int page, int size)
        {
            var viewName = await GetVoucherViewName(voucherTypeId);

            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var sqlParams = new[]
            {
                new SqlParameter("@VoucherBill_F_Id", fId),
                new SqlParameter("@VoucherTypeId", voucherTypeId),
            };

            var totalSql = @$"SELECT COUNT(0) as Total FROM {viewName} r WHERE r.VoucherBill_F_Id = @VoucherBill_F_Id AND r.VoucherTypeId = @VoucherTypeId AND {GlobalFilter()} AND r.IsBillEntry = 0";

            var table = await _purchaseOrderDBContext.QueryDataTableRaw(totalSql, sqlParams);

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
                FROM {viewName} r 

                WHERE r.VoucherBill_F_Id = @VoucherBill_F_Id AND r.VoucherTypeId = @VoucherTypeId AND {GlobalFilter()} AND r.IsBillEntry = 0

                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}

            ";
            if (size > 0)
            {
                dataSql += @$"
                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
            ";
            }
            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, sqlParams.CloneSqlParams());

            var billEntryInfoSql = $"SELECT r.* FROM {viewName} r WHERE r.VoucherBill_F_Id = @VoucherBill_F_Id AND r.VoucherTypeId = @VoucherTypeId AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTableRaw(billEntryInfoSql, sqlParams.CloneSqlParams());

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

        public async Task<IDictionary<long, BillInfoModel>> GetListVoucherBillInfoRows(int voucherTypeId, IList<long> fIds)
        {
            var viewName = await GetVoucherViewName(voucherTypeId);

            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var dataSql = @$"

                SELECT     r.*
                FROM {viewName} r 
                JOIN @FIds v ON r.VoucherBill_F_Id = v.[Value]
                WHERE r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0

            ";

            var data = (await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, new[] { fIds.ToSqlParameter("@FIds") })).ConvertData();

            var billEntryInfoSql = @$"

                SELECT     r.*
                FROM {viewName} r 
                JOIN @FIds v ON r.VoucherBill_F_Id = v.[Value]
                WHERE r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1

            ";
            var billEntryInfos = (await _purchaseOrderDBContext.QueryDataTableRaw(billEntryInfoSql, new[] { fIds.ToSqlParameter("@FIds") })).ConvertData();

            var lst = new Dictionary<long, BillInfoModel>();
            foreach (var fId in fIds)
            {
                var result = new BillInfoModel();

                var rows = data.Where(r => (long)r["VoucherBill_F_Id"] == fId).ToList();

                var billEntryInfo = billEntryInfos.FirstOrDefault(b => (long)b["VoucherBill_F_Id"] == fId);
                result.Info = billEntryInfo;
                if (billEntryInfo != null && billEntryInfo.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        foreach (var k in row.Keys)
                        {
                            if (singleFields.Contains(k))
                            {
                                row[k] = billEntryInfo[k];
                            }
                        }
                    }
                }
                else
                {
                    result.Info = rows.FirstOrDefault()?.CloneNew();
                }

                result.Rows = rows;
                lst.Add(fId, result);
            }
            return lst;
        }

        public async Task<BillInfoModel> GetVoucherBillInfo(int voucherTypeId, long fId)
        {
            var viewName = await GetVoucherViewName(voucherTypeId);

            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
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
                FROM {viewName} r 

                WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM {viewName} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTableRaw(billEntryInfoSql, Array.Empty<SqlParameter>());

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
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(voucherTypeId, voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Add);
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
                    BillCode = Guid.NewGuid().ToString(),
                    IsDeleted = false
                };
                await _purchaseOrderDBContext.VoucherBill.AddAsync(billInfo);

                await _purchaseOrderDBContext.SaveChangesAsync();


                var listGenerateCodeCtx = new List<IGenerateCodeContext>();

                await CreateBillVersion(voucherTypeId, billInfo, data, listGenerateCodeCtx);

                // After saving action (SQL)
                await ProcessActionAsync(voucherTypeId, voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Add);


                if (!string.IsNullOrWhiteSpace(data?.OutsideImportMappingData?.MappingFunctionKey))
                {
                    await _outsideMappingHelperService.MappingObjectCreate(data.OutsideImportMappingData.MappingFunctionKey, data.OutsideImportMappingData.ObjectId, EnumObjectType.VoucherBill, billInfo.FId);
                }

                trans.Commit();
                await ConfirmIGenerateCodeContext(listGenerateCodeCtx);

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
                                var result = await _purchaseOrderDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray());
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
                                var result = await _purchaseOrderDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray());
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

        private async Task<(int Code, string Message, List<NonCamelCaseDictionary> ResultData)> ProcessActionAsync(int voucherTypeId, string script, BillInfoModel data, Dictionary<string, EnumDataType> fields, EnumActionType action, long voucherBillId = 0)
        {
            List<NonCamelCaseDictionary> resultData = null;
            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(script))
            {
                DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                var parammeters = new List<SqlParameter>() {
                    new SqlParameter("@Action", (int)action),
                    new SqlParameter("@VoucherTypeId", voucherTypeId),
                    resultParam,
                    messageParam,
                    new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.VoucherTableType" },
                    new SqlParameter("@VoucherBillId", voucherBillId)
                };

                resultData = (await _purchaseOrderDBContext.QueryDataTableRaw(script, parammeters)).ConvertData();
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
                    var data = await _purchaseOrderDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray());
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
                        if (filterClause != null)
                        {
                            if(!await CheckRequireFilter(filterClause, info, rows, voucherAreaFields, sfValues, null))
                                continue;
                            else
                                throw new BadRequestException(VoucherErrorCode.RequireValueNotValidFilter, new object[] { SingleRowArea, field.Title, field.RequireFiltersName });
                        }
                    }

                    info.Data.TryGetStringValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(VoucherErrorCode.RequiredFieldIsEmpty, new object[] { SingleRowArea, field.Title });
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
                            if (filterClause != null)
                            {
                                if(!await CheckRequireFilter(filterClause, info, rows, voucherAreaFields, sfValues, rowIndx - 1))
                                    continue;
                                else
                                    throw new BadRequestException(VoucherErrorCode.RequireValueNotValidFilter, new object[] { rowIndx, field.Title, field.RequireFiltersName });
                            }
                        }

                        row.Data.TryGetStringValue(field.FieldName, out string value);
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
                            var dupValue = values.GroupBy(v => v).Where(v => v.Count() > 1).FirstOrDefault()?.Key?.ToString();
                            throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, dupValue });
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
            var existSql = $"SELECT F_Id, {field.FieldName} FROM vVoucherValueRow WHERE VoucherTypeId = {voucherTypeId} ";
            if (voucherValueBillId.HasValue)
            {
                existSql += $"AND VoucherBill_F_Id <> {voucherValueBillId}";
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
            var result = await _purchaseOrderDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;

            if (isExisted)
            {
                throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, result.Rows[0][field.FieldName]?.ToString() });
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
            checkData.Data.TryGetStringValue(field.FieldName, out string textValue);
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
                    checkData.Data.TryGetStringValue(fieldName, out string filterValue);
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        info.Data.TryGetStringValue(fieldName, out filterValue);
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

                    try
                    {
                        var parameters = checkData.Data?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);
                        foreach (var (key, val) in info.Data.Where(d => !d.Value.IsNullOrEmptyObject() && !parameters.ContainsKey(d.Key)))
                        {
                            parameters.Add(key, val);
                        }

                        suffix = filterClause.FilterClauseProcess(tableName, tableName, whereCondition, sqlParams, suffix, refValues: parameters);

                    }
                    catch (EvalObjectArgException agrEx)
                    {
                        var fieldBefore = (allFields.FirstOrDefault(f => f.FieldName == agrEx.ParamName)?.Title) ?? agrEx.ParamName;
                        throw RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            if (whereCondition.Length > 0)
            {
                existSql += $" AND {whereCondition}";
            }

            var result = await _purchaseOrderDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, value) };
                result = await _purchaseOrderDBContext.QueryDataTableRaw(checkExistedReferSql, checkExistedReferParams.ToArray());
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(VoucherErrorCode.ReferValueNotFound, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(VoucherErrorCode.ReferValueNotValidFilter, new object[] { rowIndex.HasValue ? rowIndex.ToString() : "thông tin chung", field.Title + ": " + value, field.FiltersName });
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

            checkData.Data.TryGetStringValue(field.FieldName, out string value);
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
            var viewName = await GetVoucherViewName(voucherTypeId);

            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);


            // Validate multiRow existed
            if (data.Rows == null || data.Rows.Count == 0) data.Rows = new List<NonCamelCaseDictionary>(){
                new NonCamelCaseDictionary()
            };

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));

            // Lấy thông tin field
            var voucherAreaFields = await GetVoucherFields(voucherTypeId);

            // Get changed info
            var infoSQL = new StringBuilder("SELECT TOP 1 UpdatedDatetimeUtc, ");
            var singleFields = voucherAreaFields.Where(f => !f.IsMultiRow).ToList();
            AppendSelectFields(ref infoSQL, singleFields);
            infoSQL.Append($" FROM {viewName} r WHERE VoucherTypeId = {voucherTypeId} AND VoucherBill_F_Id = {voucherValueBillId} AND {GlobalFilter()}");
            var currentInfo = (await _purchaseOrderDBContext.QueryDataTableRaw(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData().FirstOrDefault();

            if (currentInfo == null)
            {
                throw BillNotFound.BadRequest();
            }

            data.Info.TryGetValue(GlobalFieldConstants.UpdatedDatetimeUtc, out object modelUpdatedDatetimeUtc);

            currentInfo.TryGetValue(GlobalFieldConstants.UpdatedDatetimeUtc, out object entityUpdatedDatetimeUtc);

            if (modelUpdatedDatetimeUtc?.ToString() != entityUpdatedDatetimeUtc?.ToString())
            {
                throw GeneralCode.DataIsOld.BadRequest();
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
            rowsSQL.Append($" FROM {viewName} r WHERE VoucherBill_F_Id = {voucherValueBillId} AND {GlobalFilter()}");
            var currentRows = (await _purchaseOrderDBContext.QueryDataTableRaw(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            foreach (var futureRow in data.Rows)
            {
                futureRow.TryGetStringValue("F_Id", out string futureValue);
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
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(voucherTypeId, voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Update, billInfo.FId);
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

                var lstCtx = new List<IGenerateCodeContext>();

                billInfo.LatestBillVersion++;

                await CreateBillVersion(voucherTypeId, billInfo, data, lstCtx);

                await _purchaseOrderDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(voucherTypeId, voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Update);

                trans.Commit();
                await ConfirmIGenerateCodeContext(lstCtx);

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

        public async Task<bool> UpdateMultipleVoucherBills(int voucherTypeId, string fieldName, object oldValue, object newValue, long[] billIds, long[] detailIds)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var voucherTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);

            if (billIds.Length == 0) throw ListBillsToUpdateIsEmpty.BadRequest();

            // Get field
            var field = _purchaseOrderDBContext.VoucherAreaField.Include(f => f.VoucherField).Include(f => f.VoucherArea).FirstOrDefault(f => f.VoucherArea.VoucherTypeId == voucherTypeId && f.VoucherField.FieldName == fieldName);
            if (field == null) throw FieldNotFound.BadRequest();

            if (!field.VoucherArea.IsMultiRow && detailIds?.Length > 0)
            {
                var checkUpdateMultipleFielsSql = $@"
                    ;WITH db AS(
                        SELECT r.VoucherBill_F_Id, COUNT(0) TotalDetail 
                            FROM {VOUCHERVALUEROW_TABLE} r 
                            WHERE r.VoucherTypeId = {voucherTypeId} AND r.IsDeleted = 0 
                                AND r.VoucherBill_F_Id IN(SELECT [Value] FROM @BillIds) 
                            GROUP BY r.VoucherBill_F_Id 
                    ),req AS (
                        SELECT r.VoucherBill_F_Id, COUNT(0) TotalDetail 
                            FROM {VOUCHERVALUEROW_TABLE} r 
                            WHERE r.VoucherTypeId = {voucherTypeId} AND r.IsDeleted = 0 
                                AND r.VoucherBill_F_Id IN (SELECT [Value] FROM @BillIds) 
                                AND r.F_Id  IN (SELECT [Value] FROM @DetailIds) 
                            GROUP BY r.VoucherBill_F_Id 
                    )
                    SELECT r.{PurchaseOrderConstants.BILL_CODE} 
                        FROM db 
                            LEFT JOIN req ON db.VoucherBill_F_Id = req.VoucherBill_F_Id
                            LEFT JOIN {VOUCHERVALUEROW_TABLE} r ON db.VoucherBill_F_Id = r.VoucherBill_F_Id
                    WHERE req.VoucherBill_F_Id IS NULL OR req.TotalDetail < db.TotalDetail
                    ";
                var invalids = await _purchaseOrderDBContext.QueryDataTableRaw(checkUpdateMultipleFielsSql, new[] { billIds.ToSqlParameter("@BillIds"), detailIds.ToSqlParameter("@DetailIds") });
                if (invalids.Rows.Count > 0)
                {
                    var billCode = invalids.Rows[0][PurchaseOrderConstants.BILL_CODE];
                    throw new BadRequestException($@"Trường dữ liệu ở vùng chung. Bạn cần lựa chọn tất cả các dòng chi tiết của chứng từ có mã {billCode}");
                }
            }

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
                    var oldResult = await _purchaseOrderDBContext.QueryDataTableRaw(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldValue)) });
                    if (oldResult == null || oldResult.Rows.Count == 0) throw OldValueIsInvalid.BadRequest();
                    oldSqlValue = ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldResult.Rows[0][0]);
                }
                else
                {
                    oldSqlValue = DBNull.Value;
                }

                if (newValue != null)
                {
                    var newResult = await _purchaseOrderDBContext.QueryDataTableRaw(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(newValue)) });
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
                where af.VoucherTypeId == voucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                select f.FieldName).ToListAsync()).ToHashSet();

            // Get bills by old value
            var sqlParams = new List<SqlParameter>()
            {
                billIds.ToSqlParameter("@BillIds")
            };

            var dataSql = new StringBuilder(@$"

                SELECT     r.*
                FROM {VOUCHERVALUEROW_TABLE} r 
                WHERE r.VoucherTypeId = {voucherTypeId} AND r.IsDeleted = 0 AND r.VoucherBill_F_Id IN (SELECT [Value] FROM @BillIds) AND {GlobalFilter()}");

            /**
           * NOTICE
           * Not add old condition to filter params, because we need to select all details of bill, and create new version
           * old data will be compare and replace at new version
           */


            //if (oldValue == null)
            //{
            //    dataSql.Append($" AND r.{fieldName} IS NULL");
            //}
            //else
            //{
            //    var paramName = $"@{fieldName}";
            //    dataSql.Append($" AND r.{fieldName} = {paramName}");
            //    sqlParams.Add(new SqlParameter(paramName, oldSqlValue));
            //}

            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql.ToString(), sqlParams.ToArray());
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

                    if (column.ColumnName.Equals(PurchaseOrderConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) && !v.IsNullOrEmptyObject())
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
                if (detailIds == null || detailIds.Length == 0 || detailIds.Contains((long)row["F_Id"]))
                {
                    var value = row[fieldName];

                    if (value.IsNullOrEmptyObject() && oldSqlValue.IsNullOrEmptyObject() || Equals(value, oldSqlValue) || value?.ToString() == oldSqlValue?.ToString())
                    {
                        newRow[fieldName] = newSqlValue;
                    }
                }

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
                         .JsonData(new { voucherTypeId, fieldName, oldValue, newValue, billIds }.JsonSerialize().JsonSerialize())
                         .CreateLog();

                        // Update last bill version
                        bill.LatestBillVersion++;
                    }

                    await _purchaseOrderDBContext.SaveChangesAsync();
                    trans.Commit();
                    await batch.CommitAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateMultipleVoucherBills");
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
            var viewName = await GetVoucherViewName(voucherTypeId);

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
                infoSQL.Append($" FROM {viewName} r WHERE VoucherBill_F_Id = {voucherBill_F_Id} AND {GlobalFilter()}");
                var infoLst = (await _purchaseOrderDBContext.QueryDataTableRaw(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();

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
                    rowsSQL.Append($" FROM {viewName} r WHERE VoucherBill_F_Id = {voucherBill_F_Id} AND {GlobalFilter()}");
                    var currentRows = (await _purchaseOrderDBContext.QueryDataTableRaw(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
                    data.Rows = currentRows.Select(r => r.ToNonCamelCaseDictionary(f => f.Key, f => f.Value)).ToArray();
                }
                await ValidateSaleVoucherConfig(null, data?.Info);

                // Get all fields
                var voucherFields = _purchaseOrderDBContext.VoucherField
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                var result = await ProcessActionAsync(voucherTypeId, voucherTypeInfo.BeforeSaveActionExec, data, voucherFields, EnumActionType.Delete, billInfo.FId);

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
                await ProcessActionAsync(voucherTypeId, voucherTypeInfo.AfterSaveActionExec, data, voucherFields, EnumActionType.Delete);

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

        private async Task FillGenerateColumn(long? fId, List<IGenerateCodeContext> generateCodeCtxs, Dictionary<string, ValidateVoucherField> fields, IList<NonCamelCaseDictionary> rows)
        {
            Dictionary<string, int> baseValueChains = new Dictionary<string, int>();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                foreach (var infoField in fields)
                {
                    var field = infoField.Value;

                    if ((EnumFormType)field.FormTypeId == EnumFormType.Generate &&
                        (!row.TryGetStringValue(field.FieldName, out var value) || value.IsNullOrEmptyObject())
                    )
                    {
                        var code = rows.FirstOrDefault(r => r.ContainsKey(PurchaseOrderConstants.BILL_CODE))?[PurchaseOrderConstants.BILL_CODE]?.ToString();

                        var ngayCt = rows.FirstOrDefault(r => r.ContainsKey(PurchaseOrderConstants.BILL_DATE))?[PurchaseOrderConstants.BILL_DATE]?.ToString();

                        var currentCode = rows.FirstOrDefault(r => r.ContainsKey(field.FieldName) && !string.IsNullOrWhiteSpace(r[field.FieldName]?.ToString()))?.ToString();
                        long? ngayCtValue = null;
                        if (long.TryParse(ngayCt, out var v))
                        {
                            ngayCtValue = v;
                        }
                        value = (value ?? "").Trim();
                        var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
                        value = await ctx.SetConfig(EnumObjectType.VoucherTypeRow, EnumObjectType.VoucherAreaField, field.VoucherAreaFieldId, null)
                            .SetConfigData(fId ?? 0, ngayCtValue)
                            .TryValidateAndGenerateCode(currentCode,
                            async (code) =>
                            {
                                var sqlCommand = $"SELECT {field.FieldName} FROM {VOUCHERVALUEROW_TABLE}" +
                                $" WHERE {field.FieldName} = @Code " +
                                $"AND VoucherBill_F_Id <> @FId " +
                                $"AND isDeleted = 0";
                                var dataRow = await _purchaseOrderDBContext.QueryDataTableRaw(sqlCommand, new[]
                                {
                                    new SqlParameter("@Code", code),
                                    new SqlParameter("@FId", fId)
                                });

                                return dataRow.Rows.Count > 0;
                            });
                        generateCodeCtxs.Add(ctx);
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
        private async Task ConfirmIGenerateCodeContext(List<IGenerateCodeContext> lstCtx)
        {
            foreach (var ctx in lstCtx)
            {
                await ctx.ConfirmCode();
            }
        }

        private async Task CreateBillVersion(int voucherTypeId, VoucherBill billInfo, BillInfoModel data, List<IGenerateCodeContext> generateCodeCtxs)
        {
            var fields = (await GetVoucherFields(voucherTypeId)).ToDictionary(f => f.FieldName, f => f);

            var infoFields = fields.Where(f => !f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(billInfo.FId, generateCodeCtxs, infoFields, new[] { data.Info });


            if (data.Info.TryGetStringValue(PurchaseOrderConstants.BILL_CODE, out var sct))
            {
                Utils.ValidateCodeSpecialCharactors(sct);
                sct = sct?.ToUpper();
                data.Info[PurchaseOrderConstants.BILL_CODE] = sct;
                billInfo.BillCode = sct;
            }

            var rowFields = fields.Where(f => f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(billInfo.FId, generateCodeCtxs, rowFields, data.Rows);

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

                    if (item.Key.IsVndColumn() && !value.IsNullOrEmptyObject())
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
            if (data.Info.Any(k => k.Key.IsVndColumn() && decimal.TryParse(k.Value?.ToString(), out var value)))
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

        public async Task<List<ValidateVoucherField>> GetVoucherFields(int voucherTypeId, int? areaId = null, bool isExport = false)
        {
            var area = _purchaseOrderDBContext.VoucherArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.VoucherAreaId == areaId);

            }


            var fields = await (from af in _purchaseOrderDBContext.VoucherAreaField
                                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                                join a in area on af.VoucherAreaId equals a.VoucherAreaId
                                where af.VoucherTypeId == voucherTypeId
                                orderby a.SortOrder, af.SortOrder
                                select new ValidateVoucherField
                                {
                                    VoucherAreaFieldId = af.VoucherAreaFieldId,
                                    Title = af.Title,
                                    IsAutoIncrement = af.IsAutoIncrement,
                                    IsRequire = af.IsRequire,
                                    IsUnique = af.IsUnique,
                                    FiltersName = af.FiltersName,
                                    Filters = af.Filters,
                                    FieldName = f.FieldName,
                                    DataTypeId = f.DataTypeId,
                                    FormTypeId = f.FormTypeId,
                                    RefTableCode = f.RefTableCode,
                                    RefTableField = f.RefTableField,
                                    RefTableTitle = f.RefTableTitle,
                                    RegularExpression = af.RegularExpression,
                                    IsMultiRow = a.IsMultiRow,
                                    RequireFiltersName = af.RequireFiltersName,
                                    RequireFilters = af.RequireFilters,
                                    IsReadOnly = f.IsReadOnly,
                                    IsHidden = af.IsHidden,
                                    AreaTitle = a.Title
                                }).ToListAsync();

            if (isExport)
            {
                var refFieldNames = fields.Where(f => !string.IsNullOrWhiteSpace(f.RefTableCode))
                     .SelectMany(f => f.RefTableTitle.Split(',').Select(r => $"{f.FieldName}_{r.Trim()}"));
                fields = fields.Where(f => (f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect) || refFieldNames.Contains(f.FieldName))
                    .ToList();
            }
            else
            {
                fields = fields.Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                    .ToList();

            }

            return fields;
        }


        public async Task<CategoryNameModel> GetFieldDataForMapping(int voucherTypeId, int? areaId, bool? isExport = null)
        {
            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking().FirstOrDefaultAsync(t => t.VoucherTypeId == voucherTypeId);


            // Lấy thông tin field
            var fields = await GetVoucherFields(voucherTypeId, areaId, isExport == true);

            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.VoucherTypeId,
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
                    //CategoryFieldId = field.VoucherAreaFieldId,
                    FieldName = field.FieldName,
                    FieldTitle = GetTitleCategoryField(field),
                    RefCategory = null,
                    IsRequired = field.IsRequire,
                    GroupName = field.AreaTitle
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
            var viewName = await GetVoucherViewName(voucherTypeId);

            var voucherType = await GetVoucherTypExecInfo(voucherTypeId);

            var reader = new ExcelReader(stream);


            // Lấy thông tin field
            var fields = await GetVoucherFields(voucherTypeId);

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var firstRow = data.Rows.FirstOrDefault();
            if (firstRow != null)
            {
                var notContainColumn = mapping.MappingFields.FirstOrDefault(m => !firstRow.ContainsKey(m.Column));
                if (notContainColumn != null)
                {
                    throw GeneralCode.InvalidParams.BadRequest($"Không tồn tại cột {notContainColumn.Column} trong Sheet {mapping.SheetName}");
                }
            }

            using (var longTask = await longTaskResourceLockService.Accquire($"Nhập chứng từ bán hàng \"{voucherType.Title}\" từ excel"))
            {
                longTask.SetTotalRows(data.Rows.Count());
                longTask.SetCurrentStep("Kiểm tra dữ liệu");

                var requiredField = fields.FirstOrDefault(f => f.IsRequire && string.IsNullOrWhiteSpace(f.RequireFilters) && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

                if (requiredField != null) throw FieldRequired.BadRequestFormat(requiredField.Title);

                var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
                var referTableNames = fields.Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();

                var referFields = await _httpCategoryHelperService.GetReferFields(referTableNames, referMapingFields.Select(f => f.RefFieldName).ToList());


                var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == PurchaseOrderConstants.BILL_CODE);
                if (columnKey == null)
                {
                    throw BillCodeError.BadRequest();
                }

                var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();

                var groups = data.Rows.Select((r, i) => new ImportExcelRowModel
                {
                    Data = r,
                    Index = i + mapping.FromRow
                })
                    .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                    .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                    .GroupBy(r => r.Data[columnKey.Column])
                    .ToDictionary(r => r.Key, r => r.ToList());
                List<BillInfoModel> bills = new List<BillInfoModel>();

                // Validate unique single field
                foreach (var field in fields.Where(f => f.IsUnique))
                {
                    var mappingField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                    if (mappingField == null) continue;

                    var values = field.IsMultiRow ? groups.SelectMany(b => b.Value.Select(r => r.Data[mappingField.Column]?.ToString())).ToList() : groups.Where(b => b.Value.Count() > 0).Select(b => b.Value.First().Data[mappingField.Column]?.ToString()).ToList();

                    // Check unique trong danh sách values thêm mới
                    if (values.Distinct().Count() < values.Count)
                    {
                        throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(", ", values.Distinct().Take(5)) });
                    }
                    // Checkin unique trong db
                    var existSql = $"SELECT F_Id,{field.FieldName} FROM {viewName} WHERE VoucherTypeId = {voucherTypeId} ";
                    existSql += $" AND {field.FieldName} IN (SELECT NValue FROM @Values)";
                    var existKeyParams = new List<SqlParameter>() { values.ToSqlParameter("@Values") };
                    var result = await _purchaseOrderDBContext.QueryDataTableRaw(existSql, existKeyParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(VoucherErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(", ", result.AsEnumerable().Select(r => r[field.FieldName]?.ToString()).ToList().Distinct().Take(5)) });
                    }
                }

                foreach (var bill in groups)
                {
                    var billInfo = await GetBillFromRows(bill, mapping, fields, referFields, false);

                    await ValidateSaleVoucherConfig(billInfo?.Info, null);

                    bills.Add(billInfo);
                }

                longTask.SetCurrentStep("Cập nhật vào cơ sở dữ liệu");

                using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var lstCtx = new List<IGenerateCodeContext>();

                        // Get all fields
                        var voucherFields = _purchaseOrderDBContext.VoucherField
                         .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
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
                            await ProcessActionAsync(voucherTypeId, voucherType.BeforeSaveActionExec, bill, voucherFields, EnumActionType.Add);

                            bill.Info.TryGetStringValue(PurchaseOrderConstants.BILL_CODE, out var billCode);
                            if (string.IsNullOrWhiteSpace(billCode))
                            {
                                bill.GetExcelRowNumbers().TryGetValue(bill.Rows[0], out var rNumber);
                                throw new BadRequestException($@"Mã chứng từ dòng {rNumber} không được để trống!");
                            }

                            var billInfo = new VoucherBill()
                            {
                                VoucherTypeId = voucherTypeId,
                                LatestBillVersion = 1,
                                SubsidiaryId = _currentContextService.SubsidiaryId,
                                BillCode = billCode?.ToUpper(),
                                IsDeleted = false
                            };

                            await _purchaseOrderDBContext.VoucherBill.AddAsync(billInfo);

                            await _purchaseOrderDBContext.SaveChangesAsync();

                            await CreateBillVersion(voucherTypeId, billInfo, bill, lstCtx);

                            // After saving action (SQL)
                            await ProcessActionAsync(voucherTypeId, voucherType.AfterSaveActionExec, bill, voucherFields, EnumActionType.Add);

                            longTask.IncProcessedRows();

                        }

                        trans.Commit();
                        await ConfirmIGenerateCodeContext(lstCtx);
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
        }

        public async Task<BillInfoModel> ParseBillFromMapping(int voucherTypeId, BillParseMapping parseMapping, Stream stream)
        {
            var mapping = parseMapping.Mapping;
            var bill = parseMapping.Bill;

            var inputTypeInfo = await GetVoucherTypExecInfo(voucherTypeId);

            var reader = new ExcelReader(stream);
            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var firstRow = data.Rows.FirstOrDefault();
            if (firstRow != null)
            {
                var notContainColumn = mapping.MappingFields.FirstOrDefault(m => !firstRow.ContainsKey(m.Column));
                if (notContainColumn != null)
                {
                    throw GeneralCode.InvalidParams.BadRequest($"Không tồn tại cột {notContainColumn.Column} trong Sheet {mapping.SheetName}");
                }
            }

            // Lấy thông tin field
            var fields = await GetVoucherFields(voucherTypeId);
            foreach (var f in fields)
            {
                f.IsRequire = false;
                f.RequireFilters = null;
            };

            //var infoFields = fields.Where(f => f.AreaId != areaId).ToList();


            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
            var referTableNames = fields.Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();
            var referFields = await _httpCategoryHelperService.GetReferFields(referTableNames, null);


            var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();

            var insertRows = data.Rows.Select((r, i) => new ImportExcelRowModel
            {
                Data = r,
                Index = i + mapping.FromRow
            })
            .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
            .ToList();

            foreach (var row in insertRows)
            {
                foreach (var infoData in bill.Info)
                {
                    var field = fields.FirstOrDefault(f => f.FieldName == infoData.Key);
                    if (field != null && !row.Data.ContainsKey(infoData.Key))
                    {
                        var valueData = ((EnumDataType)field.DataTypeId).GetSqlValue(infoData.Value);
                        row.Data.Add(infoData.Key, valueData?.ToString());
                    }
                }
            }


            var billExcel = new KeyValuePair<string, List<ImportExcelRowModel>>("", insertRows);

            var billInfo = await GetBillFromRows(billExcel, mapping, fields, referFields, true);

            foreach (var row in billInfo.Rows)
            {
                foreach (var fieldName in row.Keys.ToArray())
                {
                    var field = fields.FirstOrDefault(f => f.FieldName == fieldName);
                    if (field != null)
                    {
                        row[fieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(row[fieldName]);
                    }

                }
            }
            return billInfo;

        }


        /// <summary>
        /// Convert excel data to string, datetime => unix => string = "1667985449"
        /// </summary>
        /// <param name="bill"></param>
        /// <param name="mapping"></param>
        /// <param name="fields"></param>
        /// <param name="referFields"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException"></exception>
        private async Task<BillInfoModel> GetBillFromRows(KeyValuePair<string, List<ImportExcelRowModel>> bill, ImportExcelMapping mapping, List<ValidateVoucherField> fields, List<ReferFieldModel> referFields, bool isGetRefObj)
        {
            var info = new NonCamelCaseDictionary();
            var rows = new List<NonCamelCaseDictionary>();
            int count = bill.Value.Count();
            for (int rowIndx = 0; rowIndx < count; rowIndx++)
            {
                var mapRow = new NonCamelCaseDictionary();
                var row = bill.Value.ElementAt(rowIndx);
                foreach (var mappingField in mapping.MappingFields)
                {
                    var field = fields.FirstOrDefault(f => f.FieldName == mappingField.FieldName);

                    // Validate mapping required
                    if (field == null && mappingField.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty) throw new BadRequestException(GeneralCode.ItemNotFound, $"Trường dữ liệu {mappingField.FieldName} không tìm thấy");
                    if (field == null) continue;
                    //if (!field.IsMultiRow && rowIndx > 0) continue;

                    object value = null;
                    var titleValues = new Dictionary<string, object>();

                    if (row.Data.ContainsKey(mappingField.Column))
                        value = row.Data[mappingField.Column];

                    var strValue = value?.ToString()?.Trim();
                    var originValue = value;

                    // Validate require
                    if (string.IsNullOrWhiteSpace(strValue) && field.IsRequire && string.IsNullOrWhiteSpace(field.RequireFilters)) throw new BadRequestException(VoucherErrorCode.RequiredFieldIsEmpty, new object[] { row.Index, field.Title });

                    if (string.IsNullOrWhiteSpace(strValue)) continue;


                    if (strValue.StartsWith(PREFIX_ERROR_CELL))
                    {
                        throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(row.Index, mappingField.Column, $"\"{field.Title}\" {originValue}");
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)field.DataTypeId))
                    {
                        if (!DateTime.TryParse(strValue, out DateTime date))
                            throw CannotConvertValueInRowFieldToDateTime.BadRequestFormat(originValue?.JsonSerialize(), row.Index, field.Title);
                        value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix();
                        strValue = value?.ToString();
                    }

                    // Validate refer
                    if (!((EnumFormType)field.FormTypeId).IsSelectForm())
                    {
                        // Validate value
                        if (!field.IsAutoIncrement)
                        {
                            string regex = ((EnumDataType)field.DataTypeId).GetRegex();
                            if ((field.DataSize > 0 && strValue.Length > field.DataSize)
                                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(strValue, regex))
                                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(strValue, field.RegularExpression)))
                            {
                                throw new BadRequestException(VoucherErrorCode.VoucherValueInValid, new object[] { originValue?.JsonSerialize(), row.Index, field.Title });
                            }
                        }
                    }
                    else
                    {
                        int suffix = 0;
                        var paramName = $"@{mappingField.RefFieldName}_{suffix}";
                        var titleRefConfigs = field.RefTableTitle.Split(',')?.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                        titleValues = referFields.Where(f => f.CategoryCode == field.RefTableCode
                        && (isGetRefObj || titleRefConfigs.Contains(f.CategoryFieldName))
                        )
                            .ToList()
                            .ToDictionary(f => f.CategoryFieldName, f => (object)null);

                        var titleFieldSelect = string.Join(", ", titleValues.Keys.ToArray());
                        if (!string.IsNullOrWhiteSpace(titleFieldSelect))
                        {
                            titleFieldSelect = ", " + titleFieldSelect;
                        }

                        var referField = referFields.FirstOrDefault(f => f.CategoryCode == field.RefTableCode && f.CategoryFieldName == mappingField.RefFieldName);
                        if (referField == null)
                        {
                            throw RefFieldNotExisted.BadRequestFormat(field.Title, mappingField.FieldName);
                        }
                        var referSql = $"SELECT TOP 1 {field.RefTableField} {titleFieldSelect} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
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
                                mapRow.TryGetStringValue(fieldName, out string filterValue);
                                if (string.IsNullOrEmpty(filterValue))
                                {
                                    info.TryGetStringValue(fieldName, out filterValue);
                                }
                                if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                                {
                                    filterValue = filterValue.Substring(start, length);
                                }
                                if (string.IsNullOrEmpty(filterValue))
                                {
                                    var fieldBefore = (fields.FirstOrDefault(f => f.FieldName == fieldName)?.Title) ?? fieldName;
                                    throw RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                                }
                                filters = filters.Replace(match[i].Value, filterValue);
                            }

                            Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                            if (filterClause != null)
                            {
                                var whereCondition = new StringBuilder();


                                try
                                {
                                    var parameters = mapRow?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);
                                    foreach (var (key, val) in info.Where(d => !d.Value.IsNullOrEmptyObject() && !parameters.ContainsKey(d.Key)))
                                    {
                                        parameters.Add(key, val);
                                    }

                                    suffix = filterClause.FilterClauseProcess($"v{field.RefTableCode}", $"v{field.RefTableCode}", whereCondition, referParams, suffix, refValues: parameters);

                                }
                                catch (EvalObjectArgException agrEx)
                                {
                                    var fieldBefore = (fields.FirstOrDefault(f => f.FieldName == agrEx.ParamName)?.Title) ?? agrEx.ParamName;
                                    throw RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                                if (whereCondition.Length > 0) referSql += $" AND {whereCondition}";
                            }
                        }

                        var referData = await _purchaseOrderDBContext.QueryDataTableRaw(referSql, referParams.ToArray());
                        if (referData == null || referData.Rows.Count == 0)
                        {
                            // Check tồn tại
                            var checkExistedReferSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                            var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                            referData = await _purchaseOrderDBContext.QueryDataTableRaw(checkExistedReferSql, checkExistedReferParams.ToArray());
                            if (referData == null || referData.Rows.Count == 0)
                            {
                                throw new BadRequestException(VoucherErrorCode.ReferValueNotFound, new object[] { row.Index, field.Title + ": " + originValue });
                            }
                            else
                            {
                                throw new BadRequestException(VoucherErrorCode.ReferValueNotValidFilter, new object[] { row.Index, field.Title + ": " + originValue, field.FiltersName });
                            }
                        }
                        var refRow = referData.Rows[0];
                        value = refRow[field.RefTableField];
                        strValue = value?.ToString();

                        foreach (var titleFieldName in titleValues.Keys.ToArray())
                        {
                            titleValues[titleFieldName] = refRow[titleFieldName];
                        }
                    }
                    if (!field.IsMultiRow)
                    {
                        if (info.ContainsKey(field.FieldName))
                        {
                            if (info[field.FieldName]?.ToString() != strValue)
                            {
                                throw MultipleDiffValueAtInfoArea.BadRequestFormat(originValue, row.Index, field.Title, bill.Key);
                            }
                        }
                        else
                        {
                            info.Add(field.FieldName, value);
                            foreach (var titleField in titleValues)
                            {
                                info.Add(field.FieldName + "_" + titleField.Key, titleField.Value);
                            }
                        }
                    }
                    else
                    {
                        mapRow.Add(field.FieldName, value);
                        foreach (var titleField in titleValues)
                        {
                            mapRow.Add(field.FieldName + "_" + titleField.Key, titleField.Value);
                        }
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

            return billInfo;
        }


        public async Task<(MemoryStream Stream, string FileName)> ExportVoucherBill(int voucherTypeId, long fId)
        {
            var viewName = await GetVoucherViewName(voucherTypeId);

            var dataSql = @$"
                SELECT     r.*
                FROM {viewName} r 
                WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, Array.Empty<SqlParameter>());
            var billEntryInfoSql = $"SELECT r.* FROM {viewName} r WHERE r.VoucherBill_F_Id = {fId} AND r.VoucherTypeId = {voucherTypeId} AND r.IsBillEntry = 1";
            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTableRaw(billEntryInfoSql, Array.Empty<SqlParameter>());

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
                    info.TryGetStringValue(uniqField, out billCode);
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

            MemoryStream stream = writer.WriteToStream();
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
                        var result = await _purchaseOrderDBContext.QueryDataTableRaw(existSql, new[] { new SqlParameter("@value", value) });
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

            await ValidateSaleVoucherConfig(billDate as DateTime?, oldDate as DateTime?);
        }

        public async Task ValidateSaleVoucherConfig(DateTime? billDate, DateTime? oldDate)
        {
            await ValidateDateOfBill(billDate, oldDate);
        }

        private string GlobalFilter()
        {
            return $"r.SubsidiaryId = {_currentContextService.SubsidiaryId}";
        }

        public async Task<BillInfoModel> GetPackingListInfo(int packingListVoucherTypeId, long voucherBill_BHXKId)
        {          

            var viewName = await GetVoucherViewName(packingListVoucherTypeId);

            var singleFields = (await (
               from af in _purchaseOrderDBContext.VoucherAreaField
               join a in _purchaseOrderDBContext.VoucherArea on af.VoucherAreaId equals a.VoucherAreaId
               join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
               where af.VoucherTypeId == packingListVoucherTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
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
                FROM {viewName} r 

                WHERE r.so_bh_xk = {voucherBill_BHXKId} AND r.VoucherTypeId = {packingListVoucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = await _purchaseOrderDBContext.QueryDataTableRaw(dataSql, Array.Empty<SqlParameter>());
            result.Info = null;
            var billEntryInfoSql = $"SELECT r.* FROM {viewName} r WHERE r.so_bh_xk = {voucherBill_BHXKId} AND r.VoucherTypeId = {packingListVoucherTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _purchaseOrderDBContext.QueryDataTableRaw(billEntryInfoSql, Array.Empty<SqlParameter>());
            var billEntryInfos = billEntryInfo.ConvertData();
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var _billEntryInfo = billEntryInfos.Where(d => (long)d["VoucherBill_F_Id"] == (long)row["VoucherBill_F_Id"]).FirstOrDefault();
                if (_billEntryInfo != null && billEntryInfos.Count > 0)
                {
                    for (var j = 0; j < data.Columns.Count; j++)
                    {
                        var column = data.Columns[j];
                        if (singleFields.Contains(column.ColumnName))
                        {
                            row[column] = _billEntryInfo[column.ColumnName];
                        }
                    }
                }
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
            if (orderCodes == null || orderCodes.Count == 0) return new List<VoucherOrderDetailSimpleModel>();
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

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int voucherTypeId)
        {
            var sql = $"SELECT DISTINCT v.VoucherTypeId ObjectTypeId, v.VoucherBill_F_Id ObjectBill_F_Id, v.so_ct ObjectBillCode FROM {VOUCHERVALUEROW_TABLE} v WHERE (v.CensorStatusId IS NULL OR  v.CensorStatusId <> {(int)EnumCensorStatus.Approved}) AND v.VoucherTypeId = @VoucherTypeId AND v.IsDeleted = 0";

            return (await _purchaseOrderDBContext.QueryDataTableRaw(sql, new[] { new SqlParameter("@VoucherTypeId", voucherTypeId) }))
                    .ConvertData<ObjectBillSimpleInfoModel>()
                    .ToList();
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int voucherTypeId)
        {
            var sql = $"SELECT DISTINCT v.VoucherTypeId ObjectTypeId, v.VoucherBill_F_Id ObjectBill_F_Id, v.so_ct ObjectBillCode FROM {VOUCHERVALUEROW_TABLE} v WHERE (v.CheckStatusId IS NULL OR  v.CheckStatusId <> {(int)EnumCheckStatus.CheckedSuccess}) AND v.VoucherTypeId = @VoucherTypeId AND v.IsDeleted = 0";

            return (await _purchaseOrderDBContext.QueryDataTableRaw(sql, new[] { new SqlParameter("@VoucherTypeId", voucherTypeId) }))
                    .ConvertData<ObjectBillSimpleInfoModel>()
                    .ToList();
        }

        public async Task<bool> CheckAllBillInList(IList<ObjectBillSimpleInfoModel> models)
        {
            if (models.Count > 0)
            {
                var sql = $"UPDATE {VOUCHERVALUEROW_TABLE} SET CheckStatusId = {(int)EnumCheckStatus.CheckedSuccess} WHERE VoucherBill_F_Id IN (";
                var sqlParams = new List<SqlParameter>();
                var prefixColumn = "@VoucherBill_F_Id_";
                foreach (var item in models.Select((item, index) => new { item, index }))
                {
                    if (item.index > 0)
                        sql += ", ";
                    sql += prefixColumn + $"{item.index}";
                    sqlParams.Add(new SqlParameter(prefixColumn + $"{item.index}", item.item.ObjectBill_F_Id));
                }
                sql += ")";

                await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync(sql, sqlParams);
            }
            return true;
        }

        public async Task<bool> ApproveAllBillInList(IList<ObjectBillSimpleInfoModel> models)
        {
            if (models.Count > 0)
            {
                var sql = $"UPDATE {VOUCHERVALUEROW_TABLE} SET CensorStatusId = {(int)EnumCensorStatus.Approved} WHERE VoucherBill_F_Id IN (";
                var sqlParams = new List<SqlParameter>();
                var prefixColumn = "@VoucherBill_F_Id_";
                foreach (var item in models.Select((item, index) => new { item, index }))
                {
                    if (item.index > 0)
                        sql += ", ";
                    sql += prefixColumn + $"{item.index}";
                    sqlParams.Add(new SqlParameter(prefixColumn + $"{item.index}", item.item.ObjectBill_F_Id));
                }
                sql += ")";

                await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync(sql, sqlParams);
            }
            return true;
        }

        private async Task<string> GetVoucherViewName(int voucherTypeId)
        {
            var typeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking().FirstOrDefaultAsync(t => t.VoucherTypeId == voucherTypeId);
            if (typeInfo == null) throw GeneralCode.InvalidParams.BadRequest();
            return PurchaseOrderConstants.VoucherTypeView(typeInfo.VoucherTypeCode);
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

        private class ImportExcelRowModel
        {
            public NonCamelCaseDictionary<string> Data { get; set; }
            public int Index { get; set; }
        }
        public class ValidateVoucherField
        {
            public int VoucherAreaFieldId { get; set; }
            public string Title { get; set; }
            public bool IsAutoIncrement { get; set; }
            public bool IsRequire { get; set; }
            public bool IsUnique { get; set; }
            public string FiltersName { get; set; }
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
            public string RequireFiltersName { get; set; }
            public string RequireFilters { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsHidden { get; set; }
            public string AreaTitle { get; set; }
        }
    }
}
