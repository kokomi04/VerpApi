using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrateAndMappingApi.Services;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Service.RolePermission.Implement;
using VErp.Services.Master.Service.RolePermission.Interface;
using VErp.WebApis.VErpApi;

namespace MigrateAndMappingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEndpointController : ControllerBase
    {
        private readonly MasterDBContext _masterContext;
        private readonly IApiEndpointService _apiEndpointService;

        public ApiEndpointController(MasterDBContext masterContext
            , IApiEndpointService apiEndpointService
            )
        {
            _masterContext = masterContext;
            _apiEndpointService = apiEndpointService;
        }

        [Route("GetApiEndpoints")]
        [HttpGet]
        public async Task<IList<ApiEndpoint>> GetApiEndpoints()
        {
            var lst = new DiscoverApiEndpointService().GetActionsControllerFromAssenbly(typeof(VErpApiAssembly));
            foreach (var item in lst)
            {
                item.ApiEndpointId = _apiEndpointService.HashApiEndpointId(item.Route, (EnumMethod)item.MethodId, (EnumAction)item.ActionId);
            }
            return lst.OrderBy(e => $"{e.Route}{e.MethodId}{e.ActionId}".ToLower()).ToList();
        }

        [Route("GetApiEndpointsMapping")]
        [HttpGet]
        public async Task<IList<ApiEndpoint>> GetApiEndpointsMapping([FromQuery] int moduleId)
        {
            var lst = await (
                 from api in _masterContext.ApiEndpoint
                 join mapping in _masterContext.ModuleApiEndpointMapping on api.ApiEndpointId equals mapping.ApiEndpointId
                 where mapping.ModuleId == moduleId
                 select api
                 )
                 .ToListAsync();

            return lst.OrderBy(e => $"{e.Route}{e.MethodId}{e.ActionId}".ToLower()).ToList();
        }

        [Route("GetSystemModules")]
        [HttpGet]
        public async Task<IList<Module>> GetSystemModules()
        {
            return await _masterContext.Module.ToListAsync();
        }

        [Route("GetSystemModulesMapping")]
        [HttpGet]
        public async Task<IList<Module>> GetSystemModulesMapping([FromQuery] Guid apiEndpointId)
        {
            var lst = await (
                 from module in _masterContext.Module
                 join mapping in _masterContext.ModuleApiEndpointMapping on module.ModuleId equals mapping.ModuleId
                 where mapping.ApiEndpointId == apiEndpointId
                 select module
                 )
                 .ToListAsync();

            return lst.OrderBy(e => $"{e.ModuleName}".ToLower()).ToList();
        }

        [Route("GetSystemModuleInfo")]
        [HttpGet]
        public async Task<Module> GetSystemModuleInfo(int moduleId)
        {
            return await _masterContext.Module.FirstOrDefaultAsync(m => m.ModuleId == moduleId);
        }

        [Route("SyncApiEndpoints")]
        [HttpPost]
        public async Task<bool> SyncApiEndpoints()
        {
            var lst = new DiscoverApiEndpointService().GetActionsControllerFromAssenbly(typeof(VErpApiAssembly));
            foreach (var item in lst)
            {
                item.ApiEndpointId = _apiEndpointService.HashApiEndpointId(item.Route, (EnumMethod)item.MethodId, (EnumAction)item.ActionId);
            }

            _masterContext.ApiEndpoint.RemoveRange(_masterContext.ApiEndpoint);

            await _masterContext.ApiEndpoint.AddRangeAsync(lst);
            await _masterContext.SaveChangesAsync();
            return true;
        }

        [Route("DeleteMapping")]
        [HttpDelete]
        public async Task<bool> DeleteMapping([FromBody] ModuleApiEndpointMapping data)
        {
            var mapping = await _masterContext.ModuleApiEndpointMapping.FirstOrDefaultAsync(m => m.ApiEndpointId == data.ApiEndpointId && m.ModuleId == data.ModuleId);

            if (mapping != null)
            {
                _masterContext.ModuleApiEndpointMapping.Remove(mapping);
                await _masterContext.SaveChangesAsync();
            }
            
            return true;
        }

        [Route("AddMapping")]
        [HttpPost]
        public async Task<bool> AddMapping([FromBody] ModuleApiEndpointMapping data)
        {
            var mapping = await _masterContext.ModuleApiEndpointMapping.FirstOrDefaultAsync(m => m.ApiEndpointId == data.ApiEndpointId && m.ModuleId == data.ModuleId);
            if (mapping == null)
            {
                _masterContext.ModuleApiEndpointMapping.Add(data);

                await _masterContext.SaveChangesAsync();
            }
            return true;
        }
    }
}