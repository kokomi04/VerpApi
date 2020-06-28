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

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = new InputBill()
                {
                    InputTypeId = inputTypeId,
                    LatestBillVersion = 1,
                    IsDeleted = false
                };
                await _accountancyDBContext.InputBill.AddAsync(billInfo);

                await _accountancyDBContext.SaveChangesAsync();

                await CreateBillVersion(inputTypeId, billInfo.FId, 1, data);

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

        public async Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _accountancyDBContext.InputBill.FirstOrDefaultAsync(b => b.FId == inputValueBillId);

                if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ");


                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);

                await CreateBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion + 1, data);

                billInfo.LatestBillVersion++;

                await _accountancyDBContext.SaveChangesAsync();

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


                await DeleteBillVersion(inputTypeId, billInfo.FId, billInfo.LatestBillVersion);


                billInfo.IsDeleted = true;
                billInfo.DeletedDatetimeUtc = DateTime.UtcNow;
                billInfo.UpdatedByUserId = _currentContextService.UserId;

                await _accountancyDBContext.SaveChangesAsync();

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
    }
}
