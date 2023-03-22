using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.EF.EFExtensions
{

    public static class SqlDBHelper
    {
        private const string SubIdParam = "@SubId";
        private const string SubsidiaryIdColumn = "SubsidiaryId";

        private static ILogger __logger;
        private static ILogger _logger
        {
            get
            {
                if (__logger != null) return __logger;
                var loggerFactory = Utils.LoggerFactory;
                if (loggerFactory != null)
                {
                    __logger = Utils.LoggerFactory.CreateLogger(typeof(SqlDBHelper));
                    return __logger;
                }
                return Utils.DefaultLoggerFactory.CreateLogger(typeof(SqlDBHelper));
            }
        }

        private static SqlParameter CreateSubSqlParam(this ISubsidiayRequestDbContext requestDbContext)
        {
            return new SqlParameter(SubIdParam, SqlDbType.Int) { Value = requestDbContext.SubsidiaryId };
        }

        public static async Task ExecuteStoreProcedure(this DbContext dbContext, string procedureName, IList<SqlParameter> parameters, bool includeSubId = false)
        {
            var sql = new StringBuilder($"EXEC {procedureName}");
            foreach (var p in parameters)
            {
                if (p.Value.IsNullOrEmptyObject())
                {
                    p.Value = DBNull.Value;
                }

                sql.Append($" {p.ParameterName} = {p.ParameterName}");
                if (p.Direction == ParameterDirection.Output) sql.Append(" OUTPUT");
                sql.Append(",");
            }

            if (includeSubId && dbContext is ISubsidiayRequestDbContext requestDbContext)
            {
                parameters = parameters.Append(requestDbContext.CreateSubSqlParam()).ToArray();

                sql.Append($" {SubIdParam} = {SubIdParam}");
                sql.Append(",");
            }

            dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(3));

            await dbContext.Database.ExecuteSqlRawAsync(sql.ToString().TrimEnd(','), parameters);
        }

        public static async Task<DataTable> ExecuteDataProcedure(this DbContext dbContext, string procedureName, IList<SqlParameter> parameters, TimeSpan? timeout = null)
        {
            return await QueryDataTable(dbContext, procedureName, parameters, CommandType.StoredProcedure, timeout);
        }

        public static async Task<DataSet> ExecuteMultipleDataProcedure(this DbContext dbContext, string procedureName, IList<SqlParameter> parameters, TimeSpan? timeout = null)
        {
            return await QueryMultiDataTable(dbContext, procedureName, parameters, CommandType.StoredProcedure, timeout);
        }

        public static async Task<int> ExecuteNoneQueryProcedure(this DbContext dbContext, string procedureName, IList<SqlParameter> parameters, TimeSpan? timeout = null)
        {
            var ps = new List<SqlParameter>();
            foreach (var param in parameters)
            {
                ps.Add(param);
            }

            if (dbContext is ISubsidiayRequestDbContext requestDbContext)
            {
                ps.Add(requestDbContext.CreateSubSqlParam());
            }

            var sql = new StringBuilder($"EXEC {procedureName}");
            foreach (var param in ps)
            {
                sql.Append($" {param.ParameterName} = {param.ParameterName}");
                if (param.Direction == ParameterDirection.Output) sql.Append(" OUTPUT");
                sql.Append(",");
            }

            if (timeout.HasValue)
            {
                dbContext.Database.SetCommandTimeout(timeout.Value);
            }
            else
            {
                dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(3));
            }

            return await dbContext.Database.ExecuteSqlRawAsync(sql.ToString().TrimEnd(','), ps);
        }

        public static async Task ChangeDatabase(this DbContext dbContext, string dbName)
        {
            var dbConnection = dbContext.Database.GetDbConnection();
            await dbConnection.OpenAsync();
            await dbConnection.ChangeDatabaseAsync(dbName);
        }

        public static async Task<IList<T>> QueryList<T>(this DbContext dbContext, string rawSql, IList<SqlParameter> parameters, CommandType cmdType = CommandType.Text, TimeSpan? timeout = null)
        {
            var dataTable = await QueryDataTable(dbContext, rawSql, parameters, cmdType, timeout);
            return dataTable.ConvertData<T>();
        }

        public static async Task<DataTable> QueryDataTable(this DbContext dbContext, string rawSql, IList<SqlParameter> parameters, CommandType cmdType = CommandType.Text, TimeSpan? timeout = null, ICachingService cachingService = null)
        {
            if (cachingService != null)
            {
                var builder = new StringBuilder();
                builder.AppendLine(rawSql);
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        builder.AppendLine(p.ParameterName);
                        builder.AppendLine(JsonUtils.JsonSerialize(p.Value));
                    }
                }
                builder.AppendLine(cmdType.ToString());
                var key = "QueryDataTable_" + builder.ToString().ToGuid().ToString();
                return await cachingService.TryGetSet("QueryDataTable", key, TimeSpan.FromMinutes(3), async () =>
                {
                    return await QueryDataTableDb(dbContext, rawSql, parameters, cmdType, timeout);
                }, TimeSpan.FromMinutes(3));
            }
            return await QueryDataTableDb(dbContext, rawSql, parameters, cmdType, timeout);
        }

        private static async Task<DataTable> QueryDataTableDb(this DbContext dbContext, string rawSql, IList<SqlParameter> parameters, CommandType cmdType = CommandType.Text, TimeSpan? timeout = null)
        {

            try
            {

                var st = new Stopwatch();
                st.Start();

                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandType = cmdType;
                    command.CommandText = rawSql;
                    command.Parameters.Clear();

                    if (dbContext is ISubsidiayRequestDbContext requestDbContext)
                    {
                        command.Parameters.Add(requestDbContext.CreateSubSqlParam());
                    }
                    command.Parameters.Add(new SqlParameter("@ToDayUtc", DateTime.UtcNow));
                    command.Parameters.Add(new SqlParameter("@ToDay", DateTime.Now));

                    foreach (var param in parameters)
                    {
                        if (param.Value.IsNullOrEmptyObject())
                        {
                            param.Value = DBNull.Value;
                        }

                        if (param.SqlDbType == SqlDbType.VarChar)
                        {
                            param.SqlDbType = SqlDbType.NVarChar;
                        }

                        if (param.SqlDbType == SqlDbType.NVarChar)
                        {
                            param.Size += 4;//for instance add prefix % to compare LIKE operator

                            if (param.Size < 128)
                            {
                                param.Size = 128;
                            }
                        }



                        command.Parameters.Add(param);
                    }


                    if (timeout.HasValue)
                    {
                        command.CommandTimeout = Convert.ToInt32(timeout.Value.TotalSeconds);
                    }
                    else
                    {
                        command.CommandTimeout = 2 * 60;
                    }

                    var trans = dbContext.Database.CurrentTransaction?.GetDbTransaction();
                    if (trans != null)
                    {
                        command.Transaction = trans;
                    }

                    dbContext.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(result);
                        st.Stop();
                        _logger.LogInformation($"Executed DbCommand QueryDataTable ({st.ElapsedMilliseconds}ms) CommandTimeout={command.CommandTimeout}, CommandType = {command.CommandType} [Parametters={string.Join(", ", parameters.Select(p => p.ToDeclareString() + " = N'" + p.Value + "'").ToArray())}], CommandText={command.CommandText}");
                        return dt;
                    }
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error QueryDataTable {ex.Message} \r\nParametters: [{string.Join(",", parameters?.Select(p => p.ToDeclareString() + " = '" + p.Value + "'"))}] {rawSql}", ex);
            }
        }


        public static async Task<DataSet> QueryMultiDataTable(this DbContext dbContext, string rawSql, IList<SqlParameter> parameters, CommandType cmdType = CommandType.Text, TimeSpan? timeout = null)
        {
            try
            {
                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandType = cmdType;
                    command.CommandText = rawSql;
                    command.Parameters.Clear();
                    foreach (var param in parameters)
                    {
                        if (param.Value.IsNullOrEmptyObject())
                        {
                            param.Value = DBNull.Value;
                        }

                        if (param.SqlDbType == SqlDbType.VarChar)
                        {
                            param.SqlDbType = SqlDbType.NVarChar;
                        }

                        if (param.SqlDbType == SqlDbType.NVarChar)
                        {
                            param.Size += 4;//for instance add prefix % to compare LIKE operator

                            if (param.Size < 128)
                            {
                                param.Size = 128;
                            }
                        }

                        command.Parameters.Add(param);
                    }

                    if (dbContext is ISubsidiayRequestDbContext requestDbContext)
                    {
                        command.Parameters.Add(requestDbContext.CreateSubSqlParam());
                    }

                    if (timeout.HasValue)
                    {
                        command.CommandTimeout = Convert.ToInt32(timeout.Value.TotalSeconds);
                    }
                    else
                    {
                        command.CommandTimeout = 2 * 60;
                    }

                    var trans = dbContext.Database.CurrentTransaction?.GetDbTransaction();
                    if (trans != null)
                    {
                        command.Transaction = trans;
                    }

                    dbContext.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        DataSet dt = new DataSet();
                        while (!result.IsClosed)
                        {
                            DataTable t = new DataTable();
                            t.Load(result);
                            dt.Tables.Add(t);
                        }
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error QueryDataTable {ex.Message} \r\nParametters: [{string.Join(",", parameters?.Select(p => p.ParameterName + "=" + p.Value))}] {rawSql}", ex);
            }
        }

        public static SqlParameter CloneSqlParam(this SqlParameter sqlParameter)
        {
            return new SqlParameter(sqlParameter.ParameterName, sqlParameter.Value)
            {
                Direction = sqlParameter.Direction,
                TypeName = sqlParameter.TypeName,
                DbType = sqlParameter.DbType,
                Size = sqlParameter.Size,
                SqlDbType = sqlParameter.SqlDbType
            };
        }

        public static IList<SqlParameter> CloneSqlParams(this IList<SqlParameter> sqlParameters)
        {
            return sqlParameters?.Select(p => p.CloneSqlParam()).ToList();
        }


        public static async Task<long> InsertDataTable(this DbContext dbContext, DataTable table, bool includeSubId = false)
        {
            var newId = 0L;
            var columns = new HashSet<DataColumn>();
            foreach (DataColumn c in table.Columns)
            {
                columns.Add(c);
            }

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                var insertColumns = new List<string>();
                var sqlParams = new List<SqlParameter>();
                foreach (var c in columns)
                {
                    var cell = row[c];

                    insertColumns.Add(c.ColumnName);
                    sqlParams.Add(new SqlParameter("@" + c.ColumnName, cell));
                }

                if (includeSubId && dbContext is ISubsidiayRequestDbContext requestDbContext)
                {
                    if (!insertColumns.Any(c => c == SubsidiaryIdColumn))
                    {
                        insertColumns.Add(SubsidiaryIdColumn);
                        sqlParams.Add(requestDbContext.CreateSubSqlParam());
                    }
                }

                var idParam = new SqlParameter("@Id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                sqlParams.Add(idParam);
                var sql = $"INSERT INTO [{table.TableName}]({string.Join(",", insertColumns.Select(c => $"[{c}]"))}) VALUES({string.Join(",", sqlParams.Where(p => p.ParameterName != "@Id").Select(p => $"{p.ParameterName}"))}); SELECT @Id = SCOPE_IDENTITY();";

                await dbContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);
                newId = (idParam.Value as long?).GetValueOrDefault();
            }
            return newId;
        }

        public static async Task<int> UpdateCategoryData(this DbContext dbContext, DataTable table, int fId)
        {
            int numberChange = 0;
            //int id = 0;
            var columns = new HashSet<DataColumn>();
            foreach (DataColumn c in table.Columns)
            {
                if (c.ColumnName == nameof(SubsidiaryIdColumn)) { continue; }

                columns.Add(c);
            }

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                var insertColumns = new List<string>();
                var sqlParams = new List<SqlParameter>();
                foreach (var c in columns)
                {
                    var cell = row[c];

                    insertColumns.Add(c.ColumnName);
                    sqlParams.Add(new SqlParameter("@" + c.ColumnName, cell));
                }
                var sql = $"UPDATE [{table.TableName}] SET {string.Join(",", insertColumns.Select(c => $"[{c}] = @{c}"))} WHERE F_Id = {fId}";

                numberChange += await dbContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);

            }
            return numberChange;
        }


        public static async Task<int> AddColumn(this DbContext dbContext, string table, string column, EnumDataType dataType, int dataSize, int decimalPlace, string defaultValue, bool isNullable)
        {
            return await dbContext.ModColumn(table, column, true, dataType, dataSize, decimalPlace, defaultValue, isNullable);
        }
        public static async Task<int> UpdateColumn(this DbContext dbContext, string table, string column, EnumDataType dataType, int dataSize, int decimalPlace, string defaultValue, bool isNullable)
        {
            return await dbContext.ModColumn(table, column, false, dataType, dataSize, decimalPlace, defaultValue, isNullable);
        }

        public static async Task<int> ModColumn(this DbContext dbContext, string table, string column, bool isNewColumn, EnumDataType dataType, int dataSize, int decimalPlace, string defaultValue, bool isNullable)
        {
            if (!table.ValidateValidSqlObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên bảng không được phép"); };
            if (!column.ValidateValidSqlObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên cột không được phép"); };

            if (defaultValue?.Contains("--") == true) { throw new BadRequestException(GeneralCode.InvalidParams, "Giá trị mặc định không được phép"); };

            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parammeters = new[]
            {
                new SqlParameter("@IsAddNew", isNewColumn),
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                new SqlParameter("@DataType", dataType.GetSqlDataType().ToString()),
                new SqlParameter("@DataSize", dataSize),
                new SqlParameter("@DecimalPlace", decimalPlace),
                new SqlParameter("@DefaultValue", defaultValue==null?(object)DBNull.Value:defaultValue),
                new SqlParameter("@IsNullable", isNullable),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_UpdateField", parammeters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static async Task<int> DropColumn(this DbContext dbContext, string table, string column)
        {
            if (!table.ValidateValidSqlObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên bảng không được phép"); };
            if (!column.ValidateValidSqlObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên cột không được phép"); };

            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parameters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_DropField", parameters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static async Task<int> RenameColumn(this DbContext dbContext, string table, string oldColumn, string newColumn)
        {
            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parameters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@OldFieldName", oldColumn),
                new SqlParameter("@NewFieldName", newColumn),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_RenameField", parameters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }

        public static async Task<int> DeleteColumn(this DbContext dbContext, string table, string column)
        {
            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parameters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_DeleteField", parameters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static SqlDbType GetSqlDataType(this EnumDataType dataType) => dataType switch
        {
            EnumDataType.Text => SqlDbType.NVarChar,
            EnumDataType.Int => SqlDbType.Int,
            EnumDataType.Date => SqlDbType.DateTime2,
            EnumDataType.Month => SqlDbType.DateTime2,
            EnumDataType.Year => SqlDbType.DateTime2,
            EnumDataType.QuarterOfYear => SqlDbType.DateTime2,
            EnumDataType.DateRange => SqlDbType.DateTime2,
            EnumDataType.PhoneNumber => SqlDbType.NVarChar,
            EnumDataType.Email => SqlDbType.NVarChar,
            EnumDataType.Boolean => SqlDbType.Bit,
            EnumDataType.Percentage => SqlDbType.TinyInt,
            EnumDataType.BigInt => SqlDbType.BigInt,
            EnumDataType.Decimal => SqlDbType.Decimal,
            _ => SqlDbType.NVarChar
        };

        public static bool ValidateValidSqlObjectName(this string objectName)
        {
            var pattern = @"^[a-zA-Z0-9_\.]{1,64}$";

            Regex regex = new Regex(pattern);
            return regex.IsMatch(objectName);
        }


        public static void FilterClauseProcess(this Clause clause, string tableName, string viewAlias, ref StringBuilder condition, ref List<SqlParameter> sqlParams, ref int suffix, bool not = false, object value = null, NonCamelCaseDictionary refValues = null)
        {
            if (clause != null)
            {
                condition.Append("( ");
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    if (value != null)
                    {
                        singleClause.Value = value;
                    }

                    if (singleClause.Value?.GetType() == typeof(string) && !singleClause.Value.IsNullOrEmptyObject())
                    {
                        singleClause.Value = Regex.Replace(singleClause.Value?.ToString(), "\\{(?<ex>[^\\}]*)\\}", delegate (Match match)
                        {
                            var expression = match.Groups["ex"].Value;
                            return EvalUtils.EvalObject(expression, refValues)?.ToString();
                        });
                    }

                    BuildExpressionRef(singleClause, tableName, viewAlias, ref condition, ref sqlParams, ref suffix, not);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    if (arrClause.Rules.Count == 0)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin trong mảng điều kiện không được để trống.Vui lòng kiểm tra lại cấu hình điều kiện lọc!");
                    }
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        if (indx != 0)
                        {
                            condition.Append(isOr ? " OR " : " AND ");
                        }
                        FilterClauseProcess(arrClause.Rules.ElementAt(indx), tableName, viewAlias, ref condition, ref sqlParams, ref suffix, isNot, value, refValues);
                    }
                }
                else
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin lọc không sai định dạng");
                }
                condition.Append(" )");
            }
        }

        public static void BuildExpression(SingleClause clause, string tableName, string viewAlias, ref StringBuilder conditionAll, ref List<SqlParameter> sqlParams, ref int suffix, bool isNot)
        {
            var aliasField = string.IsNullOrWhiteSpace(viewAlias) ? $"[{clause.FieldName}]" : $"[{viewAlias}].[{clause.FieldName}]";

            var aliasFId = string.IsNullOrWhiteSpace(viewAlias) ? $"[F_Id]" : $"[{viewAlias}].[F_Id]";

            var condition = new StringBuilder();
            if (clause != null)
            {
                var paramName = $"@{clause.FieldName}_filter_{suffix}";
                var defaultValueRawSqlQuote = clause.DataType.GetDefaultValueRawSqlStringWithQuote();
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:

                        if (clause.Value == null || clause.Value == DBNull.Value)
                        {
                            condition.Append($"{aliasField} IS NULL");
                        }
                        else
                        {
                            condition.Append($"{aliasField} = {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        }
                        break;
                    case EnumOperator.NotEqual:

                        if (clause.Value == null || clause.Value == DBNull.Value)
                        {
                            condition.Append($"{aliasField} IS NOT NULL");
                        }
                        else
                        {
                            condition.Append($"{aliasField} <> {paramName} OR {aliasField} IS NULL");
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        }
                        break;
                    case EnumOperator.Contains:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} LIKE {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}%"));
                        }

                        break;
                    case EnumOperator.NotContains:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} OR {aliasField} IS NULL");
                        }
                        else
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} AND {aliasField} IS NOT NULL");
                        }

                        sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}%"));
                        break;
                    case EnumOperator.InList:

                        condition.Append($"{aliasField} IN (");
                        // int inSuffix = 0;
                        // var paramNames = new StringBuilder();
                        IList<object> values = new List<object>();
                        //if (clause.Value is IList<string> lst)
                        //{
                        //    values = lst;
                        //}
                        var type = clause.Value.GetType();
                        if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                        {
                            foreach (object v in (dynamic)clause.Value)
                            {
                                if (!values.Contains(v))
                                    values.Add(v);
                            }
                        }

                        if (clause.Value is string str)
                        {
                            values = (str ?? "").Split(",").Distinct().Select(v => (object)v.Trim()).ToList();
                        }


                        SqlParameter sqlParam;

                        switch (clause.DataType)
                        {
                            case EnumDataType.BigInt:
                                condition.Append($"SELECT [Value] FROM {paramName}");
                                sqlParam = values.Select(v => (long)v).ToList().ToSqlParameter(paramName);
                                break;
                            default:
                                condition.Append($"SELECT [NValue] FROM {paramName}");
                                sqlParam = values.Select(v => v?.ToString()).ToList().ToSqlParameter(paramName);
                                break;

                        }

                        condition.Append(")");

                        sqlParams.Add(sqlParam);


                        break;
                    case EnumOperator.IsLeafNode:
                        var internalAlias = $"{viewAlias}_{suffix}";
                        condition.Append($"NOT EXISTS (SELECT {internalAlias}.F_Id FROM {tableName} {internalAlias} WHERE {internalAlias}.ParentId = {aliasFId})");
                        break;
                    case EnumOperator.StartsWith:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} LIKE {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, $"{clause.Value}%"));
                        }
                        break;
                    case EnumOperator.NotStartsWith:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} OR {aliasField} IS NULL");
                        }
                        else
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} AND {aliasField} IS NOT NULL");
                        }
                        sqlParams.Add(new SqlParameter(paramName, $"{clause.Value}%"));

                        break;
                    case EnumOperator.EndsWith:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} LIKE {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}"));
                        }
                        break;
                    case EnumOperator.NotEndsWith:
                        if (!clause.Value.IsNullOrEmptyObject())
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} OR {aliasField} IS NULL");
                        }
                        else
                        {
                            condition.Append($"{aliasField} NOT LIKE {paramName} AND {aliasField} IS NOT NULL");
                        }
                        sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}"));

                        break;
                    case EnumOperator.Greater:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) > {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.GreaterOrEqual:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) >= {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThan:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) < {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThanOrEqual:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) <= {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.IsNull:
                        condition.Append($"{aliasField} IS NULL");
                        break;
                    case EnumOperator.IsEmpty:
                        condition.Append($"{aliasField} = {defaultValueRawSqlQuote}");
                        break;
                    case EnumOperator.IsNullOrEmpty:
                        condition.Append($"({aliasField} = {defaultValueRawSqlQuote} OR {aliasField} IS NULL)");
                        break;
                    default:
                        break;
                }

                if (isNot)
                {
                    condition.Insert(0, " NOT (");
                    condition.Append(") ");
                }

                conditionAll.Append(condition);
                suffix++;
            }
        }

        public static void BuildExpressionRef(SingleClause clause, string tableName, string viewAlias, ref StringBuilder conditionAll, ref List<SqlParameter> sqlParams, ref int suffix, bool isNot)
        {
            var aliasField = string.IsNullOrWhiteSpace(viewAlias) ? $"[{clause.FieldName}]" : $"[{viewAlias}].[{clause.FieldName}]";

            var aliasFId = string.IsNullOrWhiteSpace(viewAlias) ? $"[F_Id]" : $"[{viewAlias}].[F_Id]";

            var condition = new StringBuilder();
            if (clause != null)
            {
                var paramName = $"@{clause.FieldName}_filter_{suffix}";
                var isRaw = false;
                if (clause.Value.StringStartsWith('\\'))
                {
                    isRaw = true;
                    paramName = clause.Value.ToString()?.TrimStart('\\');
                }
                var defaultValueRawSqlQuote = clause.DataType.GetDefaultValueRawSqlStringWithQuote();
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:

                        condition.Append($"({aliasField} IS NULL AND {paramName} IS NULL)");

                        condition.Append($" OR {aliasField} = {paramName}");

                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));

                        break;
                    case EnumOperator.NotEqual:

                        condition.Append($"{paramName} IS NULL AND {aliasField} IS NOT NULL");
                        condition.Append($" OR {paramName} IS NOT NULL AND ({aliasField} IS NULL OR {aliasField} <> {paramName})");

                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));

                        break;
                    case EnumOperator.Contains:
                        condition.Append($"{paramName} IS NULL");
                        condition.Append($" OR {aliasField} LIKE CONCAT('%',{paramName},'%')");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.Value));

                        break;
                    case EnumOperator.NotContains:
                        condition.Append($"{aliasField} IS NULL AND {paramName} IS NOT NULL");

                        condition.Append($" OR {aliasField} NOT LIKE CONCAT('%',{paramName},'%')");

                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.InList:

                        condition.Append($"{aliasField} IN (");

                        if (!isRaw)
                        {
                            // int inSuffix = 0;
                            // var paramNames = new StringBuilder();
                            IList<object> values = new List<object>();
                            //if (clause.Value is IList<string> lst)
                            //{
                            //    values = lst;
                            //}
                            var type = clause.Value.GetType();
                            if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                            {
                                foreach (object v in (dynamic)clause.Value)
                                {
                                    if (!values.Contains(v))
                                        values.Add(v);
                                }
                            }

                            if (clause.Value is string str)
                            {
                                values = (str ?? "").Split(",").Distinct().Select(v => (object)v.Trim()).ToList();
                            }


                            SqlParameter sqlParam;

                            switch (clause.DataType)
                            {
                                case EnumDataType.BigInt:
                                    condition.Append($"SELECT [Value] FROM {paramName}");
                                    sqlParam = values.Select(v => (long)v).ToList().ToSqlParameter(paramName);
                                    break;
                                default:
                                    condition.Append($"SELECT [NValue] FROM {paramName}");
                                    sqlParam = values.Select(v => v?.ToString()).ToList().ToSqlParameter(paramName);
                                    break;

                            }
                            sqlParams.Add(sqlParam);
                        }
                        else
                        {
                            condition.Append(paramName);
                        }

                        condition.Append(")");


                        break;
                    case EnumOperator.IsLeafNode:
                        var internalAlias = $"{viewAlias}_{suffix}";
                        condition.Append($"NOT EXISTS (SELECT {internalAlias}.F_Id FROM {tableName} {internalAlias} WHERE {internalAlias}.ParentId = {aliasFId})");
                        break;
                    case EnumOperator.StartsWith:

                        condition.Append($"{paramName} IS NULL");
                        condition.Append($" OR {aliasField} LIKE CONCAT({paramName},'%')");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.Value));

                        break;
                    case EnumOperator.NotStartsWith:

                        condition.Append($"{aliasField} IS NULL AND {paramName} IS NOT NULL");

                        condition.Append($" OR {aliasField} NOT LIKE CONCAT({paramName},'%')");

                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));

                        break;
                    case EnumOperator.EndsWith:
                        condition.Append($"{paramName} IS NULL");
                        condition.Append($" OR {aliasField} LIKE CONCAT('%',{paramName})");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.Value));
                        break;
                    case EnumOperator.NotEndsWith:
                        condition.Append($"{aliasField} IS NULL AND {paramName} IS NOT NULL");

                        condition.Append($" OR {aliasField} NOT LIKE CONCAT('%',{paramName})");

                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));

                        break;
                    case EnumOperator.Greater:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) > ISNULL({paramName},{defaultValueRawSqlQuote})");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.GreaterOrEqual:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) >= ISNULL({paramName},{defaultValueRawSqlQuote})");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThan:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) < ISNULL({paramName},{defaultValueRawSqlQuote})");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThanOrEqual:
                        condition.Append($"ISNULL({aliasField},{defaultValueRawSqlQuote}) <= ISNULL({paramName},{defaultValueRawSqlQuote})");
                        if (!isRaw)
                            sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.IsNull:
                        condition.Append($"{aliasField} IS NULL");
                        break;
                    case EnumOperator.IsEmpty:
                        condition.Append($"{aliasField} = {defaultValueRawSqlQuote}");
                        break;
                    case EnumOperator.IsNullOrEmpty:
                        condition.Append($"({aliasField} = {defaultValueRawSqlQuote} OR {aliasField} IS NULL)");
                        break;
                    default:
                        break;
                }

                if (isNot)
                {
                    condition.Insert(0, " NOT (");
                    condition.Append(") ");
                }

                conditionAll.Append(condition);
                suffix++;
            }
        }

        /// <summary>
        /// NValue
        /// </summary>
        /// <param name="values"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static SqlParameter ToSqlParameter(this IList<string> values, string parameterName)
        {
            return values.ToSqlParameter(parameterName, "_NVALUES", "NValue");
        }

        public static SqlParameter ToSqlParameter(this IList<int> values, string parameterName)
        {
            return values.ToSqlParameter(parameterName, "_INTVALUES", "Value");
        }

        public static SqlParameter ToSqlParameter(this IList<long> values, string parameterName)
        {
            return values.ToSqlParameter(parameterName, "_BIGINTVALUES", "Value");
        }

        private static SqlParameter ToSqlParameter<T>(this IList<T> values, string parameterName, string type, string valueColumn)
        {
            var table = new DataTable(type);
            table.Columns.Add(new DataColumn(valueColumn, typeof(T)));
            if (values != null)
            {
                foreach (var item in values)
                {
                    var row = table.NewRow();
                    row[valueColumn] = item;
                    table.Rows.Add(row);
                }
            }

            return new SqlParameter(parameterName, SqlDbType.Structured) { Value = table, TypeName = type };
        }

        public static SqlParameter ToDecimalKeyValueSqlParameter(this NonCamelCaseDictionary<decimal> values, string parameterName)
        {
            var type = "_DECIMAL_KEY_VALUES";
            var keyColumn = "Key";
            var valueColumn = "Value";
            var table = new DataTable(type);
            table.Columns.Add(new DataColumn(keyColumn, typeof(string)));
            table.Columns.Add(new DataColumn(valueColumn, typeof(decimal)));
            if (values != null)
            {
                foreach (var item in values)
                {
                    var row = table.NewRow();
                    row[keyColumn] = item.Key;
                    row[valueColumn] = item.Value;
                    table.Rows.Add(row);
                }
            }
            return new SqlParameter(parameterName, SqlDbType.Structured) { Value = table, TypeName = type };
        }

        public static SqlParameter ToDecimalKeyValueSqlParameter(this NonCamelCaseDictionary<decimal?> values, string parameterName)
        {
            var type = "_DECIMAL_KEY_VALUES";
            var keyColumn = "Key";
            var valueColumn = "Value";
            var table = new DataTable(type);
            table.Columns.Add(new DataColumn(keyColumn, typeof(string)));
            table.Columns.Add(new DataColumn(valueColumn, typeof(decimal)));
            if (values != null)
            {
                foreach (var item in values)
                {
                    var row = table.NewRow();
                    row[keyColumn] = item.Key;
                    row[valueColumn] = item.Value.HasValue ? (object)item.Value.Value : DBNull.Value;
                    table.Rows.Add(row);
                }
            }
            return new SqlParameter(parameterName, SqlDbType.Structured) { Value = table, TypeName = type };
        }

        public static SqlParameter ToSqlParameterValue(this decimal? value, string parameterName)
        {
            return new SqlParameter(parameterName, SqlDbType.Decimal) { Value = value.HasValue ? (object)value : DBNull.Value };
        }

        public static string ToDeclareString(this SqlParameter p)
        {
            var declare = $"{p.ParameterName} {p.SqlDbType}";

            if (p.DbType == DbType.String)
            {
                var length = p.Value?.ToString()?.Length;
                if (!length.HasValue || length < 128)
                {
                    length = 128;
                }
                declare += $" ({length})";
            }

            if (p.DbType == DbType.Decimal)
            {
                declare += $" (32,12)";
            }

            return declare;
        }


        public static DataTable ConvertToDataTable(NonCamelCaseDictionary info, IList<NonCamelCaseDictionary> rows, Dictionary<string, EnumDataType> fields)
        {
            var dataTable = new DataTable();
            foreach (var field in fields)
            {
                dataTable.Columns.Add(field.Key, field.Value.GetColumnDataType());
            }
            foreach (var row in rows)
            {
                var dataRow = dataTable.NewRow();
                foreach (var field in fields)
                {
                    row.TryGetStringValue(field.Key, out var celValue);
                    if (celValue == null) info.TryGetStringValue(field.Key, out celValue);
                    var value = (field.Value).GetSqlValue(celValue);
                    dataRow[field.Key] = value;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        public static DataTable ConvertToIntValues(IList<int> values)
        {
            DataTable rows = new DataTable();
            rows.Columns.Add("Value", typeof(int));
            foreach (var value in values)
            {
                var dataRow = rows.NewRow();
                dataRow["Value"] = value;
                rows.Rows.Add(dataRow);
            }
            return rows;
        }

        private static char[] SpaceChars = new[] { ';', '\n', '\r', '\t', '\v', ' ' };

        public static string TSqlAppendCondition(this string sql, string filterCondition)
        {
            sql = sql.TrimEnd(SpaceChars);

            var parseOptions = new ParseOptions();
            var scanner = new Scanner(parseOptions);

            int state = 0,
                start,
                end,
                token;

            bool isPairMatch, isExecAutoParamHelp;

            scanner.SetSource(sql, 0);

            var commentTokens = new[] { (int)Tokens.LEX_END_OF_LINE_COMMENT, (int)Tokens.LEX_MULTILINE_COMMENT };

            var idxSelect = -1;
            var idxWhereStart = -1;
            var idxWhereEnd = -1;

            var stack = new Stack<string>();

            var correct = true;
            while ((token = scanner.GetNext(ref state, out start, out end, out isPairMatch, out isExecAutoParamHelp)) != (int)Tokens.EOF)
            {
                if (!commentTokens.Contains(token))
                {
                    var sqlToken = sql.Substring(start, end - start + 1);
                    if (string.Equals(sqlToken, "SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        idxSelect = start;
                    }

                    if (string.Equals(sqlToken, "WHERE", StringComparison.OrdinalIgnoreCase))
                    {
                        idxWhereStart = start;
                        idxWhereEnd = end;
                        stack.Clear();
                        correct = true;
                    }

                    if (sqlToken == "(")
                    {
                        stack.Push(sqlToken);
                    }

                    if (sqlToken == ")" && correct)
                    {
                        if (stack.Count == 0)
                        {
                            correct = false;
                        }
                        else
                        {
                            stack.Pop();
                        }
                    }
                }
            }


            if (idxWhereStart < idxSelect)
            {
                idxWhereStart = -1;
            }

            if (idxWhereStart > 0)
            {
                if (stack.Count == 0 && correct)
                {
                    sql = sql.Insert(idxWhereEnd + 1, " (");

                    sql += $"\n) AND ({filterCondition})";
                }
                else
                {
                    sql += $"\n WHERE {filterCondition}";
                }
            }
            else
            {
                sql += $"\n WHERE {filterCondition}";
            }

            return sql;
        }
    }
}
