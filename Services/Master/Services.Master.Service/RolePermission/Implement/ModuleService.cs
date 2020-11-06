using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.RolePermission;

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
    }
}
