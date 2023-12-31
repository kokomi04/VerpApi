﻿using ConfigApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrateAndMappingApi.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.MasterDB;
using VErp.WebApis.VErpApi;

namespace MigrateAndMappingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DeveloperApi]
    public class ApiEndpointController : VErpBaseController
    {
        private readonly UnAuthorizeMasterDBContext _masterContext;

        public ApiEndpointController(UnAuthorizeMasterDBContext masterContext
            )
        {
            _masterContext = masterContext;
        }

        private IList<ApiEndpoint> GetApis()
        {
            var lst = new DiscoverApiEndpointService().GetActionsControllerFromAssenbly(typeof(VErpApiAssembly), VErpApiAssembly.ServiceId);

            var configApis = new DiscoverApiEndpointService().GetActionsControllerFromAssenbly(typeof(ConfigApiAssembly), ConfigApiAssembly.ServiceId);

            lst.AddRange(configApis);

            foreach (var item in lst)
            {
                item.ApiEndpointId = Utils.HashApiEndpointId(item.ServiceId, item.Route, (EnumMethod)item.MethodId);
            }
            return lst.OrderBy(e => e.ServiceId).ThenBy(e => $"{e.Route}{e.MethodId}{e.ActionId}".ToLower()).ToList();
        }


        [Route("GetApiEndpoints")]
        [HttpGet]
        public IList<ApiEndpoint> GetApiEndpoints()
        {
            return GetApis();
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

            return lst.OrderBy(e => e.ServiceId).ThenBy(e => $"{e.Route}{e.MethodId}{e.ActionId}".ToLower()).ToList();
        }

        [Route("GetSystemModules")]
        [HttpGet]
        public async Task<IList<Module>> GetSystemModules()
        {
            return await _masterContext.Module.OrderBy(g => g.SortOrder).ToListAsync();
        }

        [Route("GetSystemModuleGroups")]
        [HttpGet]
        public async Task<IList<ModuleGroup>> GetSystemModuleGroups()
        {
            return await _masterContext.ModuleGroup.OrderBy(g => g.SortOrder).ToListAsync();
        }

        [Route("AddSystemModuleGroup")]
        [HttpPost]
        public async Task<bool> AddSystemModuleGroup([FromBody] ModuleGroup data)
        {
            await _masterContext.ModuleGroup.AddAsync(data);
            await _masterContext.SaveChangesAsync();

            return true;
        }

        [Route("UpdateSystemModuleGroup")]
        [HttpPut]
        public async Task<bool> UpdateSystemModuleGroup([FromBody] ModuleGroup data)
        {
            var group = await _masterContext.ModuleGroup.FirstOrDefaultAsync(g => g.ModuleGroupId == data.ModuleGroupId);
            if (group == null)
            {
                throw new Exception("Not found");
            }

            group.ModuleGroupName = data.ModuleGroupName;

            group.SortOrder = data.SortOrder;

            await _masterContext.SaveChangesAsync();

            return true;
        }

        [Route("DeleteSystemModuleGroup")]
        [HttpDelete]
        public async Task<bool> DeleteSystemModuleGroup([FromBody] ModuleGroup data)
        {
            var group = await _masterContext.ModuleGroup.FirstOrDefaultAsync(g => g.ModuleGroupId == data.ModuleGroupId);
            if (group == null)
            {
                throw new Exception("Not found");
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var modules = _masterContext.Module.Where(m => m.ModuleGroupId == data.ModuleGroupId);

                var apiMappings = from m in _masterContext.ModuleApiEndpointMapping
                                  join module in modules on m.ModuleId equals module.ModuleId
                                  select m;

                _masterContext.ModuleApiEndpointMapping.RemoveRange(apiMappings);

                _masterContext.Module.RemoveRange(modules);

                _masterContext.ModuleGroup.Remove(group);

                await _masterContext.SaveChangesAsync();

                trans.Commit();
            }
            return true;
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
            var lst = GetApis();

            var storedMappings = await _masterContext.ModuleApiEndpointMapping.ToListAsync();

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                _masterContext.ModuleApiEndpointMapping.RemoveRange(storedMappings);
                await _masterContext.SaveChangesAsync();

                _masterContext.ApiEndpoint.RemoveRange(_masterContext.ApiEndpoint);

                await _masterContext.SaveChangesAsync();

                await _masterContext.ApiEndpoint.AddRangeAsync(lst);

                var lstNewIds = lst.Select(a => a.ApiEndpointId).ToList();
                var newMappings = storedMappings.Where(a => lstNewIds.Contains(a.ApiEndpointId));
                foreach(var m in newMappings)
                {
                    m.ModuleApiEndpointMappingId = 0;
                }
                _masterContext.ModuleApiEndpointMapping.AddRange(newMappings);

                await _masterContext.SaveChangesAsync();
                trans.Commit();
            }
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



        [Route("AddModule")]
        [HttpPost]
        public async Task<bool> AddModule([FromBody] Module data)
        {
            var reserveRoles = await _masterContext.Role.Where(r => !r.IsEditable).ToListAsync();
            var rolePermissions = new List<RolePermission>();

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {

                await _masterContext.Module.AddAsync(data);
                await _masterContext.SaveChangesAsync();

                foreach (var role in reserveRoles)
                {
                    rolePermissions.Add(new RolePermission()
                    {
                        RoleId = role.RoleId,
                        ModuleId = data.ModuleId,
                        Permission = int.MaxValue
                    });
                }

                await _masterContext.RolePermission.AddRangeAsync(rolePermissions);

                await _masterContext.SaveChangesAsync();

                trans.Commit();

            }
            return true;
        }

        [Route("UpdateModule")]
        [HttpPut]
        public async Task<bool> UpdateModule([FromBody] Module data)
        {
            var info = await _masterContext.Module.FirstOrDefaultAsync(g => g.ModuleId == data.ModuleId);
            if (info == null)
            {
                throw new Exception("Not found");
            }

            info.ModuleGroupId = data.ModuleGroupId;
            info.ModuleName = data.ModuleName;
            info.Description = data.Description;
            info.SortOrder = data.SortOrder;
            info.IsDeveloper = data.IsDeveloper;

            await _masterContext.SaveChangesAsync();

            return true;
        }

        [Route("DeleteModule")]
        [HttpDelete]
        public async Task<bool> DeleteModule([FromBody] Module data)
        {
            var info = await _masterContext.Module.FirstOrDefaultAsync(g => g.ModuleId == data.ModuleId);
            if (info == null)
            {
                throw new Exception("Not found");
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {

                var apiMappings = from m in _masterContext.ModuleApiEndpointMapping
                                  where m.ModuleId == info.ModuleId
                                  select m;
                var permissions = _masterContext.RolePermission.Where(p => p.ModuleId == data.ModuleId);

                _masterContext.RolePermission.RemoveRange(permissions);

                await _masterContext.SaveChangesAsync();

                _masterContext.ModuleApiEndpointMapping.RemoveRange(apiMappings);

                await _masterContext.SaveChangesAsync();

                _masterContext.Module.Remove(info);

                await _masterContext.SaveChangesAsync();

                trans.Commit();
            }
            return true;
        }
    }
}