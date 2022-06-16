using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.RolePermission;

namespace VErpApi.Controllers.System
{
    [Route("api/modules")]
    public class ModulesController : VErpBaseController
    {
        private readonly IModuleService _moduleService;
        public ModulesController(IModuleService moduleService
            )
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Lấy danh sách nhóm module
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("moduleGroups")]
        public async Task<IList<ModuleGroupOutput>> ModuleGroups()
        {
            return (await _moduleService.GetModuleGroups()).ToList();
        }

        /// <summary>
        /// Lấy danh sách modules
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IList<ModuleOutput>> Modules()
        {
            return (await _moduleService.GetList()).ToList();
        }

    }
}