using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                foreach (var param in filterFiled.ParamerterName.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(param)) continue;

                    var paramName = param.Trim();

                    if (filters.ContainsKey(paramName))
                    {
                        value = filters[paramName];
                        if (!value.IsNullObject())
                        {
                            if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year, EnumDataType.DateRange }.Contains(filterFiled.DataTypeId))
                            {
                                value = Convert.ToInt64(value);
                            }
                        }
                    }
                    sqlParams.Add(new SqlParameter($"@{paramName}", filterFiled.DataTypeId.GetSqlValue(value)));
                }
            }

            if (!string.IsNullOrWhiteSpace(reportInfo.HeadSql))
            {
                var data = await _accountancyDBContext.QueryDataTable(reportInfo.HeadSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                result.Head = data.ConvertFirstRowData().ToNonCamelCaseDictionary();
                foreach (var head in result.Head)
                {
                    sqlParams.Add(new SqlParameter($"@{AccountantConstants.REPORT_HEAD_PARAM_PREFIX}" + head.Key, head.Value ?? DBNull.Value));
                }
            }

            var suffix = 0;
            var filterCondition = new StringBuilder();
            if (model.ColumnsFilters != null)
            {
                model.ColumnsFilters.FilterClauseProcess(string.Empty, "v", ref filterCondition, ref sqlParams, ref suffix);
            }

            if (reportInfo.IsBsc)
            {
                var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();

                if (bscConfig != null)
                {
                    var (data, totals) = await GetRowsByBsc(reportInfo, orderByFieldName, filterCondition.ToString(), asc, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(reportInfo.BodySql))
                {
                    var (data, totals) = await GetRowsByView(reportInfo, orderByFieldName, filterCondition.ToString(), asc, page, size, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
                else
                {
                    var (data, totals) = await GetRowsByQuery(reportInfo, orderByFieldName, filterCondition.ToString(), asc, page, size, sqlParams.Select(p => p.CloneSqlParam()).ToList());
                    result.Totals = totals;
                    result.Rows = data;
                }
            }


            if (!string.IsNullOrWhiteSpace(reportInfo.FooterSql))
            {
                var data = await _accountancyDBContext.QueryDataTable(reportInfo.FooterSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                result.Head = data.ConvertFirstRowData().ToNonCamelCaseDictionary();
            }

            return result;
        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByBsc(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, IList<SqlParameter> sqlParams)
        {
            var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();
            if (bscConfig == null) return (null, null);

            IList<NonCamelCaseDictionary> bscRows = new List<NonCamelCaseDictionary>();

            var keyValueRows = new Dictionary<string, string>[bscConfig.Rows.Count];

            //1. Query body sql
            var sql = new StringBuilder();
            var sqlBscCalcQuery = new List<BscValueOrder>();

            var queryResult = new NonCamelCaseDictionary();

            for (var i = 0; i < bscConfig.Rows.Count; i++)
            {
                var rowValue = new NonCamelCaseDictionary();
                bscRows.Add(rowValue);
                keyValueRows[i] = new Dictionary<string, string>();

                var row = bscConfig.Rows[i];

                if (row.RowData == null && row.Value != null)
                {
                    row.RowData = row.Value.ToNonCamelCaseDictionaryData(v => v.Key, v => new BscCellModel() { Value = v.Value, Style = new NonCamelCaseDictionary() });
                }

                foreach (var column in bscConfig.BscColumns)
                {
                    var valueConfig = row.RowData.ContainsKey(column.Name) ? row.RowData[column.Name]?.Value : null;
                    var keyValue = string.Empty;
                    var configStr = (valueConfig?.ToString()?.Trim()) ?? "";
                    if (configStr.StartsWith("["))
                    {
                        var endKeyIndex = configStr.IndexOf(']');
                        keyValue = configStr.Substring(1, endKeyIndex - 1);

                        keyValueRows[i].Add(column.Name, keyValue);

                        configStr = configStr.Substring(endKeyIndex + 1).Trim();
                    }
                    else
                    {
                        if (configStr.StartsWith("\\["))
                        {
                            configStr = configStr.TrimStart('\\');
                        }
                    }


                    int sortValue = 0;
                    if (configStr.StartsWith("|"))
                    {
                        var endKeyIndex = configStr.IndexOf('=');

                        if (endKeyIndex <= 0)
                        {
                            endKeyIndex = 2;
                        }

                        sortValue = int.Parse(configStr.Substring(1, endKeyIndex - 1).Trim());

                        configStr = configStr.Substring(endKeyIndex).Trim();
                    }
                    else
                    {
                        if (configStr.StartsWith("\\|"))
                        {
                            configStr = configStr.TrimStart('|');
                        }
                    }

                    rowValue.TryAdd(column.Name, configStr);

                    if (keyValueRows[i].ContainsKey(column.Name) && !string.IsNullOrWhiteSpace(keyValueRows[i][column.Name]))
                    {
                        rowValue.TryAdd(column.Name + "_key", keyValueRows[i][column.Name]);
                    }
                    else
                    {
                        rowValue.TryAdd(column.Name + "_key", null);
                    }

                    if (BscRowsModel.IsSqlSelect(configStr))
                    {
                        var selectData = $"{configStr.TrimStart('=')} AS [{column.Name}_{i}]";
                        if (BscRowsModel.IsBscSelect(configStr))
                        {
                            sqlBscCalcQuery.Add(new BscValueOrder()
                            {
                                SortOrder = sortValue,
                                SelectData = selectData,
                                KeyValue = keyValue
                            });

                            //BscAppendSelect(sqlBscSelect, selectData);
                        }
                        else
                        {
                            BscAppendSelect(sql, selectData);
                        }
                    }
                }
            }

            Dictionary<string, (object value, Type type)> selectValue = null;

            if (sql.Length > 0)
            {
                var data = await _accountancyDBContext.QueryDataTable($"{reportInfo.BodySql}\n {sql} ", sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                selectValue = data.ConvertFirstRowData();
                BscSetValue(bscRows, selectValue, keyValueRows, sqlParams);
            }


            if (sqlBscCalcQuery.Count > 0)
            {
                //var data = await _accountancyDBContext.QueryDataTable($"{sqlBscSelect}", sqlParams.Select(p => p.CloneSqlParam()).ToArray());
                //selectValue = data.ConvertFirstRowData();
                //BscSetValue(bscRows, selectValue, keyValueRows, sqlParams);

                var cacls = sqlBscCalcQuery.OrderBy(s => s.SortOrder).ToList();
                // Xử lý BSC_VALUE chứa BSC_VALUE
                foreach (var item in cacls)
                {
                    item.SelectData = GetBscSelectData(cacls, item.SelectData, item.KeyValue);
                }
                foreach (var item in cacls)
                {
                    var data = await _accountancyDBContext.QueryDataTable($"SELECT {item.SelectData}", sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                    selectValue = data.ConvertFirstRowData();
                    BscSetValue(bscRows, selectValue, keyValueRows, sqlParams);
                }
            }

            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            bscRows = await CastBscAlias(reportInfo, filterCondition, columns, bscRows, sqlParams, orderByFieldName, asc);

            //Totals
            var totals = new NonCamelCaseDictionary();

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
                        if (!colData.IsNullObject())
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


        private string GetBscSelectData(List<BscValueOrder> cacls, string selectData, string keyValue, string parentKeyValue = null)
        {
            var result = new StringBuilder(selectData);
            var pattern = $"@{AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX}(?<key_value>\\w+)";
            Regex rx = new Regex(pattern);
            var match = rx.Matches(selectData);//.Select(m => m.Groups["key_value"].Value).Distinct().ToList();
            var moveIndex = 0;
            for (int i = 0; i < match.Count; i++)
            {
                var key = match[i].Groups["key_value"].Value;
                if (key == keyValue) throw new BadRequestException(GeneralCode.InternalError, "Cấu hình lỗi do có dòng dữ liệu BSC bằng chính nó");
                if (key == parentKeyValue) throw new BadRequestException(GeneralCode.InternalError, "Cấu hình lỗi do có vòng lặp giá trị BSC");
                var element = cacls.FirstOrDefault(e => e.KeyValue == key);
                if (element != null)
                {
                    var newText = GetBscSelectData(cacls, element.SelectData, element.KeyValue, keyValue);
                    newText = $"({newText.Substring(0, newText.IndexOf("AS"))})";
                    result.Remove(match[i].Index + moveIndex, match[i].Length);
                    result.Insert(match[i].Index + moveIndex, newText);
                    moveIndex = moveIndex + newText.Length - match[i].Length;
                }
            }
            return result.ToString();
        }


        private void BscAppendSelect(StringBuilder selectBuilder, string selectColumn)
        {
            if (selectBuilder.Length > 0)
            {
                selectBuilder.Append(",");
            }
            else
            {
                selectBuilder.AppendLine("SELECT");
            }

            selectBuilder.AppendLine(selectColumn);
        }

        private void BscSetValue(IList<NonCamelCaseDictionary> bscRows, Dictionary<string, (object value, Type type)> selectValue, Dictionary<string, string>[] keyvalueRows, IList<SqlParameter> sqlParams)
        {
            for (var i = 0; i < bscRows.Count; i++)
            {
                var row = bscRows[i];
                var keys = row.Keys.ToList();
                foreach (var col in keys)
                {
                    var fieldName = $"{col}_{i}";
                    if (selectValue.ContainsKey(fieldName))
                    {
                        var value = selectValue[fieldName].value;
                        var type = selectValue[fieldName].type;

                        row[col] = value;

                        if (!BscRowsModel.IsSqlSelect(value))
                        {
                            var rowKeys = keyvalueRows[i];
                            if (rowKeys.ContainsKey(col))
                            {
                                var keyValue = rowKeys[col];
                                var paramName = $"@{AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX}{keyValue}";
                                if (!string.IsNullOrWhiteSpace(keyValue) && !sqlParams.Any(p => p.ParameterName == paramName))
                                {
                                    sqlParams.Add(new SqlParameter(paramName, type.ConvertToDbType()) { Value = value ?? DBNull.Value });
                                }
                            }
                        }
                    }
                }
            }

        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByQuery(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, int page, int size, IList<SqlParameter> sqlParams)
        {
            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            var selectAliasSql = SelectAsAlias(columns.Where(c => !c.IsGroup).ToDictionary(c => c.Alias, c => string.IsNullOrWhiteSpace(c.Where) ? c.Value : $"CASE WHEN {c.Value} {c.Where} THEN {c.Value} ELSE NULL END"));

            selectAliasSql = $"SELECT {selectAliasSql} FROM ({reportInfo.BodySql}) AS v1";

            if (!string.IsNullOrEmpty(filterCondition))
                selectAliasSql = $"SELECT * FROM ({selectAliasSql}) AS v WHERE {filterCondition}";

            string orderBy = reportInfo?.OrderBy;

            if (string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(reportInfo.OrderBy))
            {
                orderBy = reportInfo.OrderBy;
            }

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
                orderBy = $"{orderByFieldName}" + (asc ? "" : " DESC");
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                selectAliasSql += " ORDER BY " + orderBy;
            }

            //var whereColumn = new List<string>();
            //foreach (var column in columns.Where(c => !string.IsNullOrWhiteSpace(c.Where)))
            //{
            //    whereColumn.Add($"{column.Alias} {column.Where}");
            //}

            //if (whereColumn.Count > 0)
            //{
            //    selectAliasSql += " WHERE " + string.Join(",", whereColumn);
            //}

            var table = await _accountancyDBContext.QueryDataTable(selectAliasSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            var totals = new NonCamelCaseDictionary();

            var data = table.ConvertData();

            var calSumColumns = columns.Where(c => c.IsCalcSum);
            foreach (var column in calSumColumns)
            {
                totals.Add(column.Alias, 0M);
            }

            for (var i = 0; i < data.Count; i++)
            {
                var row = data[i];

                if (row != null)
                {
                    foreach (var column in calSumColumns)
                    {
                        var colData = row[column.Alias];
                        if (!colData.IsNullObject())
                        {
                            totals[column.Alias] = (decimal)totals[column.Alias] + Convert.ToDecimal(colData);
                        }
                    }

                }

            }

            if (!asc)
            {
                data.Reverse();
            }

            var pagedData = data.Skip((page - 1) * size).Take(size).ToList();

            return (new PageDataTable() { List = pagedData, Total = data.Count }, totals);

        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByView(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, int page, int size, IList<SqlParameter> sqlParams)
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

            var view = $"(SELECT {SelectAsAlias(columns.Where(c => !c.IsGroup).ToDictionary(c => c.Alias, c => string.IsNullOrWhiteSpace(c.Where) ? c.Value : $"CASE WHEN {c.Value} {c.Where} THEN {c.Value} ELSE NULL END"))} FROM {viewSql}) as v";

            //var whereColumn = new List<string>();
            //foreach (var column in columns.Where(c => !string.IsNullOrWhiteSpace(c.Where)))
            //{
            //    whereColumn.Add($"{column.Alias} {column.Where}");
            //}

            //if (whereColumn.Count > 0)
            //{
            //    view += " WHERE " + string.Join(",", whereColumn);
            //}

            var totalSql = new StringBuilder();

            totalSql.Append("SELECT COUNT(0) AS Total");

            foreach (var column in columns.Where(c => c.IsCalcSum))
            {
                totalSql.Append($", SUM({column.Alias}) AS {column.Alias}");
            }
            totalSql.Append($" FROM {view}");
            if (!string.IsNullOrEmpty(filterCondition))
                totalSql.Append($" WHERE {filterCondition}");

            var table = await _accountancyDBContext.QueryDataTable(totalSql.ToString(), sqlParams.ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            var totalRows = 0;
            if (table != null && table.Rows.Count > 0)
            {
                totalRows = (table.Rows[0]["Total"] as int?).GetValueOrDefault();

                foreach (var column in columns.Where(c => c.IsCalcSum))
                {
                    totals.Add(column.Alias, table.Rows[0][column.Alias]);
                }
            }

            string orderBy = "";

            if (string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(reportInfo.OrderBy))
            {
                orderBy = reportInfo.OrderBy;
            }

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
                orderBy = $"{orderByFieldName}" + (asc ? "" : " DESC");
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "1";
            }

            var dataSql = new StringBuilder(@$"                 
                SELECT 
                    *
                FROM {view}
                ");
            if (!string.IsNullOrEmpty(filterCondition))
                dataSql.Append($"WHERE {filterCondition}");
            dataSql.Append(@$"
                ORDER BY {orderBy}

                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
                ");

            var data = await _accountancyDBContext.QueryDataTable(dataSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            return ((data, totalRows), totals);
        }


        private async Task<IList<NonCamelCaseDictionary>> CastBscAlias(ReportType reportInfo, string filterCondition, ReportColumnModel[] columns, IList<NonCamelCaseDictionary> orignalData, IList<SqlParameter> sqlParams, string orderByFieldName, bool asc)
        {
            var data = new List<NonCamelCaseDictionary>();

            const string staticRowParamPrefix = "@_bsc_row_data";

            foreach (var row in orignalData)
            {
                var rowSql = SelectAsAlias(row.ToDictionary(k => k.Key, k => $"{staticRowParamPrefix}{k.Key}"));

                var rowParams = sqlParams.Select(p => p.CloneSqlParam()).ToList();
                rowParams.AddRange(row.Select(c => new SqlParameter($"{staticRowParamPrefix}{c.Key}", c.Value ?? DBNull.Value)));

                var selectAliasSql = SelectAsAlias(columns.ToDictionary(c => c.Alias, c => string.IsNullOrWhiteSpace(c.Where) ? c.Value : $"CASE WHEN {c.Value} {c.Where} THEN {c.Value} ELSE NULL END"));

                selectAliasSql = $"SELECT * FROM (SELECT {selectAliasSql} FROM (SELECT {rowSql}) AS v1) AS v";
                if (!string.IsNullOrEmpty(filterCondition)) selectAliasSql += $" WHERE {filterCondition}";
                string orderBy = reportInfo?.OrderBy;

                if (string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(reportInfo.OrderBy))
                {
                    orderBy = reportInfo.OrderBy;
                }

                if (!string.IsNullOrWhiteSpace(orderByFieldName))
                {
                    if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
                    orderBy = $"{orderByFieldName}" + (asc ? "" : " DESC");
                }

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    selectAliasSql += " ORDER BY " + orderBy;
                }

                //var whereColumn = new List<string>();
                //foreach (var column in columns.Where(c => !string.IsNullOrWhiteSpace(c.Where)))
                //{
                //    whereColumn.Add($"{column.Alias} {column.Where}");
                //}

                //if (whereColumn.Count > 0)
                //{
                //    selectAliasSql += " WHERE " + string.Join(",", whereColumn);
                //}

                var rowData = await _accountancyDBContext.QueryDataTable(selectAliasSql, rowParams.ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                if (rowData.Rows.Count > 0)
                    data.Add(rowData.ConvertFirstRowData().ToNonCamelCaseDictionary());
            }

            return data;
        }

        private string SelectAsAlias(Dictionary<string, string> keyValues)
        {
            var selectSql = new StringBuilder();
            var first = true;
            foreach (var (key, value) in keyValues)
            {
                if (!first)
                {
                    selectSql.Append(",");
                }
                selectSql.AppendLine($"{value} AS {key}");
                first = false;
            }

            return selectSql.ToString();
        }

        private class BscValueOrder
        {
            public string SelectData { get; set; }
            public int SortOrder { get; set; }

            public string KeyValue { get; set; }
        }
    }



}
