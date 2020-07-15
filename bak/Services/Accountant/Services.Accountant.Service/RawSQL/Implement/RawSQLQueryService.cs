using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Accountant.Service.RawSQLQuery.Implement
{
    public class RawSQLQueryService : IRawSQLQueryService
    {
        protected readonly AccountingDBContext _accountingContext;
        private readonly ILogger _logger;
        public RawSQLQueryService(AccountingDBContext accountingContext
            , ILogger<RawSQLQueryService> logger
            )
        {
            _accountingContext = accountingContext;
            _logger = logger;
        }

        public async Task<ServiceResult<List<List<Dictionary<string, string>>>>> FromSQLRaw(string query)
        {
            List<List<Dictionary<string, string>>> data = new List<List<Dictionary<string, string>>>();
            using var command = _accountingContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            _accountingContext.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            try
            {
                while (reader.HasRows)
                {
                    List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
                    List<string> columnNames = new List<string>();
                    var schemaTable = reader.GetSchemaTable();
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        columnNames.Add(row.Field<string>("ColumnName"));
                    }
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, string> map = new Dictionary<string, string>();
                        foreach (var columnName in columnNames)
                        {
                            map.Add(columnName, reader[columnName]?.ToString() ?? string.Empty);
                        }
                        result.Add(map);
                    }
                    data.Add(result);
                    reader.NextResult();
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Raw SQL Query");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<string>> GetProcedure(string procedureName)
        {
            string procedure = string.Empty;
            using var command = _accountingContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"select text from syscomments where id=(select id from sysobjects where name='{procedureName}');";
            _accountingContext.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                await reader.ReadAsync();
                var value = reader["text"];
                if (value != null)
                {
                    procedure = value.ToString();
                }
            }
            return procedure;
        }

        public async Task<ServiceResult<List<string>>> GetProcedures()
        {
            List<string> procedures = new List<string>();
            using var command = _accountingContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"Select * from sys.procedures;";
            _accountingContext.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    var value = reader["name"];
                    if (value != null)
                    {
                        procedures.Add(value.ToString());
                    }
                }
            }
            return procedures;
        }

        public Task<ServiceResult<List<List<Dictionary<string, string>>>>> ProcedureExec(string procedure, string[] parameter)
        {
            throw new NotImplementedException();
        }
    }
}
