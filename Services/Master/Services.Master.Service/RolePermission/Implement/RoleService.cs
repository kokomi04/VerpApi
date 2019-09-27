using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.RolePermission.Interface;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class RoleService : IRoleService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public RoleService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<RoleService> logger
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<int>> AddRole(RoleInput role)
        {
            var validate = ValidateRoleInput(role);
            if (!validate.IsSuccess())
            {
                return validate;
            }

          
            role.RoleName = role.RoleName.Trim();
            var roleInfo = new Role()
            {
                RoleName = role.RoleName,
                Description = role.Description,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                IsEditable = true,
                RoleStatusId = (int)role.RoleStatusId
            };

            await _masterContext.Role.AddAsync(roleInfo);
            await _masterContext.SaveChangesAsync();
            return roleInfo.RoleId;
        }

        public async Task<PageData<RoleOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from r in _masterContext.Role
                 select new RoleOutput()
                 {
                     RoleId = r.RoleId,
                     RoleName = r.RoleName,
                     Description = r.Description,
                     RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                     IsEditable = r.IsEditable
                 }
             );

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from r in query
                        where r.RoleName.Contains(keyword)
                        select r;
            }

            var lst = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<RoleOutput>> GetRoleInfo(int roleId)
        {
            var roleInfo = await _masterContext.Role.Select(r => new RoleOutput()
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                IsEditable = r.IsEditable
            }).FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (roleInfo == null)
            {
                return RoleErrorCode.RoleNotFound;
            }

            return roleInfo;
        }

        public async Task<Enum> UpdateRole(int roleId, RoleInput role)
        {
            var validate = ValidateRoleInput(role);
            if (!validate.IsSuccess())
            {
                return validate;
            }
           
            role.RoleName = role.RoleName.Trim();

            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                return RoleErrorCode.RoleNotFound;
            }

            if (!roleInfo.IsEditable)
            {
                return RoleErrorCode.RoleIsReadonly;
            }           

            roleInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            roleInfo.RoleName = role.RoleName;
            roleInfo.Description = role.Description;
            roleInfo.RoleStatusId = (int)role.RoleStatusId;

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<Enum> DeleteRole(int roleId)
        {
            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                return RoleErrorCode.RoleNotFound;
            }
            if (!roleInfo.IsEditable)
            {
                return RoleErrorCode.RoleIsReadonly;
            }

            roleInfo.IsDeleted = true;
            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<Enum> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions)
        {
            permissions = permissions.Where(p => p.Permission > 0).ToList();

            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                return RoleErrorCode.RoleNotFound;
            }

            if (!roleInfo.IsEditable)
            {
                return RoleErrorCode.RoleIsReadonly;
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var rolePermissions = _masterContext.RolePermission.Where(p => p.RoleId == roleId);

                _masterContext.RolePermission.RemoveRange(rolePermissions);

                if (permissions.Count > 0)
                {
                    await _masterContext.RolePermission.AddRangeAsync(
                        permissions.Select(p => new Infrastructure.EF.MasterDB.RolePermission()
                        {
                            RoleId = roleId,
                            ModuleId = p.ModuleId,
                            Permission = p.Permission,
                            CreatedDatetimeUtc = DateTime.UtcNow
                        }));
                }

                await _masterContext.SaveChangesAsync();

                trans.Commit();

                return GeneralCode.Success;
            }
        }

        public async Task<IList<RolePermissionModel>> GetRolePermission(int roleId)
        {
            return await _masterContext.RolePermission
                .Where(p => p.RoleId == roleId)
                     .Select(p => new RolePermissionModel()
                     {
                         ModuleId = p.ModuleId,
                         Permission = p.Permission,
                     })
                     .ToListAsync();
        }
       

        #region private
        private Enum ValidateRoleInput(RoleInput req)
        {
            //if (!Enum.IsDefined(req.RoleStatusId.GetType(), req.RoleStatusId))
            //{
            //    return GeneralCode.InvalidParams;
            //}

            if (string.IsNullOrWhiteSpace(req.RoleName))
            {
                return RoleErrorCode.EmptyRoleName;
            }
            return GeneralCode.Success;
        }
       
        #endregion
    }
}
