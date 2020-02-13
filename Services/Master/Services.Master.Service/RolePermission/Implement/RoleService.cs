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
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.RolePermission;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class RoleService : IRoleService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public RoleService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<RoleService> logger
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<int>> AddRole(RoleInput role)
        {
            var validate = ValidateRoleInput(null, role);
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
                RoleStatusId = (int)role.RoleStatusId,
                RootPath = ""
            };

            Role parentInfo = null;
            if (role.ParentRoleId.HasValue)
            {
                parentInfo = _masterContext.Role.FirstOrDefault(r => r.RoleId == role.ParentRoleId);
                if (parentInfo == null) return RoleErrorCode.ParentRoleNotFound;
            }


            await _masterContext.Role.AddAsync(roleInfo);
            await _masterContext.SaveChangesAsync();

            roleInfo.RootPath = FormatRootPath(parentInfo?.RootPath, roleInfo.RoleId);
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Role, roleInfo.RoleId, $"Thêm mới nhóm quyền {roleInfo.RoleName}", role.JsonSerialize());

            return roleInfo.RoleId;
        }

        public async Task<PageData<RoleOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from r in _masterContext.Role
                 select new RoleOutput()
                 {
                     ParentRoleId = r.ParentRoleId,
                     RoleId = r.RoleId,
                     RoleName = r.RoleName,
                     Description = r.Description,
                     RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                     IsEditable = r.IsEditable,
                     RootPath = r.RootPath
                 }
             );

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from r in query
                        where r.RoleName.Contains(keyword)
                        select r;
            }

            var lst = size > 0
                ? await query.OrderBy(r => r.RootPath).Skip((page - 1) * size).Take(size).ToListAsync()
                : await query.OrderBy(r => r.RootPath).ToListAsync();
            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<RoleOutput>> GetRoleInfo(int roleId)
        {
            var roleInfo = await _masterContext.Role.Select(r => new RoleOutput()
            {
                ParentRoleId = r.ParentRoleId,
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                IsEditable = r.IsEditable,
                RootPath = r.RootPath
            }).FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (roleInfo == null)
            {
                return RoleErrorCode.RoleNotFound;
            }

            return roleInfo;
        }

        public async Task<Enum> UpdateRole(int roleId, RoleInput role)
        {
            var validate = ValidateRoleInput(roleId, role);
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

            Role parentInfo = null;
            if (role.ParentRoleId.HasValue)
            {
                parentInfo = _masterContext.Role.FirstOrDefault(r => r.RoleId == role.ParentRoleId);
                if (parentInfo == null) return RoleErrorCode.ParentRoleNotFound;
            }

            roleInfo.RootPath = FormatRootPath(parentInfo?.RootPath, roleId);

            roleInfo.ParentRoleId = role.ParentRoleId;
            roleInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            roleInfo.RoleName = role.RoleName;
            roleInfo.Description = role.Description;
            roleInfo.RoleStatusId = (int)role.RoleStatusId;

            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Role, roleInfo.RoleId, $"Cập nhật nhóm quyền {roleInfo.RoleName}", role.JsonSerialize());

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

            await _activityLogService.CreateLog(EnumObjectType.Role, roleInfo.RoleId, $"Xóa nhóm quyền {roleInfo.RoleName}", roleInfo.JsonSerialize());

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
                var rolePermissions = await _masterContext.RolePermission.Where(p => p.RoleId == roleId).ToListAsync();

                _masterContext.RolePermission.RemoveRange(rolePermissions);

                var newPermissions = new List<Infrastructure.EF.MasterDB.RolePermission>();
                if (permissions.Count > 0)
                {
                    newPermissions = permissions.Select(p => new Infrastructure.EF.MasterDB.RolePermission()
                    {
                        RoleId = roleId,
                        ModuleId = p.ModuleId,
                        Permission = p.Permission,
                        CreatedDatetimeUtc = DateTime.UtcNow
                    }).ToList();

                    await _masterContext.RolePermission.AddRangeAsync(newPermissions);
                }

                await _masterContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.RolePermission, roleId, $"Phân quyền cho nhóm {roleInfo.RoleName}", permissions.JsonSerialize());

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


        public async Task<IList<StockPemissionOutput>> GetStockPermission()
        {
            var roleDataPermissions = await RoleDataPermission(EnumObjectType.Stock);
            return roleDataPermissions.Select(d => new StockPemissionOutput()
            {
                RoleId = d.RoleId,
                StockId = (int)d.ObjectId
            })
            .ToList();
        }

        public async Task<Enum> UpdateStockPermission(IList<StockPemissionOutput> req)
        {
            if (req == null) req = new List<StockPemissionOutput>();

            var lst = _masterContext.RoleDataPermission.Where(o => o.ObjectTypeId == (int)EnumObjectType.Stock);

            _masterContext.RoleDataPermission.RemoveRange(lst);
            _masterContext.RoleDataPermission.AddRange(req.Select(d => new RoleDataPermission()
            {
                ObjectTypeId = (int)EnumObjectType.Stock,
                ObjectId = d.StockId,
                RoleId = d.RoleId
            }));
            await _masterContext.SaveChangesAsync();
            return GeneralCode.Success;
        }
        #region private

        private string FormatRootPath(string parentRootPath, int roleId)
        {
            return string.IsNullOrWhiteSpace(parentRootPath) ? $"{roleId}" : $"{parentRootPath}_{roleId}";
        }
        private Enum ValidateRoleInput(int? currentRoleId, RoleInput req)
        {
            //if (!Enum.IsDefined(req.RoleStatusId.GetType(), req.RoleStatusId))
            //{
            //    return GeneralCode.InvalidParams;
            //}

            if (string.IsNullOrWhiteSpace(req.RoleName))
            {
                return RoleErrorCode.EmptyRoleName;
            }

            if (currentRoleId == req.ParentRoleId)
            {
                return RoleErrorCode.LoopbackParentRole;
            }

            return GeneralCode.Success;
        }
        private async Task<IList<RoleDataPermission>> RoleDataPermission(EnumObjectType objectTypeId)
        {
            return await _masterContext.RoleDataPermission
                .Where(p => p.ObjectTypeId == (int)objectTypeId)
                .AsNoTracking()
                .ToListAsync();
        }
        #endregion
    }
}
