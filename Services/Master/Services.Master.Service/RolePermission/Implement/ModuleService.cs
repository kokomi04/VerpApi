using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ModuleService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<ModuleService> logger
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<IList<ModuleOutput>> GetList()
        {
            return await _masterContext.Module
                .OrderBy(m => m.SortOrder)
                .Select(m => new ModuleOutput()
                {
                    ModuleGroupId = m.ModuleGroupId,
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    Description = m.Description
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
