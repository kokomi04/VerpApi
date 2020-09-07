﻿using Elasticsearch.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.StoredProcedure;

namespace VErp.Services.Accountancy.Service.StoredProcedure.Implement
{
    public class StoredProcedureService : IStoredProcedureService
    {
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<StoredProcedureService> _logger;
        private readonly AccountancyDBContext _accountancyDBContext;

        public StoredProcedureService(AccountancyDBContext accountancyDBContext,
            ILogger<StoredProcedureService> logger,
            IActivityLogService activityLogService)
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList()
        {
            var query = @"
                select o.name, o.type_desc, m.definition
                from sys.objects o
                join sys.sql_modules m on m.object_id = o.object_id
                where o.name like ('ufn%') or o.name like ('uv%') or o.name like ('usp%')
            ";

            var result = (await _accountancyDBContext.QueryDataTable(query, Array.Empty<SqlParameter>())).ConvertData();

            var data = new NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>();

            foreach (var type in new[]{EnumStoreProcedureType.View,
                EnumStoreProcedureType.Procedure,
                EnumStoreProcedureType.Function })
            {
                var ls = result
                    .Where(x => x.Any(y => y.Key.Equals("type_desc") 
                    && y.Value.ToString().Contains(type.GetEnumDescription())))
                    .ToList();
                
                data.Add(type.GetEnumDescription(), ls);
            }

            return data;
        }

        public async Task<bool> Modify(int type, StoredProcedureModel storedProcedureModel)
        {
            await _accountancyDBContext.Database.ExecuteSqlRawAsync(storedProcedureModel.Definition.Replace("CREATE", "CREATE OR ALTER"));
            return true;
        }

        public async Task<bool> Drop(int type, StoredProcedureModel storedProcedureModel)
        {
            var query = $"DROP {storedProcedureModel.Type.GetEnumDescription()} {storedProcedureModel.Name}";
            await _accountancyDBContext.Database.ExecuteSqlRawAsync(query);
            return true;
        }

    }
}
