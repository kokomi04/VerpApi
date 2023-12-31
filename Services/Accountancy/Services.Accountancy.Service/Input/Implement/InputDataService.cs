﻿using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using Verp.Cache.RedisCache;
using Verp.Resources.Accountancy.InputData;
using Verp.Resources.GlobalObject;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using static Verp.Resources.Accountancy.InputData.InputDataValidationMessage;
using static VErp.Commons.Library.EvalUtils;
using static VErp.Commons.Library.ExcelReader;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputDataPrivateService : InputDataServiceBase, IInputDataPrivateService
    {
        private static InputDataObjectType objectTypes = new InputDataObjectType
        {
            InputType = EnumObjectType.InputType,
            InputBill = EnumObjectType.InputBill,
            InputRow = EnumObjectType.InputTypeRow,
            InputRowArea = EnumObjectType.InputAreaField
        };

        public InputDataPrivateService(AccountancyDBPrivateContext accountancyDBContext, IInputDataDependService inputDataDependService, IInputPrivateConfigService inputConfigService)
           : base(accountancyDBContext, inputDataDependService, inputConfigService, objectTypes, InputValueRowViewName.Private)
        {

        }

    }

    public class InputDataPublicService : InputDataServiceBase, IInputDataPublicService
    {
        private static InputDataObjectType objectTypes = new InputDataObjectType
        {
            InputType = EnumObjectType.InputTypePublic,
            InputBill = EnumObjectType.InputBillPublic,
            InputRow = EnumObjectType.InputTypeRowPublic,
            InputRowArea = EnumObjectType.InputAreaFieldPublic
        };

        public InputDataPublicService(AccountancyDBPublicContext accountancyDBContext, IInputDataDependService inputDataDependService, IInputPublicConfigService inputConfigService)
            : base(accountancyDBContext, inputDataDependService, inputConfigService, objectTypes, InputValueRowViewName.Public)
        {

        }

    }



    public interface IInputDataDependService
    {
        ILogger Logger { get; }
        IActivityLogService ActivityLogService { get; }
        IMapper Mapper { get; }
        ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
        ICurrentContextService CurrentContextService { get; }
        ICategoryHelperService HttpCategoryHelperService { get; }
        IOutsideMappingHelperService OutsideMappingHelperService { get; }
        ObjectActivityLogFacade InputDataActivityLog { get; }
        ICachingService CachingService { get; }
        ILongTaskResourceLockService LongTaskResourceLockService { get; }
    }
    public class InputDataDependService : IInputDataDependService
    {
        public ILogger Logger { get; }
        public IActivityLogService ActivityLogService { get; }
        public IMapper Mapper { get; }
        public ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
        public ICurrentContextService CurrentContextService { get; }
        public ICategoryHelperService HttpCategoryHelperService { get; }
        public IOutsideMappingHelperService OutsideMappingHelperService { get; }
        public ObjectActivityLogFacade InputDataActivityLog { get; }
        public ICachingService CachingService { get; }
        public ILongTaskResourceLockService LongTaskResourceLockService { get; }

        public InputDataDependService(
            ILogger<InputDataDependService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , ICategoryHelperService httpCategoryHelperService
            , IOutsideMappingHelperService outsideMappingHelperService
            , ICachingService cachingService
            , ILongTaskResourceLockService longTaskResourceLockService)
        {
            Logger = logger;
            ActivityLogService = activityLogService;
            Mapper = mapper;
            CustomGenCodeHelperService = customGenCodeHelperService;
            CurrentContextService = currentContextService;
            HttpCategoryHelperService = httpCategoryHelperService;
            OutsideMappingHelperService = outsideMappingHelperService;
            CachingService = cachingService;
            LongTaskResourceLockService = longTaskResourceLockService;
        }
    }

    public abstract class InputDataServiceBase : BillDateValidateionServiceAbstract, IInputDataServiceBase
    {
        private const string INPUTVALUEROW_TABLE = AccountantConstants.INPUTVALUEROW_TABLE;
        private readonly InputValueRowViewName _inputValueRowView;

        private readonly ILogger _logger;
        //private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly IInputConfigServiceBase _inputConfigService;
        private readonly ObjectActivityLogFacade _inputDataActivityLog;
        private readonly ICachingService _cachingService;
        private readonly ILongTaskResourceLockService longTaskResourceLockService;

        private readonly EnumObjectType _inputTypeObjectType;
        private readonly EnumObjectType _inputBillObjectType;
        private readonly EnumObjectType _inputRowObjectType;
        private readonly EnumObjectType _inputRowAreaObjectType;


        private readonly List<string> _specialViewOnlyColumns = new List<string>();

        private readonly IQueryable<InputField> _inputFieldsSet;

        protected internal InputDataServiceBase(AccountancyDBContext accountancyDBContext,
                IInputDataDependService inputDataDependService,
                IInputConfigServiceBase inputConfigService,
                InputDataObjectType objectTypes,
                InputValueRowViewName valueRowView
            ) : base(accountancyDBContext)
        {
            _inputTypeObjectType = objectTypes.InputType;
            _inputBillObjectType = objectTypes.InputBill;
            _inputRowObjectType = objectTypes.InputRow;
            _inputRowAreaObjectType = objectTypes.InputRowArea;

            _accountancyDBContext = accountancyDBContext;
            _logger = inputDataDependService.Logger;
            //_activityLogService = activityLogService;
            _mapper = inputDataDependService.Mapper;
            _customGenCodeHelperService = inputDataDependService.CustomGenCodeHelperService;
            _currentContextService = inputDataDependService.CurrentContextService;
            _httpCategoryHelperService = inputDataDependService.HttpCategoryHelperService;
            _outsideMappingHelperService = inputDataDependService.OutsideMappingHelperService;
            _inputConfigService = inputConfigService;
            _inputDataActivityLog = inputDataDependService.ActivityLogService.CreateObjectTypeActivityLog(_inputBillObjectType);
            _cachingService = inputDataDependService.CachingService;
            longTaskResourceLockService = inputDataDependService.LongTaskResourceLockService;
            _inputValueRowView = valueRowView;


            _inputFieldsSet = _accountancyDBContext.InputField.AsQueryable();
            if (_inputValueRowView == InputValueRowViewName.Public)
            {
                _inputFieldsSet = _inputFieldsSet.Where(f => !AccountantConstants.IsPublicDataExtraColumns.Contains(f.FieldName));
            }
            else
            {
                _specialViewOnlyColumns.AddRange(AccountantConstants.IsPublicDataExtraColumns);
            }

        }

        protected internal class InputDataObjectType
        {
            public EnumObjectType InputType { get; set; }
            public EnumObjectType InputBill { get; set; }
            public EnumObjectType InputRow { get; set; }
            public EnumObjectType InputRowArea { get; set; }
        }

        protected internal sealed class InputValueRowViewName
        {
            private string _value;
            private InputValueRowViewName(string value)
            {
                _value = value;
            }
            public override string ToString()
            {
                return _value.ToString();
            }

            public static implicit operator string(InputValueRowViewName view)
            {
                return view.ToString();
            }

            public static InputValueRowViewName Private = new InputValueRowViewName(AccountantConstants.INPUTVALUEROWPRIVATE_VIEW);
            public static InputValueRowViewName Public = new InputValueRowViewName(AccountantConstants.INPUTVALUEROW_VIEW);
        }


        public async Task<PageDataTable> GetBills(int inputTypeId, bool isMultirow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size, BaseWorkingDateModel workingDate = null)
        {
            keyword = (keyword ?? "").Trim();

            var viewInfo = await _accountancyDBContext.InputTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var inputTypeViewId = viewInfo?.InputTypeViewId;


            var fields = (await (
                from af in _accountancyDBContext.InputAreaField
                join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                join f in _inputFieldsSet on af.InputFieldId equals f.InputFieldId
                // && f.FormTypeId != (int)EnumFormType.SqlSelect
                where af.InputTypeId == inputTypeId && ((f.FormTypeId != (int)EnumFormType.ViewOnly) || _specialViewOnlyColumns.Contains(f.FieldName))
                select new { a.InputAreaId, af.InputAreaFieldId, f.FieldName, f.RefTableCode, f.RefTableField, f.RefTableTitle, f.FormTypeId, f.DataTypeId, a.IsMultiRow, a.IsAddition, af.IsCalcSum }
           ).ToListAsync()
           ).ToDictionary(f => f.FieldName, f => f);

            var viewFields = await (
                from f in _accountancyDBContext.InputTypeViewField
                where f.InputTypeViewId == inputTypeViewId
                select f
            ).ToListAsync();

            var sqlParams = new List<SqlParameter>() {
                new SqlParameter("@InputTypeId",inputTypeId)
            };

            var whereCondition = new StringBuilder();

            whereCondition.Append($"r.InputTypeId = @InputTypeId AND {GlobalFilter()}");

            if (fromDate.HasValue && toDate.HasValue)
            {
                whereCondition.Append($" AND r.{AccountantConstants.BILL_DATE} BETWEEN @FromDate AND @ToDate");

                sqlParams.Add(new SqlParameter("@FromDate", EnumDataType.Date.GetSqlValue(fromDate.Value)));
                sqlParams.Add(new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate.Value)));
            }
            if (workingDate != null && (!workingDate.IsIgnoreFilterAccountant.HasValue || !workingDate.IsIgnoreFilterAccountant.Value)
                && workingDate.WorkingFromDate != null && workingDate.WorkingToDate != null)
            {
                whereCondition.Append($" AND r.{AccountantConstants.BILL_DATE} BETWEEN @WorkingFromDate AND @WorkingToDate");
                sqlParams.Add(new SqlParameter("@WorkingFromDate", EnumDataType.Date.GetSqlValue(workingDate.WorkingFromDate.Value)));
                sqlParams.Add(new SqlParameter("@WorkingToDate", EnumDataType.Date.GetSqlValue(workingDate.WorkingToDate.Value)));
           
            }

            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.InputTypeViewFieldId == filter.Key);
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

                            suffix = filterClause.FilterClauseProcess(_inputValueRowView, "r", whereCondition, sqlParams, suffix, false, value);
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

                suffix = columnsFilters.FilterClauseProcess(_inputValueRowView, "r", whereCondition, sqlParams, suffix);
            }




            var fieldToSelect = fields.Values.Where(f => f.IsMultiRow == isMultirow || isMultirow).ToList();

            var sumCols = fieldToSelect.Where(c => c.IsCalcSum).ToList();

            var sumSql = string.Join(", ", sumCols.Select(c => $"SUM(r.{c.FieldName}) AS {c.FieldName}").ToArray());
            if (!string.IsNullOrWhiteSpace(sumSql))
            {
                sumSql = ", " + sumSql;
            }

            string totalSql;
            if (isMultirow)
            {
                totalSql = @$"SELECT COUNT(0) as Total {sumSql} FROM {_inputValueRowView} r WHERE {whereCondition}";
            }
            else
            {

                totalSql = @$"
                    SELECT COUNT(0) Total {sumSql} FROM (
                        SELECT r.InputBill_F_Id {sumSql} FROM {_inputValueRowView} r WHERE {whereCondition}
                        GROUP BY r.InputBill_F_Id
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


            var table = await _accountancyDBContext.QueryDataTableRaw(totalSql, sqlParams.ToArray());

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
            if (isMultirow)
            {
                dataSql = @$"
                 
                    SELECT r.InputBill_F_Id, r.F_Id BillDetailId {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                    FROM {_inputValueRowView} r
                    WHERE {whereCondition}
               
                    ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }
            else
            {
                dataSql = @$"
                 ;WITH tmp AS (
                    SELECT r.InputBill_F_Id, MAX(F_Id) as F_Id
                    FROM {_inputValueRowView} r
                    WHERE {whereCondition}
                    GROUP BY r.InputBill_F_Id    
                )
                SELECT 
                    t.InputBill_F_Id AS F_Id,
                    t.InputBill_F_Id
                    {(string.IsNullOrWhiteSpace(selectColumn) ? "" : $",{selectColumn}")}
                FROM tmp t JOIN {_inputValueRowView} r ON t.F_Id = r.F_Id
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";
            }

            if (size >= 0)
            {
                dataSql += @$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size}
                ROWS ONLY";
            }

            var data = await _accountancyDBContext.QueryDataTableRaw(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return (data, total, additionResults);
        }

        public async Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size)
        {
            var singleFields = (await (
               from af in _accountancyDBContext.InputAreaField
               join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
               join f in _inputFieldsSet on af.InputFieldId equals f.InputFieldId
               where af.InputTypeId == inputTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
               select f
            ).ToListAsync()
            )
            .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
             f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
             new[] { f.FieldName }
            )
            .ToHashSet();

            var totalSql = @$"SELECT COUNT(0) as Total FROM {_inputValueRowView} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0";

            var table = await _accountancyDBContext.QueryDataTableRaw(totalSql, new SqlParameter[0]);

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
                FROM {_inputValueRowView} r 

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
            var data = await _accountancyDBContext.QueryDataTableRaw(dataSql, Array.Empty<SqlParameter>());

            var billEntryInfoSql = $"SELECT r.* FROM {_inputValueRowView} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1";

            var billEntryInfo = await _accountancyDBContext.QueryDataTableRaw(billEntryInfoSql, Array.Empty<SqlParameter>());

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

        public async Task<IList<NonCamelCaseDictionary>> CalcResultAllowcation(int parentInputTypeId, long parentFId)
        {
            var type = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == parentInputTypeId);
            if (type == null) throw GeneralCode.ItemNotFound.BadRequest();

            var data = await _accountancyDBContext.QueryDataTableRaw(type.CalcResultAllowcationSqlQuery, new[]
            {
                new SqlParameter("@ParentInputTypeId", parentInputTypeId),
                new SqlParameter("@ParentFId", parentFId),
            });

            return data.ConvertData();
        }

        public async Task<InputType> GetTypeInfo(int inputTypeId)
        {
            return await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);
        }


        public async Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId)
        {
            return (await GetBillInfos(inputTypeId, new[] { fId })).First().Value;
        }

        public async Task<BillInfoModel> GetBillInfoByParent(int inputTypeId, long parentFId)
        {
            var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.ParentInputBillFId == parentFId);
            if (billInfo == null) return new BillInfoModel();
            return await GetBillInfo(inputTypeId, billInfo.FId);
        }

        public async Task<IList<string>> GetAllocationDataBillCodes(int inputTypeId, long fId)
        {
            return (await _accountancyDBContext.InputBillAllocation.Where(a => a.ParentInputBillFId == fId).Select(a => a.DataAllowcationBillCode).ToListAsync());
        }

        public async Task<bool> UpdateAllocationDataBillCodes(int inputTypeId, long fId, IList<string> dataAllowcationBillCodes)
        {
            var lst = await _accountancyDBContext.InputBillAllocation.Where(a => a.ParentInputBillFId == fId).ToListAsync();
            _accountancyDBContext.InputBillAllocation.RemoveRange(lst);
            await _accountancyDBContext.SaveChangesAsync();
            await _accountancyDBContext.InputBillAllocation.AddRangeAsync(
                dataAllowcationBillCodes.Select(c => c?.ToUpper()).Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .Select(c => new InputBillAllocation()
                {
                    ParentInputBillFId = fId,
                    DataAllowcationBillCode = c
                }));
            await _accountancyDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<IDictionary<long, BillInfoModel>> GetBillInfos(int inputTypeId, IList<long> fIds)
        {
            var singleFields = await _cachingService.TryGetSet("GetBillInfo", "GetBillInfo_" + inputTypeId, TimeSpan.FromMinutes(3), async () =>
            {
                return (await (
                               from af in _accountancyDBContext.InputAreaField
                               join a in _accountancyDBContext.InputArea on af.InputAreaId equals a.InputAreaId
                               join f in _inputFieldsSet on af.InputFieldId equals f.InputFieldId
                               where af.InputTypeId == inputTypeId && !a.IsMultiRow && ((f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect) || _specialViewOnlyColumns.Contains(f.FieldName))
                               select f
                            ).ToListAsync()
                )
                .SelectMany(f => !string.IsNullOrWhiteSpace(f.RefTableCode) && ((EnumFormType)f.FormTypeId).IsJoinForm() ?
                 f.RefTableTitle.Split(',').Select(t => $"{f.FieldName}_{t.Trim()}").Union(new[] { f.FieldName }) :
                 new[] { f.FieldName }
                )
                .ToHashSet();
            }, TimeSpan.FromMinutes(1));


            var dataSql = @$"

                SELECT     r.*
                FROM {_inputValueRowView} r 
                    JOIN @FIds v ON r.InputBill_F_Id = v.[Value]
                WHERE  r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 0
            ";
            var data = (await _accountancyDBContext.QueryDataTableRaw(dataSql, new[] { fIds.ToSqlParameter("@FIds") })).ConvertData();

            var billEntryInfoSql = @$"

                SELECT     r.*
                FROM {_inputValueRowView} r 
                    JOIN @FIds v ON r.InputBill_F_Id = v.[Value]
                WHERE  r.InputTypeId = {inputTypeId} AND {GlobalFilter()} AND r.IsBillEntry = 1
            ";

            var billEntryInfos = (await _accountancyDBContext.QueryDataTableRaw(billEntryInfoSql, new[] { fIds.ToSqlParameter("@FIds") })).ConvertData();
            var lst = new Dictionary<long, BillInfoModel>();

            var billInfos = await _accountancyDBContext.InputBill.Where(b => fIds.Contains(b.FId)).ToListAsync();
            foreach (var fId in fIds)
            {
                var result = new BillInfoModel()
                {
                    ParentId = billInfos?.FirstOrDefault(b => b.FId == fId)?.ParentInputBillFId
                };

                var rows = data.Where(r => (long)r["InputBill_F_Id"] == fId).ToList();

                var billEntryInfo = billEntryInfos.FirstOrDefault(b => (long)b["InputBill_F_Id"] == fId);
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


        public async Task<long> CreateBill(int inputTypeId, BillInfoModel data, bool isDeleteAllowcationBill)
        {
            await CheckAndDeleteAllocationBill(inputTypeId, 0, isDeleteAllowcationBill, data.ParentId);

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
                var inputFields = _inputFieldsSet
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
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
                    BillCode = Guid.NewGuid().ToString(),
                    IsDeleted = false,
                    ParentInputBillFId = data.ParentId
                };
                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                if (data.ParentId > 0)
                {
                    var parentInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(p => p.FId == data.ParentId);
                    if (parentInfo == null) throw GeneralCode.ItemNotFound.BadRequest("Parent was not found!");
                    parentInfo.HasChildren = true;
                }

                await _accountancyDBContext.SaveChangesAsync();

                var listGenerateCodeCtx = new List<IGenerateCodeContext>();

                await CreateBillVersion(inputTypeId, billInfo, data, listGenerateCodeCtx);

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Add);

                //if (!string.IsNullOrWhiteSpace(data?.OutsideImportMappingData?.MappingFunctionKey))
                //{
                //    await _outsideMappingHelperService.MappingObjectCreate(data.OutsideImportMappingData.MappingFunctionKey, data.OutsideImportMappingData.ObjectId, _inputTypeObjectType, billInfo.FId);
                //}


                trans.Commit();
                await ConfirmIGenerateCodeContext(listGenerateCodeCtx);

                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Create)
                .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                .BillTypeId(inputTypeId)
                .ObjectId(billInfo.FId)
                .JsonData(data)
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
                                var result = await _accountancyDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray(), cachingService: _cachingService);
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
                                var result = await _accountancyDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray(), cachingService: _cachingService);
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
                    new SqlParameter("@InputTypeObjectTypeId", _inputTypeObjectType),
                    resultParam,
                    messageParam,
                    new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.InputTableType" }
                };

                resultData = (await _accountancyDBContext.QueryDataTableRaw(script, parammeters)).ConvertData();
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

                values = values.Distinct().ToList();

                if (values.Count > 0)
                {
                    Dictionary<object, object> mapTitles = new Dictionary<object, object>(new DataEqualityComparer((EnumDataType)field.DataTypeId));
                    var sqlParams = new List<SqlParameter>();
                    var sql = new StringBuilder($"SELECT DISTINCT {field.RefTableField}, {field.RefTableTitle} FROM v{field.RefTableCode} WHERE {field.RefTableField} IN ");

                    switch ((EnumDataType)field.DataTypeId)
                    {
                        case EnumDataType.Int:
                        case EnumDataType.BigInt:
                            sql.Append("(SELECT Value FROM @Values)");
                            sqlParams.Add(values.Select(v => Convert.ToInt64(v)).ToList().ToSqlParameter("@Values"));
                            break;

                        case EnumDataType.Text:
                            sql.Append("(SELECT NValue FROM @Values)");
                            sqlParams.Add(values.Select(v => v?.ToString()).ToList().ToSqlParameter("@Values"));
                            break;
                        default:

                            var suffix = 0;
                            sql.Append("(");
                            foreach (var value in values)
                            {
                                var paramName = $"@{field.RefTableField}_{suffix}";
                                if (suffix > 0) sql.Append(",");
                                sql.Append(paramName);
                                sqlParams.Add(new SqlParameter(paramName, value) { SqlDbType = ((EnumDataType)field.DataTypeId).GetSqlDataType() });
                                suffix++;
                            }

                            sql.Append(")");

                            break;
                    }

                    var data = await _accountancyDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray(), cachingService: _cachingService);
                    for (int indx = 0; indx < data.Rows.Count; indx++)
                    {
                        var titleField = field.RefTableTitle?.Split(',')[0]?.Trim();
                        if (string.IsNullOrWhiteSpace(titleField))
                        {
                            titleField = field.RefTableField;
                        }
                        mapTitles.Add(data.Rows[indx][field.RefTableField], data.Rows[indx][titleField]);
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

                    info.Data.TryGetStringValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(InputErrorCode.RequireValueNotValidFilter,
                            new object[] { SingleRowArea, field.Title, field.RequireFiltersName });
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

                        row.Data.TryGetStringValue(field.FieldName, out string value);
                        if (string.IsNullOrEmpty(value))
                        {
                            throw new BadRequestException(InputErrorCode.RequireValueNotValidFilter,
                                new object[] { row.ExcelRow ?? rowIndx, field.Title, field.RequireFiltersName });
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
            var result = await _accountancyDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
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
                    try
                    {
                        var parameters = checkData.Data?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);

                        foreach (var (key, val) in info.Data.Where(d => !d.Value.IsNullOrEmptyObject() && !parameters.ContainsKey(d.Key)))
                        {
                            parameters.Add(key, val);
                        }

                        suffix = filterClause.FilterClauseProcess(tableName, tableName, whereCondition, sqlParams, suffix, refValues: parameters);
                    }
                    catch (Exception ex)
                    {
                        ArgumentException agrEx = null;

                        if (ex is ArgumentException except)
                        {
                            agrEx = except;
                        }

                        if (ex.InnerException is ArgumentException innerEx)
                        {
                            agrEx = innerEx;
                        }

                        if (agrEx != null)
                        {
                            var fieldBefore = (allFields.FirstOrDefault(f => f.FieldName == agrEx.ParamName)?.Title) ?? agrEx.ParamName;
                            throw RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                        }
                        throw;
                    }
                }
            }

            if (whereCondition.Length > 0)
            {
                existSql += $" AND {whereCondition}";
            }

            var result = await _accountancyDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, value) };
                result = await _accountancyDBContext.QueryDataTableRaw(checkExistedReferSql, checkExistedReferParams.ToArray(), cachingService: _cachingService);
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(InputErrorCode.ReferValueNotValidFilter,
                        new object[] { rowIndex.HasValue ? rowIndex.ToString() : SingleRowArea, field.Title + ": " + value, field.FiltersName });
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

        public async Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data, bool isDeleteAllowcationBill)
        {
            await CheckAndDeleteAllocationBill(inputTypeId, inputValueBillId, isDeleteAllowcationBill, data.ParentId);

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
            var infoSQL = new StringBuilder("SELECT TOP 1 UpdatedDatetimeUtc,  ");
            var singleFields = inputAreaFields.Where(f => !f.IsMultiRow).ToList();
            AppendSelectFields(ref infoSQL, singleFields);
            infoSQL.Append($" FROM vInputValueRow r WHERE InputTypeId={inputTypeId} AND InputBill_F_Id = {inputValueBillId} AND {GlobalFilter()}");
            var currentInfo = (await _accountancyDBContext.QueryDataTableRaw(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData().FirstOrDefault();

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

            await ValidateAccountantConfig(data?.Info, currentInfo);

            NonCamelCaseDictionary futureInfo = data.Info;
            ValidateRowModel checkInfo = new ValidateRowModel(data.Info, CompareRow(currentInfo, futureInfo, singleFields), null);

            // Get changed rows
            List<ValidateRowModel> checkRows = new List<ValidateRowModel>();
            var rowsSQL = new StringBuilder("SELECT F_Id,");
            var multiFields = inputAreaFields.Where(f => f.IsMultiRow).ToList();
            AppendSelectFields(ref rowsSQL, multiFields);
            rowsSQL.Append($" FROM vInputValueRow r WHERE InputBill_F_Id = {inputValueBillId} AND {GlobalFilter()}");
            var currentRows = (await _accountancyDBContext.QueryDataTableRaw(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
            foreach (var futureRow in data.Rows)
            {
                futureRow.TryGetStringValue("F_Id", out string futureValue);
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
                var inputFields = _inputFieldsSet
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
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


                var lstCtx = new List<IGenerateCodeContext>();

                billInfo.LatestBillVersion++;

                await CreateBillVersion(inputTypeId, billInfo, data, lstCtx);



                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Update, inputValueBillId);

                trans.Commit();

                await ConfirmIGenerateCodeContext(lstCtx);

                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Update)
                  .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                  .BillTypeId(inputTypeId)
                  .ObjectId(billInfo.FId)
                  .JsonData(data)
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

        public async Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] billIds, long[] detailIds, bool isDeleteAllowcationBill)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            var billInfos = await _accountancyDBContext.InputBill.Where(b => billIds.Contains(b.FId)).ToListAsync();
            foreach (var billId in billIds)
            {
                await CheckAndDeleteAllocationBill(inputTypeId, billId, isDeleteAllowcationBill, billInfos.FirstOrDefault(b => b.FId == billId)?.ParentInputBillFId);
            }

            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            if (billIds.Length == 0) throw ListBillsToUpdateIsEmpty.BadRequest();

            // Get field
            var fields = await _accountancyDBContext.InputAreaField.Include(f => f.InputField).Include(f => f.InputArea).Where(f => f.InputArea.InputTypeId == inputTypeId).ToListAsync();
            var field = fields.FirstOrDefault(f => f.InputField.FieldName == fieldName);
            if (field == null) throw FieldNotFound.BadRequest();

            if (!string.IsNullOrWhiteSpace(field.Filters))
            {
                throw new BadRequestException($@"Không thể cập nhật đồng loạt trường dữ liệu có ràng buộc {field.Title} {field.FiltersName}");
            }

            var dependField = fields.FirstOrDefault(f => f.Filters?.Contains(field.InputField.FieldName) == true);

            if (dependField != null)
            {
                throw new BadRequestException($@"Không thể cập nhật đồng loạt trường dữ liệu có ràng buộc {field.Title} bởi {dependField.FiltersName} {dependField.Title}");
            }

            if (!field.InputArea.IsMultiRow && detailIds?.Length > 0)
            {
                var checkUpdateMultipleFielsSql = $@"
                    ;WITH db AS(
                        SELECT r.InputBill_F_Id, COUNT(0) TotalDetail 
                            FROM {INPUTVALUEROW_TABLE} r 
                            WHERE r.InputTypeId = {inputTypeId} AND r.IsDeleted = 0 
                                AND r.InputBill_F_Id IN(SELECT [Value] FROM @BillIds) 
                            GROUP BY r.InputBill_F_Id 
                    ),req AS (
                        SELECT r.InputBill_F_Id, COUNT(0) TotalDetail 
                            FROM {INPUTVALUEROW_TABLE} r 
                            WHERE r.InputTypeId = {inputTypeId} AND r.IsDeleted = 0 
                                AND r.InputBill_F_Id IN (SELECT [Value] FROM @BillIds) 
                                AND r.F_Id  IN (SELECT [Value] FROM @DetailIds) 
                            GROUP BY r.InputBill_F_Id 
                    )
                    SELECT r.{AccountantConstants.BILL_CODE} 
                        FROM db 
                            LEFT JOIN req ON db.InputBill_F_Id = req.InputBill_F_Id
                            LEFT JOIN {INPUTVALUEROW_TABLE} r ON db.InputBill_F_Id = r.InputBill_F_Id
                    WHERE req.InputBill_F_Id IS NULL OR req.TotalDetail < db.TotalDetail
                    ";
                var invalids = await _accountancyDBContext.QueryDataTableRaw(checkUpdateMultipleFielsSql, new[] { billIds.ToSqlParameter("@BillIds"), detailIds.ToSqlParameter("@DetailIds") });
                if (invalids.Rows.Count > 0)
                {
                    var billCode = invalids.Rows[0][AccountantConstants.BILL_CODE];
                    throw new BadRequestException($@"Trường dữ liệu ở vùng chung. Bạn cần lựa chọn tất cả các dòng chi tiết của chứng từ có mã {billCode}");
                }
            }

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
                    var oldResult = await _accountancyDBContext.QueryDataTableRaw(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldValue)) });
                    if (oldResult == null || oldResult.Rows.Count == 0) throw OldValueIsInvalid.BadRequest();
                    oldSqlValue = ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(oldResult.Rows[0][0]);
                }
                else
                {
                    oldSqlValue = DBNull.Value;
                }

                if (newValue != null)
                {
                    var newResult = await _accountancyDBContext.QueryDataTableRaw(selectSQL, new SqlParameter[] { new SqlParameter("@ValueParam", ((EnumDataType)refTitleField.DataTypeId).GetSqlValue(newValue)) });
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
                join f in _inputFieldsSet on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId && !a.IsMultiRow && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                select f.FieldName).ToListAsync()).ToHashSet();

            // Get bills by old value
            var sqlParams = new List<SqlParameter>()
            {
                billIds.ToSqlParameter("@BillIds")
            };

            var dataSql = new StringBuilder(@$"

                SELECT     r.*
                FROM {INPUTVALUEROW_TABLE} r 

                WHERE r.InputTypeId = {inputTypeId} AND r.IsDeleted = 0 AND r.InputBill_F_Id IN (SELECT [Value] FROM @BillIds) AND {GlobalFilter()}");

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
            //    sqlParams.Add(new SqlParameter(paramName, oldSqlValue));//
            //}

            var data = await _accountancyDBContext.QueryDataTableRaw(dataSql.ToString(), sqlParams.ToArray());
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

                    if (column.ColumnName.Equals(AccountantConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) && !v.IsNullOrEmptyObject())
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
                var newDate = fieldName.Equals(AccountantConstants.BILL_DATE, StringComparison.OrdinalIgnoreCase) ? (newSqlValue as DateTime?) : null;

                await ValidateAccountantConfig(newDate ?? oldBillDate.Value, oldBillDate.Value);
            }

            var bills = _accountancyDBContext.InputBill.Where(b => updateBillIds.Contains(b.FId) && b.SubsidiaryId == _currentContextService.SubsidiaryId).ToList();
            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Created bill version
                await _accountancyDBContext.InsertDataTable(dataTable, true);

                using (var batchLog = _inputDataActivityLog.BeginBatchLog())
                {
                    foreach (var bill in bills)
                    {
                        // Delete bill version
                        await DeleteBillVersion(inputTypeId, bill.FId, bill.LatestBillVersion);


                        await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.UpdateMulti)
                            .MessageResourceFormatDatas(inputTypeInfo.Title, field?.Title + " (" + field?.Title + ")", bill.BillCode)
                            .BillTypeId(inputTypeId)
                            .ObjectId(bill.FId)
                            .JsonData(new { inputTypeId, fieldName, oldValue, newValue, billIds })
                            .CreateLog();

                        // Update last bill version
                        bill.LatestBillVersion++;
                    }

                    await _accountancyDBContext.SaveChangesAsync();
                    trans.Commit();

                    await batchLog.CommitAsync();
                }


                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateMultipleBills");
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
            info.SetGlobalSetting(global);
            return info;
        }

        public async Task<bool> DeleteBill(int inputTypeId, long inputBill_F_Id, bool isDeleteAllowcationBill)
        {
            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));


            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputBill_F_Id && b.SubsidiaryId == _currentContextService.SubsidiaryId);

                if (billInfo == null) throw BillNotFound.BadRequest();

                await CheckAndDeleteAllocationBill(inputTypeId, inputBill_F_Id, isDeleteAllowcationBill, billInfo.ParentInputBillFId);

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
                var infoLst = (await _accountancyDBContext.QueryDataTableRaw(infoSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();

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
                    var currentRows = (await _accountancyDBContext.QueryDataTableRaw(rowsSQL.ToString(), Array.Empty<SqlParameter>())).ConvertData();
                    data.Rows = currentRows.Select(r => r.ToNonCamelCaseDictionary(f => f.Key, f => f.Value)).ToArray();
                }
                await ValidateAccountantConfig(null, data?.Info);

                // Get all fields
                var inputFields = _inputFieldsSet
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                 .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);

                // Before saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.BeforeSaveActionExec, data, inputFields, EnumActionType.Delete, inputBill_F_Id);

                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);

                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;


                if (billInfo.ParentInputBillFId > 0)
                {
                    var parentInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(p => p.FId == billInfo.ParentInputBillFId);
                    if (parentInfo != null)
                    {
                        parentInfo.HasChildren = await _accountancyDBContext.InputBill.AnyAsync(b => b.FId != billInfo.FId && b.ParentInputBillFId == billInfo.ParentInputBillFId);
                    }
                }

                await _accountancyDBContext.SaveChangesAsync();

                // After saving action (SQL)
                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, data, inputFields, EnumActionType.Delete, inputBill_F_Id);

                await _outsideMappingHelperService.MappingObjectDelete(_inputBillObjectType, billInfo.FId);

                trans.Commit();

                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.Delete)
                           .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                           .BillTypeId(inputTypeId)
                           .ObjectId(billInfo.FId)
                           .JsonData(data)
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

        private async Task FillGenerateColumn(long? fId, List<IGenerateCodeContext> generateCodeCtxs, Dictionary<string, ValidateField> fields, IList<NonCamelCaseDictionary> rows)
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

                        var code = rows.FirstOrDefault(r => r.ContainsKey(AccountantConstants.BILL_CODE))?[AccountantConstants.BILL_CODE]?.ToString();

                        var ngayCt = rows.FirstOrDefault(r => r.ContainsKey(AccountantConstants.BILL_DATE))?[AccountantConstants.BILL_DATE]?.ToString();
                        var currentCode = rows.FirstOrDefault(r => r.ContainsKey(field.FieldName) && !string.IsNullOrWhiteSpace(r[field.FieldName]?.ToString()))?.ToString();
                        long? ngayCtValue = null;
                        if (long.TryParse(ngayCt, out var v))
                        {
                            ngayCtValue = v;
                        }

                        var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
                        value = await ctx.SetConfig(_inputRowObjectType, _inputRowAreaObjectType, field.InputAreaFieldId, null)
                           .SetConfigData(fId ?? 0, ngayCtValue)
                           .TryValidateAndGenerateCode(currentCode,
                           async (code) =>
                           {
                               var sqlCommand = $"SELECT {field.FieldName} FROM {INPUTVALUEROW_TABLE}" +
                               $" WHERE {field.FieldName} = @Code " +
                               $"AND InputBill_F_Id <> @FId " +
                               $"AND isDeleted = 0";
                               var dataRow = await _accountancyDBContext.QueryDataTableRaw(sqlCommand, new[]
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
        private async Task CreateBillVersion(int inputTypeId, InputBill billInfo, BillInfoModel data, List<IGenerateCodeContext> generateCodeCtxs)
        {

            var fields = (await GetInputFields(inputTypeId)).ToDictionary(f => f.FieldName, f => f);


            var infoFields = fields.Where(f => !f.Value.IsMultiRow).ToDictionary(f => f.Key, f => f.Value);

            await FillGenerateColumn(billInfo.FId, generateCodeCtxs, infoFields, new[] { data.Info });

            if (data.Info.TryGetStringValue(AccountantConstants.BILL_CODE, out var sct))
            {
                Utils.ValidateCodeSpecialCharactors(sct);
                sct = sct?.ToUpper();
                data.Info[AccountantConstants.BILL_CODE] = sct;
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


            var requireFields = fields.Values.Where(f => f.IsRequire && string.IsNullOrWhiteSpace(f.RequireFilters)).Select(f => f.FieldName).Distinct().ToHashSet();

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



            //Create addition reciprocal accounting
            if (data.Info.Any(k => k.Key.IsVndColumn() && !k.Value.IsNullOrEmptyObject()))// decimal.TryParse(k.Value?.ToString(), out var value) && value != 0
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

        public async Task<List<ValidateField>> GetInputFields(int inputTypeId, int? areaId = null, bool isExport = false)
        {
            var area = _accountancyDBContext.InputArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.InputAreaId == areaId);
            }


            var fields = await (from af in _accountancyDBContext.InputAreaField
                                join f in _inputFieldsSet on af.InputFieldId equals f.InputFieldId
                                join a in area on af.InputAreaId equals a.InputAreaId
                                where af.InputTypeId == inputTypeId
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
                                    AreaTitle = a.Title,
                                    AreaId = a.InputAreaId

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


        public async Task<CategoryNameModel> GetFieldDataForMapping(int inputTypeId, int? areaId, bool? isExport)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);


            // Lấy thông tin field
            var fields = await GetInputFields(inputTypeId, areaId, isExport == true);

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



        public async Task<bool> ImportBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream, bool isDeleteAllowcationBill)
        {
            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

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

            using (var longTask = await longTaskResourceLockService.Accquire($"Nhập chứng từ \"{inputTypeInfo.Title}\" từ excel"))
            {
                longTask.SetTotalRows(data.Rows.Count());

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
                    var existKeySql = $"SELECT DISTINCT InputBill_F_Id, {columnKey.FieldName} FROM vInputValueRow WHERE InputTypeId = {inputTypeId} AND {columnKey.FieldName} IN (SELECT NValue FROM @Values)";

                    var existKeyParams = new List<SqlParameter>() { keys.ToSqlParameter("@Values") };

                    var existKeyResult = await _accountancyDBContext.QueryDataTableRaw(existKeySql, existKeyParams.ToArray());

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
                            throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { errField.Title, string.Join(", ", existKeys.Select(c => c.Key).Distinct().Take(5).ToArray()), "" });
                        case EnumImportDuplicateOption.IgnoreBill:
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
                        throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(",", values.Distinct().Take(5)), "" });
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
                    var result = await _accountancyDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
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
                        throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title, string.Join(", ", dupValues.Take(5).ToArray()), inputType_Title });
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
                        var billInfo = await GetBillFromRows(bill, mapping, fields, referFields, false, null);
                        createBills.Add(billInfo);
                    }
                }

                foreach (var bill in updateGroups)
                {
                    var billInfo = await GetBillFromRows(bill, mapping, fields, referFields, false, null);
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
                var inputFields = _inputFieldsSet
                 .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
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
                    using (var logBatch = _inputDataActivityLog.BeginBatchLog())
                    {
                        try
                        {
                            var lstCtx = new List<IGenerateCodeContext>();


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

                                bill.Info.TryGetStringValue(AccountantConstants.BILL_CODE, out var billCode);
                                if (string.IsNullOrWhiteSpace(billCode))
                                {
                                    bill.GetExcelRowNumbers().TryGetValue(bill.Rows[0], out var rNumber);
                                    throw new BadRequestException($@"Mã chứng từ dòng {rNumber} không được để trống!");
                                }

                                var billInfo = new InputBill()
                                {
                                    InputTypeId = inputTypeId,
                                    LatestBillVersion = 1,
                                    SubsidiaryId = _currentContextService.SubsidiaryId,
                                    BillCode = billCode?.ToUpper(),
                                    IsDeleted = false
                                };

                                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                                await _accountancyDBContext.SaveChangesAsync();

                                await CreateBillVersion(inputTypeId, billInfo, bill, lstCtx);

                                // After saving action (SQL)
                                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, bill, inputFields, EnumActionType.Add);

                                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.CreateViaExcel)
                                  .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                                  .BillTypeId(inputTypeId)
                                  .ObjectId(billInfo.FId)
                                  .JsonData(bill)
                                  .CreateLog();

                                longTask.IncProcessedRows();

                            }

                            var infos = await GetBillInfos(inputTypeId, updateBills.Keys.ToList());

                            // Cập nhật chứng từ
                            foreach (var bill in updateBills)
                            {

                                var oldBillInfo = infos[bill.Key];

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
                                    futureRow.TryGetStringValue("F_Id", out string futureValue);
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

                                await CheckAndDeleteAllocationBill(inputTypeId, billInfo.FId, isDeleteAllowcationBill, billInfo.ParentInputBillFId);


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

                                await CreateBillVersion(inputTypeId, billInfo, newBillInfo, lstCtx);

                                await _accountancyDBContext.SaveChangesAsync();

                                // After saving action (SQL)
                                await ProcessActionAsync(inputTypeId, inputTypeInfo.AfterSaveActionExec, newBillInfo, inputFields, EnumActionType.Update, billInfo.FId);

                                await _inputDataActivityLog.LogBuilder(() => AccountancyBillActivityLogMessage.UpdateViaExcel)
                                   .MessageResourceFormatDatas(inputTypeInfo.Title, billInfo.BillCode)
                                   .BillTypeId(inputTypeId)
                                   .ObjectId(billInfo.FId)
                                   .JsonData(newBillInfo)
                                   .CreateLog();

                                longTask.IncProcessedRows();

                            }


                            trans.Commit();

                            await ConfirmIGenerateCodeContext(lstCtx);

                            await logBatch.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            trans.TryRollbackTransaction();
                            _logger.LogError(ex, "Import");
                            throw;
                        }
                    }
                }
                return true;
            }
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
        private async Task<BillInfoModel> GetBillFromRows(KeyValuePair<string, List<ImportExcelRowModel>> bill, ImportExcelMapping mapping, List<ValidateField> fields, List<ReferFieldModel> referFields, bool isGetRefObj, NonCamelCaseDictionary refValues)
        {
            if (refValues == null)
            {
                refValues = new NonCamelCaseDictionary();
            }

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
                    //if (!field.IsMultiRow && rowIndx > 0 && info.ContainsKey(field.FieldName)) continue;

                    object value = null;
                    var titleValues = new Dictionary<string, object>();
                    if (row.Data.ContainsKey(mappingField.Column))
                        value = row.Data[mappingField.Column];

                    var strValue = value?.ToString()?.Trim();
                    var originValue = value;

                    if (string.IsNullOrWhiteSpace(strValue)) continue;


                    if (strValue.StartsWith(PREFIX_ERROR_CELL))
                    {
                        throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(row.Index, mappingField.Column, $"\"{field.Title}\" {originValue}");
                    }

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)field.DataTypeId))
                    {
                        if (!DateTime.TryParse(strValue, out DateTime date))
                            throw CannotConvertValueInRowFieldToDateTime.BadRequestFormat(originValue?.JsonSerialize(), row.Index, field.Title);
                        value = date.Date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix();
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
                                throw new BadRequestException(InputErrorCode.InputValueInValid, new object[] { originValue?.JsonSerialize(), row.Index, field.Title });
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

                                try
                                {
                                    var parameters = mapRow?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);
                                    foreach (var (key, val) in info.Where(d => !d.Value.IsNullOrEmptyObject() && !parameters.ContainsKey(d.Key)))
                                    {
                                        parameters.Add(key, val);
                                    }

                                    foreach (var (key, val) in refValues.Where(d => !d.Value.IsNullOrEmptyObject() && !parameters.ContainsKey(d.Key)))
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

                        var referData = await _accountancyDBContext.QueryDataTableRaw(referSql, referParams.ToArray(), cachingService: _cachingService);
                        if (referData == null || referData.Rows.Count == 0)
                        {
                            // Check tồn tại
                            var checkExistedReferSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                            var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                            referData = await _accountancyDBContext.QueryDataTableRaw(checkExistedReferSql, checkExistedReferParams.ToArray(), cachingService: _cachingService);
                            if (referData == null || referData.Rows.Count == 0)
                            {
                                throw new BadRequestException(InputErrorCode.ReferValueNotFound, new object[] { row.Index, field.Title + ": " + originValue });
                            }
                            else
                            {
                                throw new BadRequestException(InputErrorCode.ReferValueNotValidFilter,
                                    new object[] { row.Index, field.Title + ": " + originValue, field.FiltersName });
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

                    if (field != null)
                    {
                        // Validate require
                        if (string.IsNullOrWhiteSpace(value) && field.IsRequire && string.IsNullOrWhiteSpace(field.RequireFilters))
                            throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new object[] { rowIndexs[billInfo.Rows[i]], field.Title });
                    }
                }
            }
            // await ValidateAccountantConfig(billInfo?.Info, null);

            return billInfo;
        }


        public async Task<BillInfoModel> ParseBillFromMapping(int inputTypeId, BillParseMapping parseMapping, Stream stream)
        {
            var mapping = parseMapping.Mapping;
            var bill = parseMapping.Bill;

            var inputTypeInfo = await GetInputTypExecInfo(inputTypeId);

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
            var fields = await GetInputFields(inputTypeId);
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

            var billInfo = await GetBillFromRows(billExcel, mapping, fields, referFields, true, bill.Info);

            billInfo.Info = bill.Info;

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

        public async Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId)
        {

            var dataSql = @$"
                SELECT     r.*
                FROM {_inputValueRowView} r 
                WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 0
            ";
            var data = await _accountancyDBContext.QueryDataTableRaw(dataSql, Array.Empty<SqlParameter>());
            var billEntryInfoSql = $"SELECT r.* FROM {_inputValueRowView} r WHERE r.InputBill_F_Id = {fId} AND r.InputTypeId = {inputTypeId} AND r.IsBillEntry = 1";
            var billEntryInfo = await _accountancyDBContext.QueryDataTableRaw(billEntryInfoSql, Array.Empty<SqlParameter>());

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
                                    join itf in _inputFieldsSet on iaf.InputFieldId equals itf.InputFieldId
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
                    info.TryGetStringValue(uniqField, out billCode);
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

            //var fileName = $"{inputType.InputTypeCode}_{billCode}.xlsx";
            var fileName = StringUtils.RemoveDiacritics($"{billCode}#{inputType.Title}.xlsx").Replace(" ", "#");

            MemoryStream stream = writer.WriteToStream();
            return (stream, fileName);
        }

        public async Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow)
        {
            var inputReferToFields = _inputFieldsSet
                .Where(f => f.RefTableCode == categoryCode && fieldNames.Contains(f.RefTableField)).ToList();

            if (categoryRow == null)
            {
                // Check khi xóa cả danh mục
                return _inputFieldsSet.Any(f => f.RefTableCode == categoryCode);
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
                        var existSql = $"SELECT tk.F_Id FROM {_inputValueRowView} tk WHERE tk.{referToField.FieldName} = @RefValue;";
                        var result = await _accountancyDBContext.QueryDataTableRaw(existSql, new[] { referToValue });
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

            return (await _accountancyDBContext.QueryDataTableRaw(sql, new[] { new SqlParameter("@InputTypeId", inputTypeId) }))
                    .ConvertData<ObjectBillSimpleInfoModel>()
                    .ToList();
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            var sql = $"SELECT DISTINCT v.InputTypeId ObjectTypeId, v.InputBill_F_Id ObjectBill_F_Id, v.so_ct ObjectBillCode FROM {INPUTVALUEROW_TABLE} v WHERE (v.CheckStatusId IS NULL OR  v.CheckStatusId <> {(int)EnumCheckStatus.CheckedSuccess}) AND v.InputTypeId = @InputTypeId AND v.IsDeleted = 0";

            return (await _accountancyDBContext.QueryDataTableRaw(sql, new[] { new SqlParameter("@InputTypeId", inputTypeId) }))
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
            var billDate = ExtractBillDate(info) as DateTime?;
            var oldDate = ExtractBillDate(oldInfo) as DateTime?;

            await ValidateAccountantConfig(billDate, oldDate);
        }

        public async Task ValidateAccountantConfig(DateTime? billDate, DateTime? oldDate)
        {
            await ValidateDateOfBill(billDate, oldDate);
        }

        private string GlobalFilter()
        {
            return $"r.SubsidiaryId = {_currentContextService.SubsidiaryId}";
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

        private async Task CheckAndDeleteAllocationBill(int inputTypeId, long fId, bool isDelete, long? parentId)
        {
            var lst = await _accountancyDBContext.QueryListProc<ObjectBillInUsedInfo>("asp_InputBill_CheckAndDeleteAllocationBillV2", new[]
            {
                new SqlParameter("@BillTypeId",inputTypeId),
                new SqlParameter("@BillId",fId),
                new SqlParameter("@ObjectTypeId",(int)_inputBillObjectType),
                new SqlParameter("@IsDelete",isDelete),
                new SqlParameter("@ParentId",parentId),
            });
            if (!isDelete && lst.Count > 0)
            {
                throw GeneralCode.ItemInUsed.BadRequestFormatWithData(lst, $"Tồn tại chứng từ phân bổ {lst.First().BillCode}");
            }
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

            public string AreaTitle { get; set; }
            public int AreaId { get; set; }
        }
    }
}
