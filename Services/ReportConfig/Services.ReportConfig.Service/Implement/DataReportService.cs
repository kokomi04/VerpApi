using Microsoft.Data.SqlClient;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class DataReportService : IDataReportService
    {
        private readonly ReportConfigDBContext _reportConfigDBContext;
        private readonly IReportConfigService _reportConfigService;
        private readonly IDocOpenXmlService _docOpenXmlService;
        private readonly AppSetting _appSetting;
        private readonly IPhysicalFileService _physicalFileService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentContextService _currentContextService;

        private readonly Dictionary<EnumModuleType, Type> ModuleDbContextTypes = new Dictionary<EnumModuleType, Type>()
        {
            { EnumModuleType.Accountant,typeof(AccountancyDBPrivateContext) },
            { EnumModuleType.AccountantPublic,typeof(AccountancyDBPublicContext) },
            { EnumModuleType.Master,typeof(MasterDBContext) },
            { EnumModuleType.PurchaseOrder,typeof(PurchaseOrderDBContext) },
            { EnumModuleType.Stock,typeof(StockDBContext) },
            { EnumModuleType.Organization,typeof(OrganizationDBContext) },
            { EnumModuleType.Manufacturing,typeof(ManufacturingDBContext) }
        };

        private readonly Dictionary<EnumModuleType, DbContext> ModuleDbContexts = new Dictionary<EnumModuleType, DbContext>();

        public DataReportService(
            ReportConfigDBContext reportConfigDBContext,
            IReportConfigService reportConfigService,
            IDocOpenXmlService docOpenXmlService,
            IOptions<AppSetting> appSetting,
            IPhysicalFileService physicalFileService,
            IServiceProvider serviceProvider,
            ICurrentContextService currentContextService
            )
        {
            _reportConfigDBContext = reportConfigDBContext;
            _reportConfigService = reportConfigService;
            _docOpenXmlService = docOpenXmlService;
            _appSetting = appSetting.Value;
            _physicalFileService = physicalFileService;
            _serviceProvider = serviceProvider;
            _currentContextService = currentContextService;
        }


        private DbContext GetDbContext(EnumModuleType moduleType)
        {
            if (ModuleDbContexts.ContainsKey(moduleType)) return ModuleDbContexts[moduleType];

            if (!ModuleDbContextTypes.ContainsKey(moduleType))
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy DBContext cho phân hệ {moduleType.GetEnumDescription()}");
            }
            var dbContext = _serviceProvider.GetService(ModuleDbContextTypes[moduleType]) as DbContext;
            ModuleDbContexts.TryAdd(moduleType, dbContext);
            return dbContext;
        }


        public async Task<ReportDataModel> Report(int reportId, ReportFilterDataModel model, int page, int size)
        {
            var result = new ReportDataModel();

            var filters = model.Filters.GroupBy(f => f.Key.Trim().ToLower()).ToDictionary(f => f.Key, f => f.Last().Value);
            var orderByFieldName = model.OrderByFieldName;
            var asc = model.Asc;

            var reportInfo = await _reportConfigDBContext.ReportType.Include(x => x.ReportTypeGroup).AsNoTracking().FirstOrDefaultAsync(r => r.ReportTypeId == reportId);

            if (reportInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");


            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

            var reportViewInfo = await _reportConfigService.ReportTypeViewGetInfo(reportInfo.ReportTypeId);

            var sqlParams = new List<SqlParameter>()
            {
                new SqlParameter("@TimeZoneOffset", _currentContextService.TimeZoneOffset)
            };

            foreach (var filterFiled in reportViewInfo.Fields)
            {
                object value = null;
                foreach (var param in filterFiled.ParamerterName.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(param)) continue;

                    var paramName = param.Trim().ToLower();
                    if (filterFiled.FormTypeId == EnumFormType.MultiSelect)
                    {
                        if (filters.ContainsKey(paramName))
                        {
                            value = filters[paramName];

                        }
                        switch (filterFiled.DataTypeId)
                        {
                            case EnumDataType.Int:
                                sqlParams.Add((!value.IsNullOrEmptyObject() ? ((JArray)value).ToObject<IList<int>>() : Array.Empty<int>()).ToSqlParameter($"@{paramName}"));
                                break;
                            case EnumDataType.BigInt:
                                sqlParams.Add((!value.IsNullOrEmptyObject() ? ((JArray)value).ToObject<IList<long>>() : Array.Empty<long>()).ToSqlParameter($"@{paramName}"));
                                break;
                            case EnumDataType.Text:
                                sqlParams.Add((!value.IsNullOrEmptyObject() ? ((JArray)value).ToObject<IList<string>>() : Array.Empty<string>()).ToSqlParameter($"@{paramName}"));
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (filters.ContainsKey(paramName))
                        {
                            value = filters[paramName];
                            if (!value.IsNullOrEmptyObject())
                            {
                                if (filterFiled.DataTypeId.IsTimeType())
                                {
                                    value = Convert.ToInt64(value);
                                }
                            }
                        }
                        sqlParams.Add(new SqlParameter($"@{paramName}", filterFiled.DataTypeId.GetSqlValue(value)));
                    }
                }
            }

            if (reportInfo.IsDbPaging.HasValue && reportInfo.IsDbPaging.Value)
            {
                sqlParams.Add(new SqlParameter("@Page", page));
                sqlParams.Add(new SqlParameter("@Size", size));
            }

            if (!string.IsNullOrWhiteSpace(reportInfo.HeadSql))
            {
                var data = await _dbContext.QueryMultiDataTable(reportInfo.HeadSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                if (data.Tables.Count > 0) result.Head = data.Tables[0].ConvertFirstRowData().ToNonCamelCaseDictionary();
                if (data.Tables.Count > 1) result.HeadTable = data.Tables[1].ConvertData();

                foreach (var head in result.Head)
                {
                    sqlParams.Add(new SqlParameter($"@{AccountantConstants.REPORT_HEAD_PARAM_PREFIX}" + head.Key, head.Value ?? DBNull.Value));
                }
            }

            var suffix = 0;
            var filterCondition = new StringBuilder();
            if (model.ColumnsFilters != null)
            {
                var viewAlias = string.Empty;
                if (reportInfo.IsBsc || !string.IsNullOrWhiteSpace(reportInfo.BodySql))
                {
                    //viewAlias = "v";
                }
                model.ColumnsFilters.FilterClauseProcess(string.Empty, viewAlias, ref filterCondition, ref sqlParams, ref suffix);
            }

            if (reportInfo.IsBsc)
            {
                var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();
                bscConfig.Rows = bscConfig.Rows.OrderBy(r => r.SortOrder).ToList();

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
                var data = await _dbContext.QueryDataTable(reportInfo.FooterSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                result.Foot = data.ConvertFirstRowData().ToNonCamelCaseDictionary();
            }

            return result;
        }



        private async Task<(PageDataTable data, NonCamelCaseDictionary<decimal> totals)> GetRowsByBsc(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, IList<SqlParameter> sqlParams)
        {
            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

            var bscConfig = reportInfo.BscConfig.JsonDeserialize<BscConfigModel>();
            if (bscConfig == null) return (null, null);

            bscConfig.Rows = bscConfig.Rows.OrderBy(r => r.SortOrder).ToList();

            IList<NonCamelCaseDictionary> bscRows = new List<NonCamelCaseDictionary>();

            var keyValueRows = new Dictionary<string, string>[bscConfig.Rows.Count];

            //1. Query body sql
            var sql = new StringBuilder();
            var sqlBscCalcQuery = new List<BscValueOrder>();

            var queryResult = new NonCamelCaseDictionary();


            var declareValues = new HashSet<string>();
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
                        configStr = ReplaceOldBscValuePrefix(configStr);

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
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(keyValue))
                        {

                            if (!declareValues.Contains(keyValue)) declareValues.Add(keyValue);
                            var selectData = $"@{keyValue} AS [{column.Name}_{i}]";
                            BscAppendSelect(sql, selectData);
                        }

                    }
                }
            }

            Dictionary<string, (object value, Type type)> selectValue = null;

            if (sql.Length > 0)
            {
                var delcareSql = string.Join("\n", declareValues.Select(k => $"DECLARE @{k} DECIMAL(32,12);").ToArray());
                var data = await _dbContext.QueryDataTable($"{delcareSql}\n{reportInfo.BodySql}\n {sql} ", sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
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
                    var data = await _dbContext.QueryDataTable($"SELECT {item.SelectData}", sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
                    selectValue = data.ConvertFirstRowData();
                    BscSetValue(bscRows, selectValue, keyValueRows, sqlParams);
                }
            }

            var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

            bscRows = await CastBscAlias(reportInfo, filterCondition, columns, bscRows, sqlParams, orderByFieldName, asc);

            //Totals
            var totals = new NonCamelCaseDictionary<decimal>();

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
                        if (!colData.IsNullOrEmptyObject() && IsCalcSum(row, column.CalcSumConditionCol))
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

        private string ReplaceOldBscValuePrefix(string selectData)
        {
            selectData = selectData.Replace($"@{AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX_OLD}", "#");
            selectData = selectData.Replace($"#", "@#");
            return selectData;
        }
        private string GetBscSelectData(List<BscValueOrder> cacls, string selectData, string keyValue, string parentKeyValue = null)
        {
            //   selectData = ReplaceOldBscValuePrefix(selectData);

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
                                    sqlParams.Add(new SqlParameter(paramName, type.ConvertToDbType()) { Value = value.IsNullOrEmptyObject() ? 0 : value });
                                }
                            }
                        }
                    }
                }
            }

        }

        private string ReplaceCustom(string sql, string token, string value, string defaultValue = "")
        {
            token = token.Replace("$", "\\$");


            MatchEvaluator replace = (Match match) =>
            {
                var param = match.Groups["param"]?.Value;
                if (!string.IsNullOrWhiteSpace(param))
                {
                    var paramsData = param.Split(':');
                    var defaultValueSetting = paramsData.Length > 1 ? paramsData[1] : null;
                    if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValueSetting))
                    {
                        value = defaultValueSetting;
                    }
                    var preFixIfHaveValue = "";
                    if (paramsData.Length > 2)
                    {
                        preFixIfHaveValue = paramsData[2];
                    }


                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return preFixIfHaveValue + " " + value;
                    }
                    else
                    {
                        return "";
                    }
                }
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            };

            //   \(token(?<param>[^\$\n]*\:[^\$\n]*)\)
            sql = new Regex($"\\({token}(?<param>[^\\$\\n]*\\:[^\\$\\n]*)\\)").Replace(sql, replace);

            return new Regex($"{token}(?<param>\\S*)").Replace(sql, replace);

        }

        private async Task<(PageDataTable data, NonCamelCaseDictionary<decimal> totals)> GetRowsByQuery(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, int page, int size, IList<SqlParameter> sqlParams)
        {
            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

            var sql = reportInfo.BodySql;

            Regex regex;

            regex = new Regex("\\$FILTER(?<param>[\\S\\\\n]*)");

            if (reportInfo.BodySql.Contains("$FILTER"))
            {
                sql = ReplaceCustom(sql, "$FILTER", filterCondition, " 1 = 1 ");
            }
            else
            {
                if (!string.IsNullOrEmpty(filterCondition))
                {
                    sql = sql.TSqlAppendCondition(filterCondition);
                }
            }

            if (reportInfo.BodySql.Contains("$INPUT_PARAMS_DECLARE"))
            {
                var dynamicParamDeclare = string.Join(", ", sqlParams?.Select(p => p.ToDeclareString())?.ToArray());

                sql = ReplaceCustom(sql, "$INPUT_PARAMS_DECLARE", dynamicParamDeclare);

            }

            if (reportInfo.BodySql.Contains("$INPUT_PARAMS_VALUE"))
            {
                var dynamicParam = string.Join(", ", sqlParams?.Select(p => $"{p.ParameterName}")?.ToArray());
                sql = ReplaceCustom(sql, "$INPUT_PARAMS_VALUE", dynamicParam);
            }

            string orderBy = reportInfo?.OrderBy ?? "";

            if (!string.IsNullOrWhiteSpace(orderByFieldName) && !orderBy.Contains(orderByFieldName))
            {
                if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
                orderBy += $"{orderByFieldName}" + (asc ? "" : " DESC");
            }


            if (reportInfo.BodySql.Contains("$ORDERBY"))
            {
                sql = ReplaceCustom(sql, "$ORDERBY", orderBy, " 1 ");
            }
            else if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql += " ORDER BY " + orderBy;
            }

            var table = await _dbContext.QueryDataTable(sql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            var totals = new NonCamelCaseDictionary<decimal>();

            var data = table.ConvertData();

            IList<ReportColumnModel> columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>().OrderBy(col => col.SortOrder).ToList();//.Where(col => !col.IsHidden)
            columns = RepeatColumnUtils.RepeatColumnAndSortProcess(columns, data);



            var totalRecord = 0;
            var pagedData = new List<NonCamelCaseDictionary>();

            var calSumColumns = columns.Where(c => c.IsCalcSum);

            if (!reportInfo.IsDbPaging.HasValue || !reportInfo.IsDbPaging.Value)
            {
                var groupLevel1Alias = columns.Where(c => c.IsGroupRow).Select(c => c.Alias).ToHashSet();
                var isSumByGroup = groupLevel1Alias.Count > 0;

                foreach (var column in calSumColumns)
                {
                    totals.Add(column.Alias, 0M);
                }

                Action<NonCamelCaseDictionary, ReportColumnModel> calcSum = (NonCamelCaseDictionary row, ReportColumnModel column) =>
                {
                    var colData = row[column.Alias];

                    if (!colData.IsNullOrEmptyObject() && IsCalcSum(row, column.CalcSumConditionCol))
                    {
                        var decimalValue = Convert.ToDecimal(colData);

                        totals[column.Alias] += decimalValue;
                    }
                };


                if (isSumByGroup)
                {
                    var groupLevel1 = data.GroupBy(row => string.Join("|", groupLevel1Alias.Select(columnAlias => row[columnAlias])));

                    totalRecord = groupLevel1.Count();

                    var groupLevel2Alias = columns.Where(c => c.IsGroupRowLevel2).Select(c => c.Alias).ToHashSet();

                    foreach (var g1 in groupLevel1)
                    {
                        var groupLevel2 = g1.GroupBy(row => string.Join("|", groupLevel2Alias.Select(columnAlias => row[columnAlias])));
                        foreach (var column in calSumColumns)
                        {
                            if (groupLevel1Alias.Contains(column.Alias))
                            {
                                calcSum(g1.First(), column);
                            }
                            else if (groupLevel2Alias.Contains(column.Alias))
                            {
                                foreach (var g2 in groupLevel2)
                                {
                                    calcSum(g2.First(), column);
                                }
                            }
                            else
                            {
                                foreach (var row in g1)
                                {
                                    calcSum(row, column);
                                }
                            }
                        }
                    }


                    pagedData = groupLevel1.Skip((page - 1) * size).Take(size).SelectMany(g => g).ToList();
                }
                else
                {
                    totalRecord = data.Count;

                    foreach (var row in data)
                    {
                        foreach (var column in calSumColumns)
                        {
                            calcSum(row, column);
                        }
                    }

                    pagedData = data.Skip((page - 1) * size).Take(size).ToList();

                }
            }

            //var total = data.Count;
            if (reportInfo.IsDbPaging.HasValue && reportInfo.IsDbPaging.Value && data.Count > 0 && data[0].ContainsKey("TotalRecord"))
            {
                totalRecord = Convert.ToInt32(data[0]["TotalRecord"]);

                totals = new NonCamelCaseDictionary<decimal>();
                foreach (var column in calSumColumns)
                {
                    var sumColum = $"{column.Alias}_Sum";
                    if (data[0].ContainsKey(sumColum))
                    {
                        totals.Add(column.Alias, Convert.ToDecimal(data[0][sumColum]));
                    }
                    else
                    {
                        throw GeneralCode.NotYetSupported.BadRequest($"Sum columns with paging on database must be including on result data! {sumColum} was not found!");
                    }

                }

            }

            var lst = size > 0 && (!reportInfo.IsDbPaging.HasValue || !reportInfo.IsDbPaging.Value) ? pagedData : data;

            return (new PageDataTable() { List = lst, Total = totalRecord }, totals);
        }

        //class ColumnGroupHasBeenSum : HashSet<string>
        //{

        //}

        //class GroupTokenSum : Dictionary<string, ColumnGroupHasBeenSum>
        //{

        //}

        //private async Task<(PageDataTable data, NonCamelCaseDictionary totals)> GetRowsByQuery(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, int page, int size, IList<SqlParameter> sqlParams)
        //{
        //    var columns = reportInfo.Columns.JsonDeserialize<ReportColumnModel[]>();

        //    var selectAliasSql = SelectAsAlias(columns.Where(c => !c.IsGroup).ToDictionary(c => c.Alias, c => string.IsNullOrWhiteSpace(c.Where) ? c.Value : $"CASE WHEN {c.Value} {c.Where} THEN {c.Value} ELSE NULL END"));

        //    selectAliasSql = $"SELECT {selectAliasSql} FROM ({reportInfo.BodySql}) AS v1";

        //    if (!string.IsNullOrEmpty(filterCondition))
        //        selectAliasSql = $"SELECT * FROM ({selectAliasSql}) AS v WHERE {filterCondition}";

        //    string orderBy = reportInfo?.OrderBy;

        //    if (string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(reportInfo.OrderBy))
        //    {
        //        orderBy = reportInfo.OrderBy;
        //    }

        //    if (!string.IsNullOrWhiteSpace(orderByFieldName))
        //    {
        //        if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
        //        orderBy = $"{orderByFieldName}" + (asc ? "" : " DESC");
        //    }

        //    if (!string.IsNullOrWhiteSpace(orderBy))
        //    {
        //        selectAliasSql += " ORDER BY " + orderBy;
        //    }

        //    //var whereColumn = new List<string>();
        //    //foreach (var column in columns.Where(c => !string.IsNullOrWhiteSpace(c.Where)))
        //    //{
        //    //    whereColumn.Add($"{column.Alias} {column.Where}");
        //    //}

        //    //if (whereColumn.Count > 0)
        //    //{
        //    //    selectAliasSql += " WHERE " + string.Join(",", whereColumn);
        //    //}

        //    var table = await _accountancyDBContext.QueryDataTable(selectAliasSql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

        //    var totals = new NonCamelCaseDictionary();

        //    var data = table.ConvertData();

        //    var calSumColumns = columns.Where(c => c.IsCalcSum);
        //    foreach (var column in calSumColumns)
        //    {
        //        totals.Add(column.Alias, 0M);
        //    }

        //    for (var i = 0; i < data.Count; i++)
        //    {
        //        var row = data[i];

        //        if (row != null)
        //        {
        //            foreach (var column in calSumColumns)
        //            {
        //                var colData = row[column.Alias];
        //                if (!colData.IsNullObject())
        //                {
        //                    totals[column.Alias] = (decimal)totals[column.Alias] + Convert.ToDecimal(colData);
        //                }
        //            }

        //        }

        //    }

        //    if (!asc)
        //    {
        //        data.Reverse();
        //    }

        //    var pagedData = data.Skip((page - 1) * size).Take(size).ToList();

        //    return (new PageDataTable() { List = pagedData, Total = data.Count }, totals);

        //}

        private async Task<(PageDataTable data, NonCamelCaseDictionary<decimal> totals)> GetRowsByView(ReportType reportInfo, string orderByFieldName, string filterCondition, bool asc, int page, int size, IList<SqlParameter> sqlParams)
        {
            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

            var totals = new NonCamelCaseDictionary<decimal>();
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
                if (string.IsNullOrWhiteSpace(column.CalcSumConditionCol))
                {
                    totalSql.Append($", SUM({column.Alias}) AS {column.Alias}");
                }
                else
                {
                    totalSql.Append($", SUM(CASE WHEN {column.CalcSumConditionCol} = 1 THEN {column.Alias} ELSE NULL END) AS {column.Alias}");
                }
            }
            totalSql.Append($" FROM {view}");
            if (!string.IsNullOrEmpty(filterCondition))
                totalSql.Append($" WHERE {filterCondition}");

            var table = await _dbContext.QueryDataTable(totalSql.ToString(), sqlParams.ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            var totalRows = 0;
            if (table != null && table.Rows.Count > 0)
            {
                totalRows = (table.Rows[0]["Total"] as int?).GetValueOrDefault();

                foreach (var column in columns.Where(c => c.IsCalcSum))
                {
                    var v = table.Rows[0][column.Alias].IsNullOrEmptyObject() ? 0 : Convert.ToDecimal(table.Rows[0][column.Alias]);
                    totals.Add(column.Alias, v);
                }
            }

            string orderBy = (reportInfo?.OrderBy ?? "");

            if (!string.IsNullOrWhiteSpace(orderByFieldName) && !orderBy.Contains(orderByFieldName))
            {
                if (!string.IsNullOrWhiteSpace(orderBy)) orderBy += ",";
                orderBy += $"{orderByFieldName}" + (asc ? "" : " DESC");
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
                ");

            if (size > 0)
            {
                dataSql.Append(@$"
                OFFSET {(page - 1) * size} ROWS
                FETCH NEXT {size} ROWS ONLY
                ");
            }

            var data = await _dbContext.QueryDataTable(dataSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            return ((data, totalRows), totals);
        }


        private async Task<IList<NonCamelCaseDictionary>> CastBscAlias(ReportType reportInfo, string filterCondition, ReportColumnModel[] columns, IList<NonCamelCaseDictionary> orignalData, IList<SqlParameter> sqlParams, string orderByFieldName, bool asc)
        {
            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

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

                var rowData = await _dbContext.QueryDataTable(selectAliasSql, rowParams.ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);
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
                selectSql.AppendLine($"{value} AS [{key}]");
                first = false;
            }

            return selectSql.ToString();
        }

        private bool IsCalcSum(NonCamelCaseDictionary row, string CalcSumConditionCol)
        {
            if (string.IsNullOrWhiteSpace(CalcSumConditionCol) || !row.ContainsKey(CalcSumConditionCol)) return true;

            if (row[CalcSumConditionCol] is bool calc)
            {
                return calc;
            }

            if (row[CalcSumConditionCol] is int calcNum)
            {
                return calcNum != 0;
            }

            if (row[CalcSumConditionCol] is long calcNumLong)
            {
                return calcNumLong != 0;
            }

            return !row[CalcSumConditionCol].IsNullOrEmptyObject();
        }

        public async Task<(Stream file, string contentType, string fileName)> GenerateReportAsPdf(int reportId, ReportDataModel reportDataModel)
        {

            var reportInfo = await _reportConfigDBContext.ReportType.AsNoTracking().Include(x => x.ReportTypeGroup).FirstOrDefaultAsync(r => r.ReportTypeId == reportId);

            if (reportInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            var _dbContext = GetDbContext((EnumModuleType)reportInfo.ReportTypeGroup.ModuleTypeId);

            if (!reportInfo.TemplateFileId.HasValue) throw new BadRequestException(FileErrorCode.FileNotFound, "Chưa thiết lập mẫu in cho báo cáo");

            var fileInfo = await _physicalFileService.GetSimpleFileInfo(reportInfo.TemplateFileId.Value);

            if (fileInfo == null) throw new BadRequestException(FileErrorCode.FileNotFound, "Không tìm thấy mẫu in báo cáo");

            try
            {
                var newFile = await _docOpenXmlService.GenerateWordAsPdfFromTemplate(fileInfo, reportDataModel.JsonSerialize(), _dbContext);
                var fileName = GetFileName(reportDataModel.FilterData, reportInfo.ReportTypeName);
                return (newFile, "application/pdf", StringUtils.RemoveDiacritics($"{fileName}.pdf").Replace(" ", "#"));
            }
            catch (Exception ex)
            {
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }

        public async Task<(Stream stream, string fileName, string contentType)> ExportExcel(int reportId, ReportFacadeModel model)
        {
            var accountancyReportExport = new DataReportExcelFacade();
            accountancyReportExport.SetAppSetting(_appSetting);
            accountancyReportExport.SetPhysicalFileService(_physicalFileService);
            accountancyReportExport.SetContextData(_reportConfigDBContext);
            accountancyReportExport.SetCurrentContextService(_currentContextService);
            accountancyReportExport.SetDataReportService(this);
            return await accountancyReportExport.ReportExport(reportId, model);
        }

        private class BscValueOrder
        {
            public string SelectData { get; set; }
            public int SortOrder { get; set; }

            public string KeyValue { get; set; }
        }

        private string GetFileName(ReportFilterDataModel filters, string fileName)
        {
            var fromDate = "";
            var toDate = "";
            foreach (var key in filters.Filters.Keys)
            {
                if (key.ToLower().Contains("fromdate") && !filters.Filters[key].IsNullOrEmptyObject())
                {
                    fromDate = Convert.ToInt64(filters.Filters[key]).UnixToDateTime(_currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy");
                }
                if (key.ToLower().Contains("todate") && !filters.Filters[key].IsNullOrEmptyObject())
                {
                    toDate = Convert.ToInt64(filters.Filters[key]).UnixToDateTime(_currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy");
                }
            }
            if (!"".Equals(fromDate)) fileName = $"{fileName} {fromDate}";
            if (!"".Equals(toDate)) fileName = $"{fileName} {toDate}";
            return fileName;
        }

        
    }



}
