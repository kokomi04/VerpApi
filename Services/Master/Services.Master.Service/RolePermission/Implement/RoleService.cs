﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.Activity;
using RolePermissionEntity = VErp.Infrastructure.EF.MasterDB.RolePermission;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class RoleService : IRoleService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly IInputTypeHelperService _inputTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;
        private readonly IStockHelperService _stockHelperService;

        public RoleService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<RoleService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , ICategoryHelperService categoryHelperService
            , IInputTypeHelperService inputTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
            , IStockHelperService stockHelperService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _categoryHelperService = categoryHelperService;
            _inputTypeHelperService = inputTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
            _stockHelperService = stockHelperService;
        }

        public async Task<int> AddRole(RoleInput role, EnumRoleType roleTypeId)
        {
            if (role.ParentRoleId == 0)
                role.ParentRoleId = null;

            role.RoleName = role.RoleName.Trim();

            var validate = ValidateRoleInput(null, role);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            Role parentInfo = null;

            var roleInfo = new Role()
            {
                RoleName = role.RoleName,
                Description = role.Description,
                ParentRoleId = role.ParentRoleId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                IsEditable = roleTypeId != EnumRoleType.Administrator,
                RoleStatusId = (int)role.RoleStatusId,
                RootPath = "",
                IsModulePermissionInherit = role.IsModulePermissionInherit,
                IsDataPermissionInheritOnStock = role.IsDataPermissionInheritOnStock,
                RoleTypeId = (int)roleTypeId
            };


            if (role.ParentRoleId.HasValue)
            {
                parentInfo = _masterContext.Role.FirstOrDefault(r => r.RoleId == role.ParentRoleId);
                if (parentInfo == null) throw new BadRequestException(RoleErrorCode.ParentRoleNotFound);
            }
            var modules = roleTypeId == EnumRoleType.Administrator ? _masterContext.Module.ToList() : null;

            var categories = roleTypeId == EnumRoleType.Administrator ? await _categoryHelperService.GetDynamicCates() : null;

            var inputTypes = roleTypeId == EnumRoleType.Administrator ? await _inputTypeHelperService.GetInputTypeSimpleList() : null;

            var voucherTypes = roleTypeId == EnumRoleType.Administrator ? await _voucherTypeHelperService.GetVoucherTypeSimpleList() : null;

            var stocks = roleTypeId == EnumRoleType.Administrator ? await _stockHelperService.GetAllStock() : null;

            using (var trans = _masterContext.Database.BeginTransaction())
            {

                await _masterContext.Role.AddAsync(roleInfo);
                await _masterContext.SaveChangesAsync();

                roleInfo.RootPath = FormatRootPath(parentInfo?.RootPath, roleInfo.RoleId);
                await _masterContext.SaveChangesAsync();

                UpdateRoleChildren(roleInfo.RootPath);

                if (roleTypeId == EnumRoleType.Administrator)
                {
                    var lstPermissions = new List<RolePermissionEntity>();

                    foreach (var m in modules)
                    {

                        switch (m.ModuleId)
                        {
                            case (int)EnumModule.CategoryData:
                                foreach (var c in categories)
                                {
                                    var permission = new RolePermissionEntity
                                    {
                                        CreatedDatetimeUtc = DateTime.UtcNow,
                                        ModuleId = m.ModuleId,
                                        RoleId = roleInfo.RoleId,
                                        Permission = int.MaxValue,
                                        ObjectTypeId = (int)EnumObjectType.Category,
                                        ObjectId = c.CategoryId
                                    };
                                    lstPermissions.Add(permission);
                                }
                                break;

                            case (int)EnumModule.Input:

                                foreach (var c in inputTypes)
                                {
                                    var permission = new RolePermissionEntity
                                    {
                                        CreatedDatetimeUtc = DateTime.UtcNow,
                                        ModuleId = m.ModuleId,
                                        RoleId = roleInfo.RoleId,
                                        Permission = int.MaxValue,
                                        ObjectTypeId = (int)EnumObjectType.InputType,
                                        ObjectId = c.InputTypeId,
                                    };
                                    lstPermissions.Add(permission);
                                }
                                break;

                            case (int)EnumModule.SalesBill:
                                foreach (var c in voucherTypes)
                                {
                                    var permission = new RolePermissionEntity
                                    {
                                        CreatedDatetimeUtc = DateTime.UtcNow,
                                        ModuleId = m.ModuleId,
                                        RoleId = roleInfo.RoleId,
                                        Permission = int.MaxValue,
                                        ObjectTypeId = (int)EnumObjectType.VoucherType,
                                        ObjectId = c.VoucherTypeId,
                                    };
                                    lstPermissions.Add(permission);
                                }
                                break;
                            default:

                                var modulePermission = new RolePermissionEntity
                                {
                                    CreatedDatetimeUtc = DateTime.UtcNow,
                                    ModuleId = m.ModuleId,
                                    RoleId = roleInfo.RoleId,
                                    Permission = int.MaxValue,
                                    ObjectTypeId = 0,
                                    ObjectId = 0
                                };
                                lstPermissions.Add(modulePermission);
                                break;
                        }
                    }

                    await _masterContext.RolePermission.AddRangeAsync(lstPermissions);

                    var objectPermission = new List<RoleDataPermission>();
                    foreach (var stock in stocks)
                    {
                        objectPermission.Add(new RoleDataPermission()
                        {
                            RoleId = roleInfo.RoleId,
                            ObjectTypeId = (int)EnumObjectType.Stock,
                            ObjectId = stock.StockId
                        });
                    }
                    await _masterContext.RoleDataPermission.AddRangeAsync(objectPermission);

                    await _masterContext.SaveChangesAsync();
                }

                trans.Commit();
            }

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
                     RootPath = r.RootPath,
                     IsModulePermissionInherit = r.IsModulePermissionInherit,
                     IsDataPermissionInheritOnStock = r.IsDataPermissionInheritOnStock
                 }
             );

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from r in query
                        where r.RoleName.Contains(keyword)
                        select r;
            }

            var lst = size > 0
                ? await query.OrderBy(r => r.IsEditable).ThenBy(r => r.RootPath).Skip((page - 1) * size).Take(size).ToListAsync()
                : await query.OrderBy(r => r.IsEditable).ThenBy(r => r.RootPath).ToListAsync();
            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<RoleOutput> GetRoleInfo(int roleId)
        {
            var roleInfo = await _masterContext.Role.Select(r => new RoleOutput()
            {
                ParentRoleId = r.ParentRoleId,
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                IsEditable = r.IsEditable,
                RootPath = r.RootPath,
                IsModulePermissionInherit = r.IsModulePermissionInherit,
                IsDataPermissionInheritOnStock = r.IsDataPermissionInheritOnStock
            }).FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (roleInfo == null)
            {
                throw new BadRequestException(RoleErrorCode.RoleNotFound);
            }

            return roleInfo;
        }

        public async Task<RoleOutput> GetAdminRoleInfo()
        {
            return await _masterContext.Role
                .Where(r => r.RoleTypeId == (int)EnumRoleType.Administrator)
                .Select(r => new RoleOutput()
                {
                    ParentRoleId = r.ParentRoleId,
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    RoleStatusId = (EnumRoleStatus)r.RoleStatusId,
                    IsEditable = r.IsEditable,
                    RootPath = r.RootPath,
                    IsModulePermissionInherit = r.IsModulePermissionInherit,
                    IsDataPermissionInheritOnStock = r.IsDataPermissionInheritOnStock
                }).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateRole(int roleId, RoleInput role)
        {
            if (role.ParentRoleId == 0)
                role.ParentRoleId = null;

            role.RoleName = role.RoleName.Trim();

            var validate = ValidateRoleInput(roleId, role);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                throw new BadRequestException(RoleErrorCode.RoleNotFound);
            }

            if (!roleInfo.IsEditable)
            {
                throw new BadRequestException(RoleErrorCode.RoleIsReadonly);
            }

            Role parentInfo = null;

            if (role.ParentRoleId.HasValue)
            {
                parentInfo = _masterContext.Role.FirstOrDefault(r => r.RoleId == role.ParentRoleId);
                if (parentInfo == null) throw new BadRequestException(RoleErrorCode.ParentRoleNotFound);
            }

            using (var trans = _masterContext.Database.BeginTransaction())
            {
                roleInfo.RootPath = FormatRootPath(parentInfo?.RootPath, roleId);

                roleInfo.ParentRoleId = role.ParentRoleId;
                roleInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                roleInfo.RoleName = role.RoleName;
                roleInfo.Description = role.Description;
                roleInfo.RoleStatusId = (int)role.RoleStatusId;
                roleInfo.IsModulePermissionInherit = role.IsModulePermissionInherit;
                roleInfo.IsDataPermissionInheritOnStock = role.IsDataPermissionInheritOnStock;

                await _masterContext.SaveChangesAsync();

                UpdateRoleChildren(roleInfo.RootPath);

                trans.Commit();
            }

            await _activityLogService.CreateLog(EnumObjectType.Role, roleInfo.RoleId, $"Cập nhật nhóm quyền {roleInfo.RoleName}", role.JsonSerialize());

            return true;
        }

        public async Task<bool> DeleteRole(int roleId)
        {
            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                throw new BadRequestException(RoleErrorCode.RoleNotFound);
            }
            if (!roleInfo.IsEditable)
            {
                throw new BadRequestException(RoleErrorCode.RoleIsReadonly);
            }
            if (_masterContext.Role.Any(r => r.ParentRoleId == roleId))
            {
                throw new BadRequestException(RoleErrorCode.ExistedChildrenRoles);
            }

            using (var trans = _masterContext.Database.BeginTransaction())
            {
                roleInfo.IsDeleted = true;
                await _masterContext.SaveChangesAsync();

                UpdateRoleChildren(roleInfo.RootPath);

                trans.Commit();
            }

            await _activityLogService.CreateLog(EnumObjectType.Role, roleInfo.RoleId, $"Xóa nhóm quyền {roleInfo.RoleName}", roleInfo.JsonSerialize());

            return true;
        }

        public async Task<bool> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions)
        {
            permissions = permissions.Where(p => p.Permission > 0).ToList();

            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (roleInfo == null)
            {
                throw new BadRequestException(RoleErrorCode.RoleNotFound);
            }

            if (!roleInfo.IsEditable && !_currentContextService.IsDeveloper)
            {
                throw new BadRequestException(RoleErrorCode.RoleIsReadonly);
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var rolePermissions = await _masterContext.RolePermission.Where(p => p.RoleId == roleId).ToListAsync();

                _masterContext.RolePermission.RemoveRange(rolePermissions);

                var newPermissions = new List<Infrastructure.EF.MasterDB.RolePermission>();
                if (permissions.Count > 0)
                {
                    newPermissions = permissions.Select(p => new
                    {
                        RoleId = roleId,
                        ModuleId = p.ModuleId,
                        Permission = p.Permission,
                        ObjectTypeId = p.ObjectTypeId,
                        ObjectId = p.ObjectId
                    }).Distinct()
                    .Select(p => new Infrastructure.EF.MasterDB.RolePermission
                    {
                        RoleId = roleId,
                        ModuleId = p.ModuleId,
                        Permission = p.Permission,
                        ObjectTypeId = p.ObjectTypeId,
                        ObjectId = p.ObjectId
                    })
                    .ToList();

                    await _masterContext.RolePermission.AddRangeAsync(newPermissions);
                }

                await _masterContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.RolePermission, roleId, $"Phân quyền cho nhóm {roleInfo.RoleName}", permissions.JsonSerialize());

                return true;
            }
        }

        public Task<IList<RolePermissionModel>> GetRolePermission(int roleId)
        {
            return GetRolesPermission(new List<int> { roleId });
        }

        public async Task<IList<RolePermissionModel>> GetRolesPermission(IList<int> roleIds, bool? isDeveloper = null)
        {
            var modules = _masterContext.Module.AsQueryable();
            if (isDeveloper.HasValue && !isDeveloper.Value)
            {
                modules = modules.Where(m => !m.IsDeveloper);
            }
            var lst = await (
                from p in _masterContext.RolePermission
                join m in modules on p.ModuleId equals m.ModuleId
                where roleIds.Contains(p.RoleId)
                select new
                {
                    m.ModuleGroupId,
                    p.ModuleId,
                    p.ObjectTypeId,
                    p.ObjectId,
                    p.Permission
                }).ToListAsync();

            return lst.Select(p => new RolePermissionModel()
            {
                ModuleGroupId = p.ModuleGroupId,
                ModuleId = p.ModuleId,
                ObjectTypeId = p.ObjectTypeId,
                ObjectId = p.ObjectId,
                Permission = p.Permission
            }).ToList();
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

        public async Task<bool> UpdateStockPermission(IList<StockPemissionOutput> req)
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
            return true;
        }

        public async Task<bool> GrantDataForAllRoles(EnumObjectType objectTypeId, long objectId)
        {
            var existedRoleIds = (await _masterContext.RoleDataPermission.Where(o => o.ObjectTypeId == (int)objectTypeId && o.ObjectId == objectId).ToListAsync())
                .Select(o => o.RoleId)
                .Distinct()
                .ToHashSet();
            var roles = await _masterContext.Role.ToListAsync();

            var datPermissions = new List<RoleDataPermission>();
            foreach (var role in roles)
            {
                if (!existedRoleIds.Contains(role.RoleId))
                {
                    datPermissions.Add(new RoleDataPermission()
                    {
                        ObjectTypeId = (int)objectTypeId,
                        ObjectId = objectId,
                        RoleId = role.RoleId
                    });
                }
            }

            await _masterContext.RoleDataPermission.AddRangeAsync(datPermissions);
            await _masterContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GrantPermissionForAllRoles(EnumModule moduleId, EnumObjectType objectTypeId, long objectId)
        {
            var existedRoleIds = (await _masterContext.RolePermission.Where(o => o.ModuleId == (int)moduleId && o.ObjectTypeId == (int)objectTypeId && o.ObjectId == objectId).ToListAsync())
                .Select(o => o.RoleId)
                .Distinct()
                .ToHashSet();
            var roles = await _masterContext.Role.ToListAsync();

            var permissions = new List<RolePermissionEntity>();
            foreach (var role in roles)
            {
                if (!existedRoleIds.Contains(role.RoleId))
                {
                    permissions.Add(new RolePermissionEntity()
                    {
                        ModuleId = (int)moduleId,
                        ObjectTypeId = (int)objectTypeId,
                        ObjectId = objectId,
                        RoleId = role.RoleId,
                        Permission = int.MaxValue
                    });
                }
            }

            await _masterContext.RolePermission.AddRangeAsync(permissions);
            await _masterContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<CategoryPermissionModel>> GetCategoryPermissions()
        {
            var roleDataPermissions = await RoleDataPermission(EnumObjectType.Category);
            return roleDataPermissions.Select(x => new CategoryPermissionModel
            {
                CategoryId = (int)x.ObjectId,
                RoleId = x.RoleId
            }).ToList();
        }

        public async Task<bool> UpdateCategoryPermission(IList<CategoryPermissionModel> req)
        {
            if (req == null) req = new List<CategoryPermissionModel>();

            var lst = _masterContext.RoleDataPermission.Where(o => o.ObjectTypeId == (int)EnumObjectType.Category);

            _masterContext.RoleDataPermission.RemoveRange(lst);
            _masterContext.RoleDataPermission.AddRange(req.Select(d => new RoleDataPermission()
            {
                ObjectTypeId = (int)EnumObjectType.Category,
                ObjectId = d.CategoryId,
                RoleId = d.RoleId
            }));
            await _masterContext.SaveChangesAsync();

            return true;
        }

        #region private

        private string FormatRootPath(string parentRootPath, int roleId)
        {
            return string.IsNullOrWhiteSpace(parentRootPath) ? $"{roleId}" : $"{parentRootPath}_{roleId}";
        }

        private IList<int> RootPathToArray(string rootPath)
        {
            return rootPath.Split('_').Select(p => int.Parse(p)).ToList();
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

            if (currentRoleId == req.ParentRoleId && currentRoleId > 0)
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

        private void UpdateRoleChildren(string rootPath)
        {
            var roles = _masterContext.Role.ToList();
            var st = new Stack<int>();
            var childrenRoleIds = new List<int>();
            foreach (var roleId in RootPathToArray(rootPath))
            {
                st.Clear();
                childrenRoleIds.Clear();

                st.Push(roleId);
                while (st.Count > 0)
                {
                    var topRoleId = st.Pop();
                    if (roleId != topRoleId)
                        childrenRoleIds.Add(topRoleId);
                    var children = roles.Where(r => r.ParentRoleId == topRoleId).ToList();
                    foreach (var c in children)
                    {
                        st.Push(c.RoleId);
                    }
                }

                var roleInfo = roles.FirstOrDefault(r => r.RoleId == roleId);
                roleInfo.ChildrenRoleIds = string.Join(",", childrenRoleIds);
            }
            _masterContext.SaveChanges();
        }



        #endregion
    }
}
