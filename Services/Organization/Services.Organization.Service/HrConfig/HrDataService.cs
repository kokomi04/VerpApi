using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services.Organization.Model.HrConfig;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrDataService
    {
        Task<long> CreateHr(int hrTypeId, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data);
        Task<bool> DeleteHr(int hrTypeId, long hrBill_F_Id);
        Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetHr(int hrTypeId, long hrBill_F_Id);
        Task<PageDataTable> SearchHr(int hrTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);
        Task<bool> UpdateHr(int hrTypeId, long hrBill_F_Id, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data);
    }

    public class HrDataService : IHrDataService
    {
        private const string HR_TABLE_NAME_PREFIX = OrganizationConstants.HR_TABLE_NAME_PREFIX;
        private const string HR_TABLE_F_IDENTITY = OrganizationConstants.HR_TABLE_F_IDENTITY;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly IHrTypeService _hrTypeService;

        public HrDataService(
            ILogger<HrDataService> logger,
            IActivityLogService activityLogService,
            IMapper mapper,
            OrganizationDBContext organizationDBContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            ICurrentContextService currentContextService,
            ICategoryHelperService httpCategoryHelperService,
            IOutsideMappingHelperService outsideMappingHelperService,
            IHrTypeService hrTypeService)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _organizationDBContext = organizationDBContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _hrTypeService = hrTypeService;
        }

        #region public
        public async Task<PageDataTable> SearchHr(int hrTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var viewInfo = await _organizationDBContext.HrTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var hrTypeViewId = viewInfo?.HrTypeViewId;

            var hrAreas = await (from t in _organizationDBContext.HrType
                                 join a in _organizationDBContext.HrArea on t.HrTypeId equals a.HrTypeId
                                 where t.HrTypeId == hrTypeId && a.IsMultiRow == false
                                 select new
                                 {
                                     t.HrTypeCode,
                                     a.HrAreaCode,
                                     a.HrAreaId,
                                     t.HrTypeId, 
                                     a.IsMultiRow
                                 }).ToListAsync();

            var fields = (await GetHrFields(hrTypeId)).Where(x => hrAreas.Any(y => y.HrAreaId == x.HrAreaId)).ToList();

            /* 
             * Xử lý câu truy vấn lấy dữ liệu từ các vùng dữ liệu 
             * trong thiết lập chứng từ hành chính nhân sự
            */
            var mainJoin = " FROM HrBill bill";
            var mainColumn = "SELECT bill.F_Id AS F_Id";
            foreach (var hrArea in hrAreas)
            {
                var (alias, columns) = GetAliasViewAreaTable(hrArea.HrTypeCode, hrArea.HrAreaCode, fields.Where(x => x.HrAreaId == hrArea.HrAreaId), hrArea.IsMultiRow);
                mainJoin += @$" LEFT JOIN ({alias}) AS v{hrArea.HrAreaCode}
                                    ON bill.[F_Id] = [v{hrArea.HrAreaCode}].[HrBill_F_Id]
                                
                                ";
                mainColumn += ", " + string.Join(", ", columns.Select(c => $"[v{hrArea.HrAreaCode}].[{c}]"));
            }

            /* 
             * Xử lý các bộ lọc
            */
            var kvFields = fields.ToDictionary(f => f.FieldName, f => f);

            var viewFields = await (
                 from f in _organizationDBContext.HrTypeViewField
                 where f.HrTypeViewId == hrTypeViewId
                 select f
             ).ToListAsync();

            var whereCondition = new StringBuilder("1 = 1");
            var sqlParams = new List<SqlParameter>();
            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.HrTypeViewFieldId == filter.Key);
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

                            filterClause.FilterClauseProcess("tmp", "r", ref whereCondition, ref sqlParams, ref suffix, false, value);
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

                columnsFilters.FilterClauseProcess("tmp", "r", ref whereCondition, ref sqlParams, ref suffix);
            }

            if (string.IsNullOrWhiteSpace(orderByFieldName) || !mainColumn.Contains(orderByFieldName))
            {
                orderByFieldName = mainColumn.Contains("ngay_ct") ? "ngay_ct" : "F_Id";
                asc = false;
            }

            sqlParams.Add(new SqlParameter("@HrTypeId", hrTypeId));
            
            /* 
                * Tính toán tổng số dòng dữ liệu trả về cho clients
             */
            var totalSql = @$"
                ; WITH tmp AS(
                    {mainColumn}
                    {mainJoin} 
                    WHERE bill.HrTypeId = @HrTypeId AND {GlobalFilter()}
                )
                SELECT COUNT(DISTINCT r.F_Id) as Total FROM tmp r WHERE {whereCondition}
                ";

            var table = await _organizationDBContext.QueryDataTable(totalSql, sqlParams.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            /* 
                * Lấy dữ liệu trả về cho client
             */
            var dataSql = @$"
                 ;WITH tmp AS (
                    
                    {mainColumn}
                    {mainJoin} 
                    WHERE bill.HrTypeId = @HrTypeId AND {GlobalFilter()}
                    
                )
                SELECT 
                    r.*
                FROM tmp r WHERE 1 = 1 AND {whereCondition}
                ORDER BY r.[{orderByFieldName}] {(asc ? "" : "DESC")}
                ";

            if (size >= 0)
            {
                dataSql += @$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY";
            }

            var data = await _organizationDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return (data, total);
        }

        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetHr(int hrTypeId, long hrBill_F_Id)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            ValidateExistenceHrBill(hrBill_F_Id);

            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            var fields = await GetHrFields(hrTypeId);



            var results = new NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>();
            for (int i = 0; i < hrAreas.Count; i++)
            {
                var hrArea = hrAreas[i];

                var (alias, columns) = GetAliasViewAreaTable(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode, fields.Where(x => x.HrAreaId == hrArea.HrAreaId), hrArea.IsMultiRow);
                var query = $"{alias} AND [row].[HrBill_F_Id] = @HrBill_F_Id";

                var data = (await _organizationDBContext.QueryDataTable(query, new[] { new SqlParameter("@HrBill_F_Id", hrBill_F_Id) })).ConvertData();

                results.Add(hrArea.HrAreaCode, data);
            }

            return results;
        }

        public async Task<bool> DeleteHr(int hrTypeId, long hrBill_F_Id)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _organizationDBContext.HrBill.FirstOrDefaultAsync(b => b.FId == hrBill_F_Id);

                if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ hành chính nhân sự");

                for (int i = 0; i < hrAreas.Count; i++)
                {
                    var hrArea = hrAreas[i];

                    var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

                    var deleteSql = $@"UPDATE [{tableName}] SET [UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc, [DeletedDatetimeUtc] = @DeletedDatetimeUtc, [IsDeleted] = @IsDeleted WHERE [HrBill_F_Id] = @HrBill_F_Id";
                    var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(deleteSql, new[] {
                        new SqlParameter($"@HrBill_F_Id", hrBill_F_Id),
                        new SqlParameter($"@DeletedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedByUserId", _currentContextService.UserId),
                        new SqlParameter($"@IsDeleted", true),
                        });
                }

                billInfo.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                await @trans.CommitAsync();
                return true;
            }
            catch (System.Exception)
            {
                await @trans.TryRollbackTransactionAsync();
                _logger.LogError("HrDataService: DeleteHr");
                throw;
            }
        }

        public async Task<bool> UpdateHr(int hrTypeId, long hrBill_F_Id, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);

            ValidateExistenceHrBill(hrBill_F_Id);

            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {

                for (int i = 0; i < hrAreas.Count; i++)
                {
                    var hrArea = hrAreas[i];

                    if (!data.ContainsKey(hrArea.HrAreaCode) || data[hrArea.HrAreaCode].Count == 0) continue;

                    var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

                    var hrAreaData = hrArea.IsMultiRow ? data[hrArea.HrAreaCode] : new[] { data[hrArea.HrAreaCode][0] };
                    var hrAreaFields = await GetHrFields(hrTypeId, hrArea.HrAreaId);

                    var sqlOldData = @$"SELECT [{HR_TABLE_F_IDENTITY}], [HrBill_F_Id]
                                    FROM [{tableName}] WHERE [HrBill_F_Id] = @HrBill_F_Id AND [IsDeleted] = @IsDeleted";
                    var oldHrAreaData = (await _organizationDBContext.QueryDataTable(sqlOldData, new SqlParameter[] {
                        new SqlParameter("@HrBill_F_Id", hrBill_F_Id),
                        new SqlParameter("@IsDeleted", false)
                        })).ConvertData<HrAreaTableBaseInfo>();

                    foreach (var old in oldHrAreaData)
                    {
                        var oldData = hrAreaData.FirstOrDefault(x => x.ContainsKey(HR_TABLE_F_IDENTITY) && x[HR_TABLE_F_IDENTITY].ToString() == old.F_Id.ToString());
                        if (null != oldData)
                        {
                            var columns = new List<string>();
                            var sqlParams = new List<SqlParameter>();

                            var updateSql = new StringBuilder($"UPDATE [{tableName}] SET ");
                            foreach (var (field, fIndex) in hrAreaFields.Select((value, fIndex) => (value, fIndex)))
                            {
                                if (!oldData.ContainsKey(field.FieldName)) continue;

                                var paramName = $"@{field.FieldName}";

                                updateSql.Append($"[{field.FieldName}] = {paramName}, ");

                                sqlParams.Add(new SqlParameter(paramName, ((EnumDataType)field.DataTypeId).GetSqlValue(oldData[field.FieldName])));
                            }
                            updateSql.Append($"[UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc WHERE [{HR_TABLE_F_IDENTITY}] = @{HR_TABLE_F_IDENTITY}");

                            sqlParams.Add(new SqlParameter("@UpdatedDatetimeUtc", DateTime.UtcNow));
                            sqlParams.Add(new SqlParameter("@UpdatedByUserId", _currentContextService.UserId));
                            sqlParams.Add(new SqlParameter($"@{HR_TABLE_F_IDENTITY}", old.F_Id));

                            var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(updateSql.ToString(), sqlParams);
                        }
                        else
                        {
                            var deleteSql = $@" UPDATE [{tableName}] 
                                                SET [UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc, [DeletedDatetimeUtc] = @DeletedDatetimeUtc, [IsDeleted] = @IsDeleted 
                                                WHERE [{HR_TABLE_F_IDENTITY}] = @{HR_TABLE_F_IDENTITY}";
                            var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(deleteSql, new[] {
                                        new SqlParameter($"@{HR_TABLE_F_IDENTITY}", old.F_Id),
                                        new SqlParameter($"@DeletedDatetimeUtc", DateTime.UtcNow),
                                        new SqlParameter($"@UpdatedDatetimeUtc", DateTime.UtcNow),
                                        new SqlParameter($"@UpdatedByUserId", _currentContextService.UserId),
                                        new SqlParameter($"@IsDeleted", true),
                                        });
                        }

                    }

                    var newHrAreaData = hrAreaData.Where(x => !x.ContainsKey(HR_TABLE_F_IDENTITY) || string.IsNullOrWhiteSpace(x[HR_TABLE_F_IDENTITY].ToString()))
                        .ToList();
                    await AddHrBillBase(hrTypeId, hrBill_F_Id, tableName, hrAreaData, hrAreaFields, newHrAreaData);
                }

                await @trans.CommitAsync();

                return true;

            }
            catch (System.Exception)
            {
                await @trans.TryRollbackTransactionAsync();
                _logger.LogError("HrDataService: UpdateHr");
                throw;
            }
        }

        public async Task<long> CreateHr(int hrTypeId, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {

                var billInfo = new HrBill()
                {
                    HrTypeId = hrTypeId,
                    LatestBillVersion = 1,
                    SubsidiaryId = _currentContextService.SubsidiaryId,
                    IsDeleted = false
                };

                await _organizationDBContext.HrBill.AddAsync(billInfo);

                await _organizationDBContext.SaveChangesAsync();

                for (int i = 0; i < hrAreas.Count; i++)
                {
                    var hrArea = hrAreas[i];

                    if (!data.ContainsKey(hrArea.HrAreaCode) || data[hrArea.HrAreaCode].Count == 0) continue;
                    
                    var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

                    var hrAreaData = hrArea.IsMultiRow ? data[hrArea.HrAreaCode] : new[] { data[hrArea.HrAreaCode][0] };
                    var hrAreaFields = await GetHrFields(hrTypeId, hrArea.HrAreaId);

                    await AddHrBillBase(hrTypeId, billInfo.FId, tableName, hrAreaData, hrAreaFields, hrAreaData);
                    
                }

                await @trans.CommitAsync();

                return 1;

            }
            catch (System.Exception)
            {
                await @trans.TryRollbackTransactionAsync();
                _logger.LogError("HrDataService: CreateHr");
                throw;
            }
        }

        #endregion

        #region private

        private (string, IList<string>) GetAliasViewAreaTable(string hrTypeCode, string hrAreaCode, IEnumerable<ValidateField> fields, bool isMultiRow = false)
        {
            var tableName = GetHrAreaTableName(hrTypeCode, hrAreaCode);
            var @selectColumn = @$"
                row.F_Id,
                row.HrBill_F_Id,
                bill.HrTypeId
            ";
            var @join = @$"
                FROM {tableName} AS row WITH(NOLOCK)
                JOIN HrBill AS bill WITH(NOLOCK) ON row.HrBill_F_Id = bill.F_Id
            ";

            var @columns = new List<string>();

            foreach (var field in fields)
            {
                @selectColumn += $", [row].[{field.FieldName}]";
                @columns.Add(field.FieldName);

                if (!string.IsNullOrWhiteSpace(field.RefTableCode)
                    && ((EnumFormType)field.FormTypeId).IsJoinForm()
                    && !string.IsNullOrWhiteSpace(field.RefTableTitle))
                {
                    var refFields = field.RefTableTitle.Split(",").Select(refTitle => $", [v{field.FieldName}].[{refTitle}] AS [{field.FieldName}_{refTitle}]");

                    @selectColumn += string.Join("", refFields);
                    @columns.AddRange(field.RefTableTitle.Split(",").Select(refTitle => $"{field.FieldName}_{refTitle}"));

                    @join += $" LEFT JOIN [v{field.RefTableCode}] as [v{field.FieldName}] WITH(NOLOCK) ON [row].[{field.FieldName}] = [v{field.FieldName}].[{field.RefTableField}]";
                }
            }

            return ($"SELECT {(isMultiRow ? "" : "TOP 1")} {@selectColumn} {@join} WHERE [row].IsDeleted = 0 AND [row].SubsidiaryId = { _currentContextService.SubsidiaryId}", columns);
        }

        private void ValidateExistenceHrBill(long hrBill_F_Id)
        {
            if (!_organizationDBContext.HrBill.Any(x => x.FId == hrBill_F_Id))
                throw new BadRequestException(HrErrorCode.HrValueBillNotFound);
        }

        private async Task AddHrBillBase(int hrTypeId, long hrBill_F_Id, string tableName, IEnumerable<NonCamelCaseDictionary> hrAreaData, IEnumerable<ValidateField> hrAreaFields, IEnumerable<NonCamelCaseDictionary> newHrAreaData)
        {
            var checkData = newHrAreaData.Select(data => new ValidateRowModel(data, null))
                                                             .ToList();

            var requiredFields = hrAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = hrAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = hrAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkData, requiredFields, hrAreaFields);
            // Check refer
            await CheckReferAsync(checkData, selectFields);
            // Check unique
            await CheckUniqueAsync(hrTypeId, tableName, checkData, uniqueFields);
            // Check value
            CheckValue(checkData, hrAreaFields);

            var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();
            var infoFields = hrAreaFields.Where(f => !f.IsMultiRow).ToDictionary(f => f.FieldName, f => f);

            await FillGenerateColumn(hrBill_F_Id, generateTypeLastValues, infoFields, hrAreaData);


            foreach (var row in newHrAreaData)
            {
                var columns = new List<string>();
                var sqlParams = new List<SqlParameter>();

                foreach (var f in hrAreaFields)
                {
                    if (!row.ContainsKey(f.FieldName)) continue;

                    columns.Add(f.FieldName);
                    sqlParams.Add(new SqlParameter("@" + f.FieldName, ((EnumDataType)f.DataTypeId).GetSqlValue(row[f.FieldName])));
                }

                columns.AddRange(GetColumnGlobal());
                sqlParams.AddRange(GetSqlParamsGlobal());

                columns.Add("HrBill_F_Id");
                sqlParams.Add(new SqlParameter("@HrBill_F_Id", hrBill_F_Id));

                var idParam = new SqlParameter("@F_Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                sqlParams.Add(idParam);


                var sql = $"INSERT INTO [{tableName}]({string.Join(",", columns.Select(c => $"[{c}]"))}) VALUES({string.Join(",", sqlParams.Where(p => p.ParameterName != "@F_Id").Select(p => $"{p.ParameterName}"))}); SELECT @F_Id = SCOPE_IDENTITY();";

                await _organizationDBContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);

                if (null == idParam.Value)
                    throw new InvalidProgramException();

            }
        }

        private async Task FillGenerateColumn(long? fId, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues, Dictionary<string, ValidateField> fields, IEnumerable<NonCamelCaseDictionary> rows)
        {
            for (var i = 0; i < rows.Count(); i++)
            {
                var row = rows.ElementAt(i);

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
                            currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.HrTypeRow, EnumObjectType.HrAreaField, field.HrAreaFieldId, fId, code, ngayCtValue);

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
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã " + field.Title);
                            }


                            value = generated.CustomCode;
                            lastTypeValue.LastValue = generated.LastValue;
                            lastTypeValue.LastCode = generated.CustomCode;
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

        private string[] GetColumnGlobal()
        {
            return new string[]
            {
                "CreatedByUserId",
                "UpdatedByUserId",
                "CreatedDatetimeUtc",
                "UpdatedDatetimeUtc",
                "SubsidiaryId"
            };
        }

        private SqlParameter[] GetSqlParamsGlobal()
        {
            return new SqlParameter[]
            {
                new SqlParameter("@CreatedByUserId", _currentContextService.UserId),
                new SqlParameter("@UpdatedByUserId", _currentContextService.UserId),
                new SqlParameter("@CreatedDatetimeUtc", DateTime.UtcNow),
                new SqlParameter("@UpdatedDatetimeUtc",DateTime.UtcNow),
                new SqlParameter("@SubsidiaryId", _currentContextService.SubsidiaryId)
            };
        }

        private void CheckValue(IEnumerable<ValidateRowModel> rows, IEnumerable<ValidateField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                foreach (var row in rows)
                {
                    ValidValueAsync(row, field);
                }
            }
        }

        private void ValidValueAsync(ValidateRowModel checkData, ValidateField field)
        {
            if (checkData.CheckFields != null && !checkData.CheckFields.Contains(field.FieldName))
            {
                return;
            }

            checkData.Data.TryGetValue(field.FieldName, out string value);

            if (string.IsNullOrEmpty(value))
                return;

            if (((EnumFormType)field.FormTypeId).IsSelectForm() || field.IsAutoIncrement || string.IsNullOrEmpty(value))
                return;

            string regex = ((EnumDataType)field.DataTypeId).GetRegex();
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
            {
                throw new BadRequestException(HrErrorCode.HrValueInValid, new object[] { value?.JsonSerialize(), field.HrAreaCode, field.Title });
            }
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

        private async Task CheckRequired(IEnumerable<ValidateRowModel> rows, IEnumerable<ValidateField> requiredFields, IEnumerable<ValidateField> hrAreaFields)
        {
            var filters = requiredFields
                .Where(f => !string.IsNullOrEmpty(f.RequireFilters))
                .ToDictionary(f => f.FieldName, f => JsonConvert.DeserializeObject<Clause>(f.RequireFilters));

            string[] filterFieldNames = GetFieldInFilter(filters.Select(f => f.Value).ToArray());
            var sfFields = hrAreaFields.Where(f => ((EnumFormType)f.FormTypeId).IsSelectForm() && filterFieldNames.Contains(f.FieldName)).ToList();
            var sfValues = new Dictionary<string, Dictionary<object, object>>();

            foreach (var field in sfFields)
            {
                var values = rows.Where(r => r.Data.ContainsKey(field.FieldName) && r.Data[field.FieldName] != null).Select(r => r.Data[field.FieldName]).ToList();

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
                    var data = await _organizationDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        mapTitles.Add(data.Rows[i][field.RefTableField], data.Rows[i][field.RefTableTitle]);
                    }
                    sfValues.Add(field.FieldName, mapTitles);
                }
            }


            foreach (var field in requiredFields)
            {
                // ignore auto generate field
                if (field.FormTypeId == (int)EnumFormType.Generate) continue;


                foreach (var (row, index) in rows.Select((value, i) => (value, i + 1)))
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(field.RequireFilters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(field.RequireFilters);
                        if (filterClause != null && !(await CheckRequireFilter(filterClause, rows, hrAreaFields, sfValues)))
                        {
                            continue;
                        }
                    }

                    row.Data.TryGetValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(HrErrorCode.RequiredFieldIsEmpty, new object[] { index, field.Title });
                    }
                }
            }
        }

        private async Task<bool> CheckRequireFilter(Clause clause, IEnumerable<ValidateRowModel> rows, IEnumerable<ValidateField> hrAreaFields, Dictionary<string, Dictionary<object, object>> sfValues, bool not = false)
        {
            bool? isRequire = null;
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    var field = hrAreaFields.First(f => f.FieldName == singleClause.FieldName);
                    // Data check nằm trong thông tin chung và data điều kiện nằm trong thông tin chi tiết
                    var rowValues = rows.Select(r =>
                    r.Data.ContainsKey(field.FieldName) ?
                        (sfValues.ContainsKey(field.FieldName) ?
                            (sfValues[field.FieldName].ContainsKey(r.Data[field.FieldName]) ?
                                sfValues[field.FieldName][r.Data[field.FieldName]]
                                    : null)
                        : r.Data[field.FieldName])
                    : null).ToList();
                    switch (singleClause.Operator)
                    {
                        case EnumOperator.Equal:
                            isRequire = rowValues.Any(v => ((EnumDataType)field.DataTypeId).CompareValue(v, singleClause.Value) == 0);
                            break;
                        case EnumOperator.NotEqual:
                            isRequire = rowValues.Any(v => ((EnumDataType)field.DataTypeId).CompareValue(v, singleClause.Value) != 0);
                            break;
                        case EnumOperator.Contains:
                            isRequire = rowValues.Any(v => v.StringContains(singleClause.Value));
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
                            var result = await _organizationDBContext.QueryDataTable(sql.ToString(), sqlParams.ToArray());
                            isRequire = result != null && result.Rows.Count > 0;
                            break;
                        case EnumOperator.StartsWith:
                            isRequire = rowValues.Any(v => v.StringStartsWith(singleClause.Value));
                            break;
                        case EnumOperator.EndsWith:
                            isRequire = rowValues.Any(v => v.StringEndsWith(singleClause.Value));
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
                        case EnumOperator.Greater:
                            isRequire = rowValues.Any(value => ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) > 0);
                            break;
                        case EnumOperator.GreaterOrEqual:
                            isRequire = rowValues.Any(value => ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) >= 0);
                            break;
                        case EnumOperator.LessThan:
                            isRequire = rowValues.Any(value => ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) < 0);
                            break;
                        case EnumOperator.LessThanOrEqual:
                            isRequire = rowValues.Any(value => ((EnumDataType)field.DataTypeId).CompareValue(value, singleClause.Value) <= 0);
                            break;
                        default:
                            isRequire = true;
                            break;
                    }

                    isRequire = not ? !isRequire : isRequire;
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    for (int i = 0; i < arrClause.Rules.Count; i++)
                    {
                        bool clauseResult = await CheckRequireFilter(arrClause.Rules.ElementAt(i), rows, hrAreaFields, sfValues, isNot);
                        isRequire = isRequire.HasValue ? isOr ? isRequire.Value || clauseResult : isRequire.Value && clauseResult : clauseResult;
                    }
                }
            }
            return isRequire.Value;
        }

        private async Task CheckUniqueAsync(int hrTypeId, string tableName, List<ValidateRowModel> rows, List<ValidateField> uniqueFields, long? hrValueBillId = null)
        {
            // Check unique
            foreach (var field in uniqueFields)
            {
                foreach (var row in rows)
                {
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
                        throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                    }
                    if (values.Count == 0)
                    {
                        continue;
                    }
                    // Checkin unique trong db
                    await ValidUniqueAsync(hrTypeId, tableName, values, field, hrValueBillId);
                }
            }
        }

        private async Task ValidUniqueAsync(int hrTypeId, string tableName, List<object> values, ValidateField field, long? HrValueBillId = null)
        {
            var existSql = $"SELECT F_Id FROM {tableName} WHERE 1 = 1 ";
            if (HrValueBillId.HasValue)
            {
                existSql += $"AND HrBill_F_Id != {HrValueBillId}";
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
            var result = await _organizationDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;

            if (isExisted)
            {
                throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
            }
        }

        private async Task CheckReferAsync(List<ValidateRowModel> rows, List<ValidateField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    await ValidReferAsync(rows[i], field);
                }
            }
        }

        private async Task ValidReferAsync(ValidateRowModel checkData, ValidateField field)
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
                    filterClause.FilterClauseProcess(tableName, tableName, ref whereCondition, ref sqlParams, ref suffix);
                }
            }

            if (whereCondition.Length > 0)
            {
                existSql += $" AND {whereCondition}";
            }

            var result = await _organizationDBContext.QueryDataTable(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, value) };
                result = await _organizationDBContext.QueryDataTable(checkExistedReferSql, checkExistedReferParams.ToArray());
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotFound, new object[] { field.HrAreaCode, field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotValidFilter, new object[] { field.HrAreaCode, field.Title + ": " + value });
                }
            }
        }

        private async Task<List<ValidateField>> GetHrFields(int hrTypeId, int? areaId = null)
        {
            var area = _organizationDBContext.HrArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.HrAreaId == areaId);
            }
            return await (from af in _organizationDBContext.HrAreaField
                          join f in _organizationDBContext.HrField on af.HrFieldId equals f.HrFieldId
                          join a in area on af.HrAreaId equals a.HrAreaId
                          where af.HrTypeId == hrTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly //&& f.FieldName != AccountantConstants.F_IDENTITY
                          orderby a.SortOrder, af.SortOrder
                          select new ValidateField
                          {
                              HrAreaFieldId = af.HrAreaFieldId,
                              Title = af.Title,
                              IsAutoIncrement = af.IsAutoIncrement,
                              IsHidden = af.IsHidden,
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
                              HrAreaCode = a.HrAreaCode,
                              HrAreaId = a.HrAreaId
                          }).ToListAsync();
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

        private async Task<HrTypeExecData> GetHrTypExecInfo(int hrTypeId)
        {
            var global = await _hrTypeService.GetHrGlobalSetting();
            var hrTypeInfo = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);

            if (hrTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ hành chính nhân sự");

            var info = _mapper.Map<HrTypeExecData>(hrTypeInfo);
            info.GlobalSetting = global;
            return info;
        }

        private string GlobalFilter()
        {
            return $"bill.SubsidiaryId = { _currentContextService.SubsidiaryId} AND bill.IsDeleted = 0";
        }

        private string GetHrAreaTableName(string hrTypeCode, string hrAreaCode)
        {
            return $"{HR_TABLE_NAME_PREFIX}_{hrTypeCode}_{hrAreaCode}";
        }

        #endregion

        #region protected class
             
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

        protected class ValidateField
        {
            public int HrAreaFieldId { get; set; }
            public string Title { get; set; }
            public bool IsAutoIncrement { get; set; }
            public bool IsHidden { get; set; }
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
            public string HrAreaCode { get; internal set; }
            public int HrAreaId { get; internal set; }
        }

        protected class HrAreaTableBaseInfo
        {
            public long F_Id { get; set; }
            public long HrBill_F_Id { get; set; }
        }
        #endregion

    }

}