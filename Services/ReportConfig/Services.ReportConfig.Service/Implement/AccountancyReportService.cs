using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.Model;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class AccountancyReportService : IAccountancyReportService
    {
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ReportConfigDBContext _reportConfigDBContext;
        private readonly IReportConfigService _reportConfigService;

        public AccountancyReportService(
            AccountancyDBContext accountancyDBContext,
            ReportConfigDBContext reportConfigDBContext,
            IReportConfigService reportConfigService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _reportConfigDBContext = reportConfigDBContext;
            _reportConfigService = reportConfigService;
        }



        public async Task<ReportDataModel> Report(int reportId, ReportFilterModel model)
        {
            var result = new ReportDataModel();

            var filters = model.Filters;
            var orderByFieldName = model.OrderByFieldName;
            var asc = model.Asc;
            var page = model.Page;
            var size = model.Size;

            var reportInfo = await _reportConfigDBContext.ReportType.AsNoTracking().FirstOrDefaultAsync(r => r.ReportTypeId == reportId);

            if (reportInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            var reportViewInfo = await _reportConfigService.ReportTypeViewGetInfo(reportInfo.ReportTypeId);

            var sqlParams = new List<SqlParameter>();

            foreach (var filterFiled in reportViewInfo.Fields)
            {
                object value = null;
                if (filters.ContainsKey(filterFiled.ParamerterName))
                {
                    value = filters[filterFiled.ParamerterName];
                    if (!value.IsNullObject())
                    {
                        if (filterFiled.DataTypeId == EnumDataType.Date)
                        {
                            value = Convert.ToInt64(value);
                        }
                    }
                }
                sqlParams.Add(new SqlParameter($"@{filterFiled.ParamerterName}", filterFiled.DataTypeId.GetSqlValue(value)));
            }


            if (!string.IsNullOrWhiteSpace(reportInfo.HeadSql))
            {
                var data = await _accountancyDBContext.QueryDataTable(reportInfo.HeadSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());
                result.Head = data.ConvertFirstRowData();
                foreach (var head in result.Head)
                {
                    sqlParams.Add(new SqlParameter($"@{AccountantConstants.REPORT_HEAD_PARAM_PREFIX}" + head.Key, head.Value == null ? DBNull.Value : head.Value));
                }
            }


            if (reportInfo.IsBsc)
            {
                var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();

                if (bscConfig != null)
                {
                    var (data, totals) = await GetRowsByBsc(reportInfo, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(reportInfo.BodySql))
                {
                    var (data, totals) = await GetRowsByView(reportInfo, orderByFieldName, asc, page, size, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
                else
                {
                    var (data, totals) = await GetRowsByQuery(reportInfo, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
            }


            if (!string.IsNullOrWhiteSpace(reportInfo.FooterSql))
            {
                var data = await _accountancyDBContext.QueryDataTable(reportInfo.FooterSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());
                result.Head = data.ConvertFirstRowData();
            }

            return result;
        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByBsc(ReportType reportInfo, IList<SqlParameter> sqlParams)
        {
            var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();
            if (bscConfig == null) return (null, null);

            var bscRows = new List<NonCamelCaseDictionary>();
            foreach (var row in bscConfig.Rows)
            {
                var rowValue = new NonCamelCaseDictionary();

                var keyValue = "";
                foreach (var column in bscConfig.BscColumns)
                {

                    var valueConfig = row.Value.ContainsKey(column.Name) ? row.Value[column.Name] : null;
                    object value = null;
                    if (valueConfig?.ToString()?.StartsWith("=") == true)
                    {
                        valueConfig = valueConfig.ToString().TrimStart('=');
                        var data = await _accountancyDBContext.QueryDataTable($"SELECT {valueConfig} AS {column.Name}", sqlParams.Select(p => p.CloneSqlParam()).ToArray());
                        value = data.ConvertFirstRowData()[column.Name];
                    }
                    else
                    {
                        value = valueConfig;

                    }
                    if (column.IsRowKey)
                    {
                        keyValue = value?.ToString()?.NormalizeAsInternalName();
                    }

                    if (!string.IsNullOrWhiteSpace(keyValue))
                    {
                        sqlParams.Add(new SqlParameter($"@{AccountantConstants.REPORT_BSC_ROW_PARAM_PREFIX}{keyValue}_{column.Name}", value == null ? DBNull.Value : value));
                    }

                    rowValue[column.Name] = value;
                }

                bscRows.Add(rowValue);
            }

            var totals = new NonCamelCaseDictionary();

            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            var calSumColumns = columns.Where(c => c.IsCalcSum);
            foreach (var column in calSumColumns)
            {
                totals.Add(column.Alias, 0M);
            }

            for (var i = 0; i < bscRows.Count; i++)
            {
                var row = bscRows[i];

                if (row != null)
                {
                    foreach (var column in calSumColumns)
                    {
                        var colData = row[column.Alias];
                        if (colData != null)
                        {
                            totals[column.Alias] = (decimal)totals[column.Alias] + Convert.ToDecimal(colData);
                        }
                    }

                }

            }

            return (new PageDataTable()
            {
                List = bscRows,
                Total = bscRows.Count
            }, totals);

        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByQuery(ReportType reportInfo, IList<SqlParameter> sqlParams)
        {
            var data = await _accountancyDBContext.QueryDataTable(reportInfo.BodySql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            var totals = new NonCamelCaseDictionary();

            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            var calSumColumns = columns.Where(c => c.IsCalcSum);
            foreach (var column in calSumColumns)
            {
                totals.Add(column.Alias, 0M);
            }

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];

                if (row != null)
                {
                    foreach (var column in calSumColumns)
                    {
                        var colData = row[column.Alias];
                        if (colData != null)
                        {
                            totals[column.Alias] = (decimal)totals[column.Alias] + Convert.ToDecimal(colData);
                        }
                    }

                }

            }

            return ((data, data.Rows.Count), totals);

        }
        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByView(ReportType reportInfo, string orderByFieldName, bool asc, int page, int size, IList<SqlParameter> sqlParams)
        {
            var totals = new NonCamelCaseDictionary();
            if (string.IsNullOrWhiteSpace(reportInfo.MainView))
            {
                reportInfo.MainView = "_tk";
            }

            var viewSql = new StringBuilder();
            //sql.AppendLine("FROM");
            viewSql.AppendLine($"{reportInfo.MainView}");
            viewSql.AppendLine($"{reportInfo.Joins}");

            if (!string.IsNullOrWhiteSpace(reportInfo.Wheres))
            {
                viewSql.AppendLine("WHERE");
                viewSql.AppendLine($"{reportInfo.Wheres}");
            }

            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            foreach (var column in columns)
            {
                if (string.IsNullOrWhiteSpace(column.Alias))
                {
                    column.Alias = column.Name;
                }
            }

            var selectColumn = $"{string.Join(",", columns.Select(c => $"{c.Value} AS {c.Alias}"))}";

            var view = $"(SELECT {selectColumn} FROM {viewSql}) AS v";

            var whereColumn = new List<string>();
            foreach (var column in columns.Where(c => !string.IsNullOrWhiteSpace(c.Where)))
            {
                whereColumn.Add($"{column.Alias} {column.Where}");
            }

            if (whereColumn.Count > 0)
            {
                view += " WHERE " + string.Join(",", whereColumn);
            }

            var totalSql = new StringBuilder();

            totalSql.Append("SELECT COUNT(0) AS Total");

            foreach (var column in columns.Where(c => c.IsCalcSum))
            {
                totalSql.Append($", SUM({column.Alias}) AS {column.Alias}");
            }
            totalSql.Append($" FROM {view}");

            var table = await _accountancyDBContext.QueryDataTable(totalSql.ToString(), sqlParams.ToArray());

            var totalRows = 0;
            if (table != null && table.Rows.Count > 0)
            {
                totalRows = (table.Rows[0]["Total"] as int?).GetValueOrDefault();

                foreach (var column in columns.Where(c => c.IsCalcSum))
                {
                    totals.Add(column.Alias, table.Rows[0][column.Alias]);
                }
            }

            var dataSql = @$"                 
                SELECT 
                    *
                FROM {view}
                ORDER BY {(string.IsNullOrWhiteSpace(orderByFieldName) ? "1" : orderByFieldName)} {(asc ? "" : "DESC")}

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
                ";

            var data = await _accountancyDBContext.QueryDataTable(dataSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray());

            return ((data, totalRows), totals);
        }

    }
}
