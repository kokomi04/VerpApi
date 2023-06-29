using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
using Newtonsoft.Json;
using NPOI.SS.Formula.PTG;
using Org.BouncyCastle.Ocsp;
using Services.Organization.Model.HrConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.GlobalObject;
using Verp.Resources.Organization;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Model.Employee;
using VErp.Services.Organization.Service.HrConfig.Abstract;
using VErp.Services.Organization.Service.HrConfig.Facade;
using static VErp.Commons.Library.EvalUtils;
using static VErp.Commons.Library.ExcelReader;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrDataService
    {
        Task<long> CreateHr(int hrTypeId, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data);
        Task<bool> UpdateHr(int hrTypeId, long hrBill_F_Id, NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data);

        Task<bool> DeleteHr(int hrTypeId, long hrBill_F_Id);
        Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetHr(int hrTypeId, long hrBill_F_Id);
        //Task<PageDataTable> SearchHr(int hrTypeId, HrTypeBillsFilterModel req, int page, int size);
        Task<PageDataTable> SearchHrV2(int hrTypeId, bool isSelectMultirowArea, HrTypeBillsFilterModel req, int page, int size);
        Task<(Stream stream, string fileName, string contentType)> Export(int hrTypeId, HrTypeBillsExportModel req);
        Task<CategoryNameModel> GetFieldDataForMapping(int hrTypeId, int? areaId);
        Task<bool> ImportHrBillFromMapping(int hrTypeId, ImportExcelMapping mapping, Stream stream);
        Task<bool> UpdateHrBillReference(int hrTypeId, int hrAreaId, long hrBill_F_Id, long hrBillReference_F_Id);
        Task<(string query, IList<string> fieldNames)> BuildHrQuery(string hrTypeCode, bool includedMultiRowArea);
    }

    public class HrDataService : HrDataUpdateServiceAbstract, IHrDataService
    {


        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IHrTypeService _hrTypeService;
        private readonly IHrDataImportDIService _hrDataImportDIService;
        private readonly ObjectActivityLogFacade _hrDataActivityLog;

        public HrDataService(
            ILogger<HrDataService> logger,
            IActivityLogService activityLogService,
            IMapper mapper,
            OrganizationDBContext organizationDBContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            ICurrentContextService currentContextService,
            ICategoryHelperService categoryHelperService,
            IHrTypeService hrTypeService,
            IHrDataImportDIService hrDataImportDIService) : base(organizationDBContext, customGenCodeHelperService, currentContextService, categoryHelperService)
        {
            _logger = logger;
            _mapper = mapper;
            _hrTypeService = hrTypeService;
            _hrDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.HrBill);
            _hrDataImportDIService = hrDataImportDIService;
        }

        #region public
        /// <summary>
        /// When update ref bill => Update main bill set to ref Id
        /// </summary>
        /// <param name="hrTypeId"></param>
        /// <param name="hrAreaId"></param>
        /// <param name="hrBill_F_Id"></param>
        /// <param name="hrBillReference_F_Id"></param>
        /// <returns></returns>
        public async Task<bool> UpdateHrBillReference(int hrTypeId, int hrAreaId, long hrBill_F_Id, long hrBillReference_F_Id)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            ValidateExistenceHrBill(hrBill_F_Id);

            var hrArea = await _organizationDBContext.HrArea.FirstOrDefaultAsync(x => x.HrAreaId == hrAreaId);

            var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

            var existSql = $"SELECT v.[F_Id] FROM [{tableName}] v WHERE v.[HrBill_F_Id] = @HrBill_F_Id";

            var result = await _organizationDBContext.QueryDataTableRaw(existSql, new SqlParameter[] { new SqlParameter("@HrBill_F_Id", hrBill_F_Id) });
            bool isExisted = result != null && result.Rows.Count > 0;

            if (!isExisted)
            {
                await CreateFistRowReferenceData(hrBill_F_Id, hrBillReference_F_Id, tableName);
            }
            else
            {
                var sqlParams = new List<SqlParameter>();
                var updateSql = new StringBuilder($"UPDATE [{tableName}] SET [HrBillReference_F_Id] = @HrBillReference_F_Id, ");

                updateSql.Append($"[UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc WHERE [HrBill_F_Id] = @HrBill_F_Id");

                sqlParams.Add(new SqlParameter("@UpdatedDatetimeUtc", DateTime.UtcNow));
                sqlParams.Add(new SqlParameter("@UpdatedByUserId", _currentContextService.UserId));
                sqlParams.Add(new SqlParameter($"@HrBill_F_Id", hrBill_F_Id));
                sqlParams.Add(new SqlParameter($"@HrBillReference_F_Id", hrBillReference_F_Id));

                var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(updateSql.ToString(), sqlParams);
            }

            return true;
        }

        /*
        public async Task<PageDataTable> SearchHr(int hrTypeId, HrTypeBillsFilterModel req, int page, int size)
        {
            var keyword = (req?.Keyword ?? "").Trim();
            var fromDate = req.FromDate;
            var toDate = req.ToDate;
            var filters = req.Filters;
            var columnsFilters = req.ColumnsFilters;
            var orderByFieldName = req.OrderBy;
            var asc = req.Asc;


            var viewInfo = await _organizationDBContext.HrTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var hrTypeViewId = viewInfo?.HrTypeViewId;

            var hrAreas = await (from t in _organizationDBContext.HrType
                                 join a in _organizationDBContext.HrArea on t.HrTypeId equals a.HrTypeId
                                 where t.HrTypeId == hrTypeId && a.IsMultiRow == false && a.HrTypeReferenceId.HasValue == false
                                 select new
                                 {
                                     t.HrTypeCode,
                                     a.HrAreaCode,
                                     a.HrAreaId,
                                     t.HrTypeId,
                                     a.IsMultiRow
                                 }).ToListAsync();

            var fields = (await GetHrFields(hrTypeId, null, true)).Where(x => hrAreas.Any(y => y.HrAreaId == x.HrAreaId) && x.FormTypeId != EnumFormType.MultiSelect).ToList();

            /* 
             * Xử lý câu truy vấn lấy dữ liệu từ các vùng dữ liệu 
             * trong thiết lập chứng từ hành chính nhân sự
            * /
            var mainJoin = " FROM HrBill bill";
            var mainColumn = "SELECT bill.F_Id AS F_Id, CreatedDatetimeUtc ";
            foreach (var hrArea in hrAreas)
            {
                var (alias, columns) = GetAliasViewAreaTable(hrArea.HrTypeCode, hrArea.HrAreaCode, fields.Where(x => x.HrAreaId == hrArea.HrAreaId), isMultiRow: true);
                mainJoin += @$" LEFT JOIN ({alias}) AS v{hrArea.HrAreaCode}
                                    ON bill.[F_Id] = [v{hrArea.HrAreaCode}].[HrBill_F_Id]
                                
                                ";
                if (columns.Count > 0)
                    mainColumn += ", " + string.Join(", ", columns.Select(c => $"[v{hrArea.HrAreaCode}].[{c}]"));
            }//CreatedDatetimeUtc

            /* 
             * Xử lý các bộ lọc
            * /
            var kvFields = fields.ToDictionary(f => f.FieldName, f => f);

            var viewFields = await (
                 from f in _organizationDBContext.HrTypeViewField
                 where f.HrTypeViewId == hrTypeViewId
                 select f
             ).ToListAsync();

            var whereCondition = new StringBuilder("1 = 1");
            var sqlParams = new List<SqlParameter>();
            if (fromDate.HasValue && toDate.HasValue)
            {
                var dateField = "CreatedDatetimeUtc";
                if (mainColumn.Contains("ngay_ct"))
                {
                    dateField = "ngay_ct";
                }
                whereCondition.Append($" AND r.{dateField} BETWEEN @FromDate AND @ToDate");

                sqlParams.Add(new SqlParameter("@FromDate", EnumDataType.Date.GetSqlValue(fromDate.Value)));
                sqlParams.Add(new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate.Value)));
            }


            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.HrTypeViewFieldId == filter.Key);
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

                            suffix = filterClause.FilterClauseProcess("tmp", "r", whereCondition, sqlParams, suffix, false, value);
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

                suffix = columnsFilters.FilterClauseProcess("tmp", "r", whereCondition, sqlParams, suffix);
            }

            if (string.IsNullOrWhiteSpace(orderByFieldName) || !mainColumn.Contains(orderByFieldName))
            {
                orderByFieldName = mainColumn.Contains("ngay_ct") ? "ngay_ct" : "F_Id";
                asc = false;
            }

            sqlParams.Add(new SqlParameter("@HrTypeId", hrTypeId));


            /* 
                * Tính toán tổng số dòng dữ liệu trả về cho clients
             * /
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
             * /
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
                FETCH NEXT {size}
                ROWS ONLY";
            }

            var data = await _organizationDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return (data, total);
        }
*/
        public async Task<PageDataTable> SearchHrV2(int hrTypeId, bool isSelectMultirowArea, HrTypeBillsFilterModel req, int page, int size)
        {
            return await GetHrData(hrTypeId, isSelectMultirowArea, null, req, page, size);
        }

        public async Task<PageDataTable> GetHrData(int hrTypeId, bool isSelectMultirowArea, IList<int> selectAreaIds, HrTypeBillsFilterModel req, int page, int size)
        {
            var keyword = (req?.Keyword ?? "").Trim();
            var fromDate = req.FromDate;
            var toDate = req.ToDate;
            var filters = req.Filters;
            var columnsFilters = req.ColumnsFilters;
            var orderByFieldName = req.OrderBy;
            var asc = req.Asc;

            var viewInfo = await _organizationDBContext.HrTypeView.OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

            var hrTypeViewId = viewInfo?.HrTypeViewId;

            var hrAreas = await (from t in _organizationDBContext.HrType
                                 join a in _organizationDBContext.HrArea on t.HrTypeId equals a.HrTypeId
                                 where t.HrTypeId == hrTypeId && (isSelectMultirowArea || !a.IsMultiRow) && !a.HrTypeReferenceId.HasValue
                                 select new
                                 {
                                     t.HrTypeCode,
                                     a.HrAreaCode,
                                     a.HrAreaId,
                                     t.HrTypeId,
                                     a.IsMultiRow
                                 }).ToListAsync();

            var fields = (await GetHrFields(hrTypeId, null, true)).Where(x => hrAreas.Any(y => y.HrAreaId == x.HrAreaId) && x.FormTypeId != EnumFormType.MultiSelect).ToList();

            var fieldsByArea = fields
                .GroupBy(f => f.HrAreaId)
                .ToDictionary(a => a.Key, a => a.ToList());
            /* 
             * Xử lý câu truy vấn lấy dữ liệu từ các vùng dữ liệu 
             * trong thiết lập chứng từ hành chính nhân sự
            */
            var mainJoin = new StringBuilder(" FROM HrBill bill");
            var mainColumn = new StringBuilder($"SELECT bill.F_Id AS {HR_BILL_ID_FIELD_IN_AREA}, CreatedDatetimeUtc ");

            var aliasTableOfFields = new Dictionary<string, string>();
            foreach (var hrArea in hrAreas.Where(a => !a.IsMultiRow))
            {
                var areaAlias = $"v{hrArea.HrAreaCode}";
                var (query, columns) = GetAliasViewAreaTable(hrArea.HrTypeCode, hrArea.HrAreaCode, fieldsByArea[hrArea.HrAreaId], isMultiRow: true);
                mainJoin.AppendLine(@$" LEFT JOIN ({query}) AS {areaAlias}
                                    ON bill.[F_Id] = [{areaAlias}].[HrBill_F_Id]                                
                                ");
                if (columns.Count > 0)
                    mainColumn.Append(", " + string.Join(", ", columns.Select(c => $"[v{hrArea.HrAreaCode}].[{c}]")));
                foreach (var c in columns)
                {
                    aliasTableOfFields.TryAdd(c, areaAlias);
                }

            }//CreatedDatetimeUtc

            /* 
             * Xử lý các bộ lọc
            */

            var viewFields = await (
                 from f in _organizationDBContext.HrTypeViewField
                 where f.HrTypeViewId == hrTypeViewId
                 select f
             ).ToListAsync();

            var whereCondition = new StringBuilder("1 = 1");
            var sqlParams = new List<SqlParameter>();
            if (fromDate.HasValue && toDate.HasValue)
            {
                var dateField = "CreatedDatetimeUtc";
                if (mainColumn.ToString().Contains("ngay_ct"))
                {
                    dateField = "ngay_ct";
                }
                whereCondition.Append($" AND r.{dateField} BETWEEN @FromDate AND @ToDate");

                sqlParams.Add(new SqlParameter("@FromDate", EnumDataType.Date.GetSqlValue(fromDate.Value)));
                sqlParams.Add(new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate.Value)));
            }


            int suffix = 0;
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.HrTypeViewFieldId == filter.Key);
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

                            suffix = filterClause.FilterClauseProcess("HrBill", "bill", whereCondition, sqlParams, suffix, false, value, null, aliasTableOfFields);
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

                suffix = columnsFilters.FilterClauseProcess("HrBill", "bill", whereCondition, sqlParams, suffix, false, null, null, aliasTableOfFields);
            }

            if (string.IsNullOrWhiteSpace(orderByFieldName) || !mainColumn.ToString().Contains(orderByFieldName))
            {
                orderByFieldName = mainColumn.ToString().Contains("ngay_ct") ? "ngay_ct" : "F_Id";
                asc = false;
            }

            sqlParams.Add(new SqlParameter("@HrTypeId", hrTypeId));


            /* 
                * Tính toán tổng số dòng dữ liệu trả về cho clients
             */
            var totalSql = @$"
                ; WITH data AS(
                    SELECT
                    bill.F_Id
                    {mainJoin} 
                    WHERE bill.HrTypeId = @HrTypeId AND {GlobalFilter()}
                    {(whereCondition.Length > 0 ? $" AND ({whereCondition})" : "")} 
                    GROUP BY bill.F_Id
                )
                SELECT COUNT(0) as Total FROM data
                ";

            var table = await _organizationDBContext.QueryDataTableRaw(totalSql, sqlParams.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            /* 
                * Lấy dữ liệu trả về cho client
             */
            sqlParams.Add(new SqlParameter("@Size", size));
            sqlParams.Add(new SqlParameter("@FromRow", (page - 1) * size + 1));
            sqlParams.Add(new SqlParameter("@ToRow", page * size));

            var dataSql = @$"
                 ;WITH tmp AS(
                    SELECT
                    bill.F_Id AS {HR_BILL_ID_FIELD_IN_AREA},
                    ROW_NUMBER() OVER(ORDER BY {orderByFieldName} {(asc ? "" : "DESC")}) RowNumber
                    {mainJoin} 
                    WHERE bill.HrTypeId = @HrTypeId AND {GlobalFilter()}
                    {(whereCondition.Length > 0 ? $" AND ({whereCondition})" : "")}
                )
                    {mainColumn}
                    {mainJoin}
                    JOIN tmp ON bill.F_Id = tmp.{HR_BILL_ID_FIELD_IN_AREA}
                    WHERE @Size <=0 OR (RowNumber BETWEEN @FromRow AND @ToRow)
                ";

            var singleRowData = (await _organizationDBContext.QueryDataTableRaw(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray())).ConvertData();

            var identityBills = singleRowData.GroupBy(d => new
            {
                FId = Convert.ToInt64(d[HR_BILL_ID_FIELD_IN_AREA]),
                Code = d[OrganizationConstants.BILL_CODE]
            })
            .ToDictionary(g => g.Key, g => g.ToList())
            .ToList();

            var fIds = identityBills.Select(b => b.Key.FId).Distinct().ToList();

            var bills = identityBills.Select(b =>
            new HrBillInforByAreaModel()
            {
                FId = b.Key.FId,
                Code = b.Key.Code?.ToString(),
                AreaData = fieldsByArea
                .ToDictionary(a => a.Key, a =>
                {
                    if (a.Value.First().IsMultiRow) return new List<NonCamelCaseDictionary>();
                    return b.Value;

                })
            }).ToList();

            foreach (var multiArea in hrAreas.Where(a => a.IsMultiRow))
            {
                var areaData = await GetAreaData(multiArea.HrTypeCode, fieldsByArea[multiArea.HrAreaId], fIds);

                foreach (var d in areaData)
                {
                    var info = bills.First(r => r.FId == d.Key);
                    info.AreaData[multiArea.HrAreaId].AddRange(d.Value);
                }
            }

            var result = new List<NonCamelCaseDictionary>();

            /*
             * flatten hr areas by hr bill
             * Repeat single row and fill multirow
             * Example
             * STT      Ho Ten (Single row)          Bang Cap (multirow)    Than Nhan (multirow)
             * 1        Nguyen Van A                 Dai hoc                Nguyen Van Cha
             *          Nguyen Van A                 B1 Tieng Anh           Nguyen Thi Me
             *          Nguyen Van A                 Tin hoc van phong          
             *          
             * 2        Nguyen Van B                 Dai hoc                Nguyen Van Cha 2
             *          Nguyen Van B                                        Nguyen Thi Me 2
             *          Nguyen Van B                                         
             */

            var selectAreas = hrAreas;
            if (selectAreaIds?.Count > 0)
            {
                selectAreas = selectAreas.Where(a => selectAreaIds.Contains(a.HrAreaId)).ToList();
            }

            foreach (var bill in bills)
            {
                var maxAreaRow = bill.AreaData.Max(a => a.Value.Count);
                for (var i = 0; i < maxAreaRow; i++)
                {
                    var row = new NonCamelCaseDictionary();
                    row.Add(HR_TABLE_F_IDENTITY, bill.FId);

                    foreach (var hrArea in selectAreas)
                    {
                        var areaFields = fieldsByArea[hrArea.HrAreaId];
                        var areaRows = bill.AreaData[hrArea.HrAreaId];
                        if (!hrArea.IsMultiRow)
                        {
                            foreach (var field in areaFields)
                            {
                                row.Add(field.FieldName, areaRows[0][field.FieldName]);
                                if (field.HasRefField)
                                {
                                    foreach (var titleField in field.FieldNameRefTitles)
                                    {
                                        areaRows[0].TryGetValue(titleField, out var v);
                                        row.Add(titleField, v);
                                    }

                                }
                            }
                        }
                        else
                        {
                            var rowAreaData = i < areaRows.Count ? areaRows[i] : null;
                            foreach (var field in areaFields)
                            {
                                row.Add(field.FieldName, rowAreaData?[field.FieldName]);
                                if (field.HasRefField)
                                {
                                    foreach (var titleField in field.FieldNameRefTitles)
                                    {
                                        object v = null;
                                        rowAreaData?.TryGetValue(titleField, out v);

                                        row.Add(titleField, v);
                                    }
                                }
                            }
                        }
                    }
                    result.Add(row);
                }
            }

            return (result, total);
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(int hrTypeId, HrTypeBillsExportModel req)
        {
            var hrType = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);

            var fields = await GetHrFields(hrTypeId, null, true);
            fields = fields
                .Where(f => req.FieldNames == null || req.FieldNames.Contains(f.FieldName))
                .Where(f => f.FormTypeId != EnumFormType.ImportFile)
                .ToList();
            var exportFacade = new HrDataExportFacade(hrType, fields, _currentContextService);
            var selectAreaIds = fields.Select(f => f.HrAreaId).Distinct().ToList();
            var data = await GetHrData(hrTypeId, true, selectAreaIds, req, 1, -1);
            return exportFacade.Export(data.List);
        }

        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetHr(int hrTypeId, long hrBill_F_Id)
        {
            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            // ValidateExistenceHrBill(hrBill_F_Id);

            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            var fields = await GetHrFields(hrTypeId, null, true);

            var results = new NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>();
            for (int i = 0; i < hrAreas.Count; i++)
            {
                var hrArea = hrAreas[i];
                var (alias, columns) = GetAliasViewAreaTable(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode, fields.Where(x => x.HrAreaId == hrArea.HrAreaId), hrArea.HrTypeReferenceId.HasValue ? false : hrArea.IsMultiRow);
                var query = $"{alias} AND [row].[HrBill_F_Id] IN (SELECT F_Id FROM HrBill WHERE F_Id = @HrBill_F_Id AND IsDeleted = 0)  ";

                var data = (await _organizationDBContext.QueryDataTableRaw(query, new[] { new SqlParameter("@HrBill_F_Id", hrBill_F_Id) })).ConvertData();

                results.Add(hrArea.HrAreaCode, data);
            }

            return results;
        }

        public async Task<(string query, IList<string> fieldNames)> BuildHrQuery(string hrTypeCode, bool includedMultiRowArea)
        {
            var fieldNames = new List<string>();

            var hrTypeInfo = await GetHrTypExecInfo(hrTypeCode);

            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeInfo.HrTypeId).AsNoTracking().ToListAsync();

            var fields = await GetHrFields(hrTypeInfo.HrTypeId, null, true);


            var join = new StringBuilder("FROM dbo.HrBill bill");
            var select = new StringBuilder();
            select.Append("bill.F_Id, ");

            fieldNames.Add(HR_TABLE_F_IDENTITY);

            for (int i = 0; i < hrAreas.Count; i++)
            {
                var hrArea = hrAreas[i];

                if (!includedMultiRowArea && hrArea.IsMultiRow) continue;

                var tableName = GetHrAreaTableName(hrTypeCode, hrArea.HrAreaCode);
                if (hrArea.IsMultiRow)
                {
                    join.AppendLine($" LEFT JOIN (SELECT ROW_NUMBER() OVER(PARTITION BY HrBill_F_Id ORDER BY F_Id DESC) __RowNumber, * FROM {tableName}) AS {hrArea.HrAreaCode} ON [{hrArea.HrAreaCode}].HrBill_F_Id = bill.F_Id AND [{hrArea.HrAreaCode}].IsDeleted = 0 AND [{hrArea.HrAreaCode}].__RowNumber = 1");
                }
                else
                {
                    join.AppendLine($" LEFT JOIN {tableName} AS {hrArea.HrAreaCode} ON [{hrArea.HrAreaCode}].HrBill_F_Id = bill.F_Id AND [{hrArea.HrAreaCode}].IsDeleted = 0");
                }


                foreach (var field in fields.Where(x => x.HrAreaId == hrArea.HrAreaId).ToList())
                {
                    if (field.FormTypeId == EnumFormType.SqlSelect)
                    {
                        select.Append($"{field.SqlValue} AS [{field.FieldName}], ");
                    }
                    else
                    {
                        select.Append($"[{hrArea.HrAreaCode}].[{field.FieldName}], ");
                    }

                    fieldNames.Add(field.FieldName);

                    if (!string.IsNullOrWhiteSpace(field.RefTableCode)
                        && (field.FormTypeId.IsJoinForm() || field.FormTypeId == EnumFormType.MultiSelect)
                        && !string.IsNullOrWhiteSpace(field.RefTableTitle))
                    {
                        fieldNames.AddRange(field.RefTableTitle.Split(",").Select(f => $"{field.FieldName}_{f}"));

                        if (field.FormTypeId == EnumFormType.MultiSelect)
                        {
                            var refFields = field.RefTableTitle.Split(",")
                                .Select(refTitle => @$"
                                (
                                    SELECT STRING_AGG({refTitle}, ', ') AS [{refTitle}]
                                    FROM  (
                                        SELECT [row].F_Id, [{refTitle}] 
                                        FROM v{field.RefTableCode} 
                                        WHERE [v{field.RefTableCode}].[F_Id] IN (
                                            SELECT [value] FROM OPENJSON(ISNULL([row].[{field.FieldName}],'[]')) WITH (  [value] INT '$' )
                                        )
                                    ) c GROUP BY c.F_Id
                                ) AS [{field.FieldName}_{refTitle}],
                            ");


                            select.Append($"{string.Join("", refFields)}");
                        }
                        else
                        {
                            var refFields = field.RefTableTitle.Split(",").Select(refTitle => $" [v{field.FieldName}].[{refTitle}] AS [{field.FieldName}_{refTitle}], ");
                            select.Append($"{string.Join("", refFields)}");

                            join.AppendLine($" LEFT JOIN [v{field.RefTableCode}] as [v{field.FieldName}] WITH(NOLOCK) ON [{hrArea.HrAreaCode}].[{field.FieldName}] = [v{field.FieldName}].[{field.RefTableField}]");
                        }
                    }
                }
            }

            return ($"SELECT {select.ToString().TrimEnd().TrimEnd(',')} {join} WHERE bill.IsDeleted = 0 AND bill.HrTypeId = " + hrTypeInfo.HrTypeId, fieldNames);
        }


        public async Task<bool> DeleteHr(int hrTypeId, long hrBill_F_Id)
        {
            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var (billInfo, HrTypeTitle) = await SubDeleteHr(hrTypeId, hrBill_F_Id, true);

                await @trans.CommitAsync();

                await _hrDataActivityLog.LogBuilder(() => HrBillActivityLogMessage.Update)
                        .MessageResourceFormatDatas(HrTypeTitle, hrBill_F_Id)
                        .BillTypeId(hrTypeId)
                        .ObjectId(hrBill_F_Id)
                        .JsonData(billInfo.JsonSerialize())
                        .CreateLog();

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

            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId && x.HrTypeReferenceId.HasValue == false).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var billInfo = await _organizationDBContext.HrBill.FirstOrDefaultAsync(b => b.HrTypeId == hrTypeId && b.FId == hrBill_F_Id);
                if (billInfo == null)
                {
                    throw GeneralCode.ItemNotFound.BadRequest();
                }

                billInfo.LatestBillVersion++;

                await _organizationDBContext.SaveChangesAsync();


                for (int i = 0; i < hrAreas.Count; i++)
                {
                    var hrArea = hrAreas[i];

                    //if (!data.ContainsKey(hrArea.HrAreaCode) || data[hrArea.HrAreaCode].Count == 0) continue;

                    var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

                    IList<NonCamelCaseDictionary> hrAreaData = new List<NonCamelCaseDictionary>();
                    if (data.ContainsKey(hrArea.HrAreaCode))
                    {
                        hrAreaData = hrArea.IsMultiRow ? data[hrArea.HrAreaCode] : new[] { data[hrArea.HrAreaCode][0] };
                    }

                    var hrAreaFields = await GetHrFields(hrTypeId, hrArea.HrAreaId, false);

                    var sqlOldData = @$"SELECT [{HR_TABLE_F_IDENTITY}], [HrBill_F_Id]
                                    FROM [{tableName}] WHERE [HrBill_F_Id] = @HrBill_F_Id AND [IsDeleted] = @IsDeleted";
                    var oldHrAreaData = (await _organizationDBContext.QueryDataTableRaw(sqlOldData, new SqlParameter[] {
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

                                sqlParams.Add(new SqlParameter(paramName, (field.DataTypeId).GetSqlValue(oldData[field.FieldName])));
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

                    if (hrAreaData.Count > 0)
                    {
                        var newHrAreaData = hrAreaData.Where(x => !x.ContainsKey(HR_TABLE_F_IDENTITY) || string.IsNullOrWhiteSpace(x[HR_TABLE_F_IDENTITY].ToString()))
                            .ToList();
                        await AddHrBillBase(hrTypeId, hrBill_F_Id, billInfo: null, generateTypeLastValues, tableName, hrAreaData, hrAreaFields, newHrAreaData);
                    }
                }

                await @trans.CommitAsync();

                await _hrDataActivityLog.LogBuilder(() => HrBillActivityLogMessage.Update)
                        .MessageResourceFormatDatas(hrTypeInfo.Title, hrBill_F_Id)
                        .BillTypeId(hrTypeId)
                        .ObjectId(hrBill_F_Id)
                        .JsonData(data.JsonSerialize())
                        .CreateLog();

                await ConfirmCustomGenCode(generateTypeLastValues);

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
            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId && x.HrTypeReferenceId.HasValue == false).AsNoTracking().ToListAsync();
            var hrAreaReferences = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId && x.HrTypeReferenceId.HasValue == true).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

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
                    var hrAreaFields = await GetHrFields(hrTypeId, hrArea.HrAreaId, false);

                    await AddHrBillBase(hrTypeId, billInfo.FId, billInfo, generateTypeLastValues, tableName, hrAreaData, hrAreaFields, hrAreaData);

                }

                for (int i = 0; i < hrAreaReferences.Count; i++)
                {
                    var hrArea = hrAreaReferences[i];
                    var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);
                    await CreateFistRowReferenceData(billInfo.FId, null, tableName);
                }

                await @trans.CommitAsync();

                await _hrDataActivityLog.LogBuilder(() => HrBillActivityLogMessage.Create)
                        .MessageResourceFormatDatas(hrTypeInfo.Title, billInfo.FId)
                        .BillTypeId(hrTypeId)
                        .ObjectId(billInfo.FId)
                        .JsonData(data.JsonSerialize())
                        .CreateLog();

                await ConfirmCustomGenCode(generateTypeLastValues);

                return billInfo.FId;
            }
            catch (System.Exception)
            {
                await @trans.TryRollbackTransactionAsync();
                _logger.LogError("HrDataService: CreateHr");
                throw;
            }
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping(int hrTypeId, int? areaId)
        {
            var hrType = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);

            var fields = await GetHrFields(hrTypeId, null, false);
            fields = fields.Where(f => f.FormTypeId != EnumFormType.ImportFile).ToList();
            var importFacade = new HrDataImportFacade(hrType, fields, _hrDataImportDIService);

            return await importFacade.GetFieldDataForMapping();
        }

        public async Task<bool> ImportHrBillFromMapping(int hrTypeId, ImportExcelMapping mapping, Stream stream)
        {
            var hrType = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);

            var fields = await GetHrFields(hrTypeId, null, false);
            fields = fields.Where(f => f.FormTypeId != EnumFormType.ImportFile).ToList();
            var importFacade = new HrDataImportFacade(hrType, fields, _hrDataImportDIService);

            return await importFacade.ImportHrBillFromMapping(mapping, stream);


        }
        #endregion

        #region private

        private string AreaRowIdField(string hrAreaCode)
        {
            return $"{hrAreaCode}_F_Id";
        }

        private async Task<List<HrValidateField>> GetHrFields(int hrTypeId, int? areaId, bool includeSelectSqlField)
        {
            var area = _organizationDBContext.HrArea.AsQueryable();
            if (areaId > 0)
            {
                area = area.Where(a => a.HrAreaId == areaId);
            }
            return await (from af in _organizationDBContext.HrAreaField
                          join f in _organizationDBContext.HrField on af.HrFieldId equals f.HrFieldId
                          join a in area on af.HrAreaId equals a.HrAreaId
                          where af.HrTypeId == hrTypeId && f.FormTypeId != (int)EnumFormType.ViewOnly
                          && (includeSelectSqlField || f.FormTypeId != (int)EnumFormType.SqlSelect)
                          && a.HrTypeReferenceId.HasValue == false //&& f.FieldName != OrganizationConstants.F_IDENTITY
                          orderby a.SortOrder, af.SortOrder
                          select new HrValidateField
                          {
                              HrAreaFieldId = af.HrAreaFieldId,
                              Title = af.Title,
                              IsAutoIncrement = af.IsAutoIncrement,
                              IsHidden = af.IsHidden,
                              IsRequire = af.IsRequire,
                              IsUnique = af.IsUnique,
                              Filters = af.Filters,
                              FieldName = f.FieldName,
                              DataTypeId = (EnumDataType)f.DataTypeId,
                              FormTypeId = (EnumFormType)f.FormTypeId,
                              SqlValue = f.SqlValue,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              RegularExpression = af.RegularExpression,
                              IsMultiRow = a.IsMultiRow,
                              RequireFilters = af.RequireFilters,
                              HrAreaTitle = a.Title,
                              HrAreaId = a.HrAreaId,
                              HrAreaCode = a.HrAreaCode
                          }).ToListAsync();
        }

        private (string query, IList<string> columns) GetAliasViewAreaTable(string hrTypeCode, string hrAreaCode, IEnumerable<HrValidateField> fields, bool isMultiRow = false)
        {
            var tableName = GetHrAreaTableName(hrTypeCode, hrAreaCode);
            var @selectColumn = @$"
                row.F_Id [{AreaRowIdField(hrAreaCode)}],
                row.HrBill_F_Id,
                row.HrBillReference_F_Id,
                bill.HrTypeId
            ";
            var @join = @$"
                FROM {tableName} AS row WITH(NOLOCK)
                JOIN HrBill AS bill WITH(NOLOCK) ON row.HrBill_F_Id = bill.F_Id
            ";

            var @columns = new List<string>
            {
                AreaRowIdField(hrAreaCode)
            };

            foreach (var field in fields)
            {
                if (field.FormTypeId == EnumFormType.SqlSelect)
                {
                    @selectColumn += $", {field.SqlValue} AS [{field.FieldName}]";
                }
                else
                {
                    @selectColumn += $", [row].[{field.FieldName}]";
                }

                @columns.Add(field.FieldName);

                if (field.HasRefField && !string.IsNullOrWhiteSpace(field.RefTableTitle))
                {
                    @columns.AddRange(field.RefTableTitle.Split(",").Select(refTitle => $"{field.FieldName}_{refTitle}"));
                    if (field.FormTypeId == EnumFormType.MultiSelect)
                    {
                        var refFields = field.RefTableTitle.Split(",").Select(refTitle => @$", 
                            (
                                SELECT STRING_AGG({refTitle}, ', ') AS [{refTitle}]
                                FROM  (
                                    SELECT [row].F_Id, [{refTitle}] 
                                    FROM v{field.RefTableCode} 
                                    WHERE [v{field.RefTableCode}].[F_Id] IN (
                                        SELECT [value] FROM OPENJSON(ISNULL([row].[{field.FieldName}],'[]')) WITH (  [value] INT '$' )
                                    )
                                ) c GROUP BY c.F_Id
                            ) AS [{field.FieldName}_{refTitle}]
                        ");
                        @selectColumn += string.Join("", refFields);

                    }
                    else
                    {
                        var refFields = field.RefTableTitle.Split(",").Select(refTitle => $", [v{field.FieldName}].[{refTitle}] AS [{field.FieldName}_{refTitle}]");
                        @selectColumn += string.Join("", refFields);

                        @join += $" LEFT JOIN [v{field.RefTableCode}] as [v{field.FieldName}] WITH(NOLOCK) ON [row].[{field.FieldName}] = [v{field.FieldName}].[{field.RefTableField}]";
                    }
                }
            }

            return ($"SELECT {(isMultiRow ? "" : "TOP 1")} {@selectColumn} {@join} WHERE [row].IsDeleted = 0 AND [row].SubsidiaryId = {_currentContextService.SubsidiaryId}", @columns);
        }

        private void ValidateExistenceHrBill(long hrBill_F_Id)
        {
            if (!_organizationDBContext.HrBill.Any(x => x.FId == hrBill_F_Id))
                throw new BadRequestException(HrErrorCode.HrValueBillNotFound);
        }

        public async Task<HrTypeExecData> GetHrTypExecInfo(string hrTypeCode)
        {
            var hrTypeInfo = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeCode == hrTypeCode);
            return await GetHrTypExecInfo(hrTypeInfo);
        }


        private async Task<HrTypeExecData> GetHrTypExecInfo(int hrTypeId)
        {
            var hrTypeInfo = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);
            return await GetHrTypExecInfo(hrTypeInfo);
        }


        private async Task<HrTypeExecData> GetHrTypExecInfo(HrType hrTypeInfo)
        {
            if (hrTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ hành chính nhân sự");

            var global = await _hrTypeService.GetHrGlobalSetting();

            var info = _mapper.Map<HrTypeExecData>(hrTypeInfo);
            info.GlobalSetting = global;
            return info;
        }

        private string GlobalFilter()
        {
            return $"bill.SubsidiaryId = {_currentContextService.SubsidiaryId} AND bill.IsDeleted = 0";
        }

        private string GetHrAreaTableName(string hrTypeCode, string hrAreaCode) => OrganizationConstants.GetHrAreaTableName(hrTypeCode, hrAreaCode);

        private async Task CreateFistRowReferenceData(long hrBill_F_Id, long? hrBillReference_F_Id, string tableName)
        {
            var sqlParams = new List<SqlParameter>();
            var columns = GetColumnGlobal().ToList();
            columns.Add("HrBill_F_Id");

            sqlParams.AddRange(GetSqlParamsGlobal());
            sqlParams.Add(new SqlParameter("@HrBill_F_Id", hrBill_F_Id));

            if (hrBillReference_F_Id.HasValue)
            {
                columns.Add("HrBillReference_F_Id");
                sqlParams.Add(new SqlParameter("@HrBillReference_F_Id", hrBillReference_F_Id));
            }

            var idParam = new SqlParameter("@F_Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
            sqlParams.Add(idParam);

            var sql = $"INSERT INTO [{tableName}]({string.Join(",", columns.Select(c => $"[{c}]"))}) VALUES({string.Join(",", sqlParams.Where(p => p.ParameterName != "@F_Id").Select(p => $"{p.ParameterName}"))}); SELECT @F_Id = SCOPE_IDENTITY();";
            await _organizationDBContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);

            if (null == idParam.Value)
                throw new InvalidProgramException();
        }

        private async Task<(HrBill, string)> SubDeleteHr(int hrTypeId, long hrBill_F_Id, bool isDeletedHrAreaRef)
        {

            var hrTypeInfo = await GetHrTypExecInfo(hrTypeId);
            var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var billInfo = await _organizationDBContext.HrBill.FirstOrDefaultAsync(b => b.FId == hrBill_F_Id);

            if (billInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy chứng từ hành chính nhân sự");

            var billTopUsed = await GetHrBillTopInUsed(new[] { hrBill_F_Id }, true);
            if (billTopUsed.Count > 0)
            {
                throw HrErrorCode.HrBillInUsed.BadRequestFormatWithData(billTopUsed, $"{HrErrorCode.HrBillInUsed.GetEnumDescription()}. {billTopUsed.First().Description}", hrTypeInfo.Title + " " + billInfo.BillCode);
            }


            for (int i = 0; i < hrAreas.Count; i++)
            {
                var hrArea = hrAreas[i];

                var tableName = GetHrAreaTableName(hrTypeInfo.HrTypeCode, hrArea.HrAreaCode);

                if (hrArea.HrTypeReferenceId.HasValue)
                {
                    var selectSql = $@"SELECT [HrBillReference_F_Id] {OrganizationConstants.HR_TABLE_F_IDENTITY} FROM [{tableName}] WHERE [IsDeleted] = 0 AND [HrBill_F_Id] = @HrBill_F_Id";
                    var dataRef = (await _organizationDBContext.QueryDataTableRaw(selectSql, new[] {
                        new SqlParameter($"@HrBill_F_Id", hrBill_F_Id),
                        })).ConvertData();

                    if (dataRef.Count > 0)
                    {
                        var refFId = dataRef[0].GetValueOrDefault(OrganizationConstants.HR_TABLE_F_IDENTITY, 0);
                        if (!refFId.IsNullOrEmptyObject() && (long)refFId > 0)
                        {
                            await SubDeleteHr(hrArea.HrTypeReferenceId.Value, (long)refFId, false);
                        }

                    }

                }

                var deleteSql = $@"UPDATE [{tableName}] SET [UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc, [DeletedDatetimeUtc] = @DeletedDatetimeUtc, [IsDeleted] = @IsDeleted WHERE [HrBill_F_Id] = @HrBill_F_Id";
                var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(deleteSql, new[] {
                        new SqlParameter($"@HrBill_F_Id", hrBill_F_Id),
                        new SqlParameter($"@DeletedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedByUserId", _currentContextService.UserId),
                        new SqlParameter($"@IsDeleted", true),
                        });

            }

            if (isDeletedHrAreaRef)
            {
                var hrAreasRef = await _organizationDBContext.HrArea.Where(x => x.HrTypeReferenceId == hrTypeId).AsNoTracking().ToListAsync();

                for (int i = 0; i < hrAreasRef.Count; i++)
                {
                    var hrArea = hrAreasRef[i];
                    var hrType = await GetHrTypExecInfo(hrArea.HrTypeId);

                    var tableName = GetHrAreaTableName(hrType.HrTypeCode, hrArea.HrAreaCode);

                    var deleteSql = $@"UPDATE [{tableName}] SET [UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc, [DeletedDatetimeUtc] = @DeletedDatetimeUtc, [IsDeleted] = @IsDeleted WHERE [HrBillReference_F_Id] = @HrBill_F_Id";
                    var _ = await _organizationDBContext.Database.ExecuteSqlRawAsync(deleteSql, new[] {
                        new SqlParameter($"@HrBill_F_Id", hrBill_F_Id),
                        new SqlParameter($"@DeletedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedDatetimeUtc", DateTime.UtcNow),
                        new SqlParameter($"@UpdatedByUserId", _currentContextService.UserId),
                        new SqlParameter($"@IsDeleted", true),
                        });
                }
            }

            billInfo.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            return (billInfo, hrTypeInfo.Title);
        }

        public async Task<IList<HrBillInUsedInfo>> GetHrBillTopInUsed(IList<long> fIds, bool isCheckExistOnly)
        {
            var checkParams = new[]
            {
                fIds.ToSqlParameter("@FIds"),
                new SqlParameter("@IsCheckExistOnly", SqlDbType.Bit){ Value  = isCheckExistOnly }
            };
            return await _organizationDBContext.QueryListProc<HrBillInUsedInfo>("asp_HrBill_GetTopUsed_ByList", checkParams);
        }

        #endregion

        #region protected class



        public class HrValidateField
        {
            public int HrAreaFieldId { get; set; }
            public string Title { get; set; }
            public bool IsAutoIncrement { get; set; }
            public bool IsHidden { get; set; }
            public bool IsRequire { get; set; }
            public bool IsUnique { get; set; }
            public string Filters { get; set; }
            public string FieldName { get; set; }
            public EnumDataType DataTypeId { get; set; }
            public EnumFormType FormTypeId { get; set; }
            public string SqlValue { get; set; }
            public int DataSize { get; set; }
            public string RefTableCode { get; set; }
            public string RefTableField { get; set; }
            public string RefTableTitle { get; set; }
            public string RegularExpression { get; set; }
            public bool IsMultiRow { get; set; }
            public string RequireFilters { get; set; }
            public string HrAreaCode { get; internal set; }
            public int HrAreaId { get; internal set; }
            public string HrAreaTitle { get; internal set; }


            private bool? _hasRefField;
            private string _fieldNameRefTitle;
            private IList<string> _fieldNameRefTitles;



            public bool HasRefField
            {
                get
                {
                    if (_hasRefField.HasValue) return _hasRefField.Value;
                    _hasRefField = ((FormTypeId).IsJoinForm() || FormTypeId == EnumFormType.MultiSelect);
                    return _hasRefField.Value;
                }
            }

            public string FieldNameRefTitle
            {
                get
                {
                    if (_fieldNameRefTitle != null) return _fieldNameRefTitle;

                    SetFieldNameRefTitles();

                    return _fieldNameRefTitle;
                }
            }

            public IList<string> FieldNameRefTitles
            {
                get
                {
                    if (_fieldNameRefTitles != null) return _fieldNameRefTitles;

                    SetFieldNameRefTitles();

                    return _fieldNameRefTitles;
                }
            }

            private void SetFieldNameRefTitles()
            {
                if (!HasRefField || string.IsNullOrWhiteSpace(RefTableTitle))
                {
                    _fieldNameRefTitle = FieldName;
                    _fieldNameRefTitles = new[] { _fieldNameRefTitle };
                }
                else
                {
                    var fields = RefTableTitle.Split(',').ToArray();
                    _fieldNameRefTitles = new List<string>();
                    for (var i = 0; i < fields.Length; i++)
                    {
                        var name = $"{FieldName}_{fields[i].Trim()}";
                        _fieldNameRefTitles.Add(name);
                        if (i == 0)
                        {
                            _fieldNameRefTitle = name;
                        }
                    }

                }
            }


        }

        protected class HrAreaTableBaseInfo
        {
            public long F_Id { get; set; }
            public long HrBill_F_Id { get; set; }
        }
        #endregion

    }

}