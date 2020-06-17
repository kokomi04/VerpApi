using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
            if (!table.ValidateValidObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên bảng không được phép"); };
            if (!column.ValidateValidObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên cột không được phép"); };

            if (defaultValue?.Contains("--") == true) { throw new BadRequestException(GeneralCode.InvalidParams, "Giá trị mặc định không được phép"); };

            var resultParam = new SqlParameter("@ResStatus", 0) { Direction = ParameterDirection.Output };

            var parammeters = new[]
            {
                new SqlParameter("@IsAddNew", isNewColumn),
                new SqlParameter("@TableName", table),
                new SqlParameter("@FieldName", column),
                new SqlParameter("@DataType", GetSqlDataType(dataType).ToString()),
                new SqlParameter("@DataSize", dataSize),
                new SqlParameter("@DecimalPlace", decimalPlace),
                new SqlParameter("@DefaultValue", defaultValue),
                new SqlParameter("@IsNullable", isNullable),
                resultParam
            };

            await dbContext.ExecuteStoreProcedure("asp_Table_UpdateField", parammeters);
            return (resultParam.Value as int?).GetValueOrDefault();
        }


        public static async Task<int> DropColumn(this DbContext dbContext, string table, string column)
        {
            if (!table.ValidateValidObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên bảng không được phép"); };
            if (!column.ValidateValidObjectName()) { throw new BadRequestException(GeneralCode.InvalidParams, "Tên cột không được phép"); };

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

        private static bool ValidateValidObjectName(this string objectName)
        {
            var pattern = @"^[a-zA-Z][a-zA-Z0-9_]{1,64}$";

            Regex regex = new Regex(pattern);
            return regex.IsMatch(objectName);
        }

        private static SqlDbType GetSqlDataType(EnumDataType dataType) => dataType switch
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

    }
}
