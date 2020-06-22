using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class SqlDBHelper
    {
        public static async Task ExecuteStoreProcedure(this DbContext dbContext, string procedureName, SqlParameter[] parammeters)
        {
            foreach (var p in parammeters)
            {
                procedureName += $" {p.ParameterName} = {p.ParameterName},";
            }
            await dbContext.Database.ExecuteSqlRawAsync($"EXEC {procedureName.TrimEnd(',')}", parammeters);
        }

        public static async Task<DataTable> QueryDataTable(this DbContext dbContext, string rawSql, SqlParameter[] parammeters)
        {
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = rawSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(parammeters);

                dbContext.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    DataTable dt = new DataTable();
                    dt.Load(result);
                    return dt;
                }
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
            int id = 0;
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

                numberChange+=  await dbContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);

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



        public static SqlDbType GetSqlDataType(this EnumDataType dataType) => dataType switch
        {
            EnumDataType.Text => SqlDbType.NVarChar,
            EnumDataType.Int => SqlDbType.Int,
            EnumDataType.Date => SqlDbType.DateTime2,
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

    }
}
