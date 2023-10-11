using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class ModuleService : IModuleService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;

        public ModuleService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<ModuleService> logger
            , ICurrentContextService currentContextService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
        }

        public async Task<IList<ModuleOutput>> GetList()
        {
            var query = _masterContext.Module.AsNoTracking();

            if (!_currentContextService.IsDeveloper)
                query = query.Where(x => !x.IsDeveloper);

            return await query
                .OrderBy(m => m.SortOrder)
                .Select(m => new ModuleOutput()
                {
                    ModuleGroupId = m.ModuleGroupId,
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    IsDeveloper = m.IsDeveloper
                }).ToListAsync();
        }



        public async Task<IList<ModuleGroupOutput>> GetModuleGroups()
        {
            return await _masterContext.ModuleGroup
                .OrderBy(g => g.SortOrder)
                .Select(g => new ModuleGroupOutput()
                {
                    ModuleGroupId = g.ModuleGroupId,
                    ModuleGroupName = g.ModuleGroupName,
                }).ToListAsync();
        }

        public async Task<IList<CategoryNameModel>> GetRefCategoryForQuery(EnumModuleType moduleTypeId)
        {
            var fields = await _masterContext.QueryListProc<RefCategoryFieldNameModel>("asp_GetRefCategoryForQuery", new[]
            {
                new SqlParameter("moduleTypeId",(int)moduleTypeId),
                new SqlParameter("moduleId",_currentContextService.ModuleId)
            });

            var lst = new List<CategoryNameModel>();

            return fields.GroupBy(f => f.CategoryCode)
                .OrderBy(g => g.First().SortOrder)
                .Select(g => new CategoryNameModel()
                {
                    CategoryCode = g.Key,
                    CategoryTitle = g.First().CategoryTitle,
                    Fields = g.Cast<CategoryFieldNameModel>().ToList()
                })
                .ToList();

        }

    }

    internal class RefCategoryFieldNameModel : CategoryFieldNameModel
    {
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
    }
}
