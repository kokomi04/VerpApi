using Elasticsearch.Net;
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

        private const string PATTERN = @"(uv|usp|ufn)\w+";

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
                ls.ForEach(x =>
                {
                    string definition = x["definition"].ToString();
                    string target = "create";
                    x["definition"] = "ALTER " + definition.Substring(target.Length);
                });

                data.Add(type.GetEnumDescription(), ls);
            }

            return data;
        }

        public async Task<bool> Create(int type, StoredProcedureModel storedProcedureModel)
        {
            string sqlQuery = @$"select o.name
                                from sys.objects o
                                join sys.sql_modules m on m.object_id = o.object_id
                                where o.name = '{storedProcedureModel.Name}'";

            if (!storedProcedureModel.Definition.ToLower().StartsWith("create"))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidStartWith, "Định nghĩa 1 hàm tạo mới bắt đầu với \"CREATE\"");
            }

            InvalidStoreProcedure(storedProcedureModel);

            if ((await _accountancyDBContext.QueryDataTable(sqlQuery, new List<SqlParameter>().ToArray())).Rows.Count > 0)
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidExists, $"Đã tồn tại {storedProcedureModel.Name}");
            }

            await _accountancyDBContext.Database.ExecuteSqlRawAsync(storedProcedureModel.Definition);
            return true;
        }
        public async Task<bool> Update(int type, StoredProcedureModel storedProcedureModel)
        {
            if (!storedProcedureModel.Definition.ToLower().StartsWith("alter"))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidStartWith, "Định nghĩa 1 hàm thay đổi bắt đầu với \"ALTER\"");
            }
            InvalidStoreProcedure(storedProcedureModel);
            await _accountancyDBContext.Database.ExecuteSqlRawAsync(storedProcedureModel.Definition);
            return true;
        }


        private void InvalidStoreProcedure(StoredProcedureModel storedProcedureModel)
        {
            var type = storedProcedureModel.Type;

            int indexPoint = storedProcedureModel.Definition.IndexOf("AS");

            if (!storedProcedureModel.Definition.Substring(0, indexPoint).ToLower().Contains(type.GetEnumDescription().ToLower()))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidType);
            }

            if (!storedProcedureModel.Definition.Substring(0, indexPoint).ToLower().Contains(storedProcedureModel.Name.ToLower()))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidName);
            }

            if(!InvalidName(type, storedProcedureModel.Name))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidName);
            }
        }

        public async Task<bool> Drop(int type, StoredProcedureModel storedProcedureModel)
        {
            var query = $"DROP {storedProcedureModel.Type.GetEnumDescription()} {storedProcedureModel.Name}";
            await _accountancyDBContext.Database.ExecuteSqlRawAsync(query);
            return true;
        }

        private bool InvalidName(EnumStoreProcedureType type, string name)
        {
            switch (type)
            {
                case EnumStoreProcedureType.Function: 
                    return name.StartsWith("ufn");
                case EnumStoreProcedureType.View:
                    return name.StartsWith("uv");
                case EnumStoreProcedureType.Procedure:
                    return name.StartsWith("usp");
            }

            return false;
        }

    }
}
