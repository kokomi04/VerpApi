using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class SqlDBHelper
    {
        public static async Task ExecuteStoreProcedure(this DbContext dbContext, string procedureName, SqlParameter[] parammeters)
        {
            var sql = new StringBuilder($"EXEC {procedureName}");
            foreach (var p in parammeters)
            {
                sql.Append($" {p.ParameterName} = {p.ParameterName}");
                if (p.Direction == ParameterDirection.Output) sql.Append(" OUTPUT");
                sql.Append(",");
            }

            await dbContext.Database.ExecuteSqlRawAsync(sql.ToString().TrimEnd(','), parammeters);
        }

        public static async Task<DataTable> QueryDataTable(this DbContext dbContext, string rawSql, SqlParameter[] parammeters, CommandType cmdType = CommandType.Text)
        {
            try
            {
                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandType = cmdType;
                    command.CommandText = rawSql;
                    command.Parameters.Clear();
                    command.Parameters.AddRange(parammeters);

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
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error QueryDataTable {ex.Message} \r\nParametters: [{string.Join(",", parammeters?.Select(p => p.ParameterName + "=" + p.Value))}] {rawSql}", ex);
            }
        }

        public static SqlParameter CloneSqlParam(this SqlParameter sqlParameter)
        {
            return new SqlParameter(sqlParameter.ParameterName, sqlParameter.Value)
            {
                Direction = sqlParameter.Direction,
                TypeName = sqlParameter.TypeName,
                DbType = sqlParameter.DbType,
                Size = sqlParameter.Size
            };
        }


        public static async Task<long> InsertDataTable(this DbContext dbContext, DataTable table)
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

            var parammeters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_DropField", parammeters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static async Task<int> RenameColumn(this DbContext dbContext, string table, string oldColumn, string newColumn)
        {
            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parammeters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@OldFieldName", oldColumn),
                new SqlParameter("@NewFieldName", newColumn),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_RenameField", parammeters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }

        public static async Task<int> DeleteColumn(this DbContext dbContext, string table, string column)
        {
            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parammeters = new[]
            {
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_DeleteField", parammeters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static SqlDbType GetSqlDataType(this EnumDataType dataType) => dataType switch
        {
            EnumDataType.Text => SqlDbType.NVarChar,
            EnumDataType.Int => SqlDbType.Int,
            EnumDataType.Date => SqlDbType.DateTime2,
            EnumDataType.Year => SqlDbType.DateTime2,
            EnumDataType.QuarterOfYear => SqlDbType.DateTime2,
            EnumDataType.Month => SqlDbType.DateTime2,
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


        public static void FilterClauseProcess(this Clause clause, string tableName, ref StringBuilder condition, ref List<SqlParameter> sqlParams, ref int suffix, bool not = false, object value = null)
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
                    BuildExpression(singleClause, tableName, ref condition, ref sqlParams, ref suffix, not);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        if (indx != 0)
                        {
                            condition.Append(isOr ? " OR " : " AND ");
                        }
                        FilterClauseProcess(arrClause.Rules.ElementAt(indx), tableName, ref condition, ref sqlParams, ref suffix, isNot, value);
                    }
                }
                else
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin lọc không sai định dạng");
                }
                condition.Append(" )");
            }
        }

        public static void BuildExpression(SingleClause clause, string tableName, ref StringBuilder condition, ref List<SqlParameter> sqlParams, ref int suffix, bool not)
        {
            if (clause != null)
            {
                var paramName = $"@{clause.FieldName}_filter_{suffix}";
                string ope;
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        ope = not ? "!=" : "=";

                        if (clause.Value == null || clause.Value == DBNull.Value)
                        {
                            condition.Append($"([{tableName}].{clause.FieldName} {(not ? "IS NOT NULL" : "IS NULL")} OR [{tableName}].{clause.FieldName} {ope} {paramName})");
                        }
                        else
                        {
                            condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        }

                        sqlParams.Add(new SqlParameter(paramName, clause.Value));

                        break;
                    case EnumOperator.NotEqual:
                        ope = not ? "=" : "!=";
                        if (clause.Value == null || clause.Value == DBNull.Value)
                        {
                            condition.Append($"([{tableName}].{clause.FieldName} {(not ? "IS NULL" : "IS NOT NULL")} OR [{tableName}].{clause.FieldName} {ope} {paramName})");
                        }
                        else
                        {
                            condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        }

                        sqlParams.Add(new SqlParameter(paramName, clause.Value));
                        break;
                    case EnumOperator.Contains:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}%"));
                        break;
                    case EnumOperator.InList:
                        ope = not ? "NOT IN" : "IN";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} (");
                        int inSuffix = 0;
                        foreach (var value in (clause.Value as string).Split(","))
                        {
                            if (suffix > 0)
                            {
                                condition.Append(",");
                            }
                            var inParamName = $"{paramName}_{inSuffix}";
                            condition.Append($"{inParamName}");
                            sqlParams.Add(new SqlParameter(inParamName, clause.DataType.GetSqlValue(value)));
                            inSuffix++;
                        }
                        condition.Append(")");
                        break;
                    case EnumOperator.IsLeafNode:
                        ope = not ? "EXISTS" : "NOT EXISTS";
                        var alias = $"{tableName}_{suffix}";
                        condition.Append($"{ope}(SELECT {alias}.F_Id FROM {tableName} {alias} WHERE {alias}.ParentId = [{tableName}].F_Id)");
                        break;
                    case EnumOperator.StartsWith:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"{clause.Value}%"));
                        break;
                    case EnumOperator.EndsWith:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"%{clause.Value}"));
                        break;
                    case EnumOperator.Greater:
                        ope = not ? ">" : "<=";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.GreaterOrEqual:
                        ope = not ? ">=" : "<";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThan:
                        ope = not ? "<" : ">=";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.LessThanOrEqual:
                        ope = not ? "<=" : ">";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, clause.DataType.GetSqlValue(clause.Value)));
                        break;
                    case EnumOperator.IsNull:
                        ope = not ? "IS NOT NULL" : "IS NULL";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope}");
                        break;
                    case EnumOperator.IsEmpty:
                        ope = not ? "!= ''''" : "=''''";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope}");
                        break;
                    case EnumOperator.IsNullOrEmpty:
                        ope = not ? "IS NOT NULL" : "IS NULL";
                        condition.Append($"( [{tableName}].{clause.FieldName} {ope}");
                        ope = not ? "!= ''''" : "=''''";
                        condition.Append($" AND [{tableName}].{clause.FieldName} {ope})");
                        break;
                    default:
                        break;
                }
                suffix++;
            }
        }

        public static SqlParameter ToNValueSqlParameter(this IList<string> values, string parameterName)
        {
            var type = "_NVALUES";
            var valueColumn = "NValue";
            var table = new DataTable(type);
            table.Columns.Add(new DataColumn(valueColumn, typeof(string)));
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
    }
}
