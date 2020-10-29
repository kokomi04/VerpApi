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
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.StoredProcedure;

namespace VErp.Services.Master.Service.StoredProcedure.Implement
{
    public class StoredProcedureService : IStoredProcedureService
    {
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<StoredProcedureService> _logger;
        private readonly MasterDBContext _masterDBContext;
        private readonly ISubSystemService _subSystemService;

        private const string PATTERN = @"(uv|usp|ufn)\w+";

        public StoredProcedureService(MasterDBContext masterDBContext,
            ILogger<StoredProcedureService> logger,
            IActivityLogService activityLogService,
            ISubSystemService subSystemService)
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _subSystemService = subSystemService;
        }

        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList(EnumModuleType moduleType)
        {
            var data = new NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>();
            var db = await GetDatabase(moduleType);

            using (var connection = _masterDBContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                await connection.ChangeDatabaseAsync(db);

                var query = @$"
                select o.name, o.type_desc, m.definition
                from sys.objects o
                join sys.sql_modules m on m.object_id = o.object_id
                where o.name like ('ufn%') or o.name like ('uv%') or o.name like ('usp%');";

                var result = (await _masterDBContext.QueryDataTable(query, Array.Empty<SqlParameter>())).ConvertData();

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
                await connection.CloseAsync();
                return data;
            }
        }

        public async Task<bool> Create(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel)
        {
            var db = await GetDatabase(moduleType);

            using (var connection = _masterDBContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                await connection.ChangeDatabaseAsync(db);

                string sqlQuery = @$"
                                select o.name
                                from sys.objects o
                                join sys.sql_modules m on m.object_id = o.object_id
                                where o.name = '{storedProcedureModel.Name}';";

                if (!storedProcedureModel.Definition.ToLower().StartsWith("create"))
                {
                    throw new BadRequestException(StoredProcedureErrorCode.InvalidStartWith, "Định nghĩa 1 hàm tạo mới bắt đầu với \"CREATE\"");
                }

                InvalidStoreProcedure(storedProcedureModel);

                if ((await _masterDBContext.QueryDataTable(sqlQuery, new List<SqlParameter>().ToArray())).Rows.Count > 0)
                {
                    throw new BadRequestException(StoredProcedureErrorCode.InvalidExists, $"Đã tồn tại {storedProcedureModel.Name}");
                }

                await _masterDBContext.Database.ExecuteSqlRawAsync(storedProcedureModel.Definition);
                await connection.CloseAsync();
                return true;
            }

        }
        public async Task<bool> Update(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel)
        {
            if (!storedProcedureModel.Definition.ToLower().StartsWith("alter"))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidStartWith, "Định nghĩa 1 hàm thay đổi bắt đầu với \"ALTER\"");
            }
            InvalidStoreProcedure(storedProcedureModel);

            var db = await GetDatabase(moduleType);

            using (var connection = _masterDBContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                await connection.ChangeDatabaseAsync(db);

                await _masterDBContext.Database.ExecuteSqlRawAsync(storedProcedureModel.Definition);

                await connection.CloseAsync();
            }
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

            if (!InvalidName(type, storedProcedureModel.Name))
            {
                throw new BadRequestException(StoredProcedureErrorCode.InvalidName);
            }
        }

        public async Task<bool> Drop(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel)
        {
            var db = await GetDatabase(moduleType);
            using (var connection = _masterDBContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                await connection.ChangeDatabaseAsync(db);

                var query = $"DROP {storedProcedureModel.Type.GetEnumDescription()} {storedProcedureModel.Name}";
                await _masterDBContext.Database.ExecuteSqlRawAsync(query);

                await connection.CloseAsync();
            }
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

        private async Task<string> GetDatabase(EnumModuleType moduleType)
        {
            var ls = await _subSystemService.GetDbByModuleTypeId(moduleType);
            return ls[0];
        }

    }
}
