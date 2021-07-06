using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Services.Master.Service.RolePermission;
using VErp.Services.Master.Service.Users;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class MenuService : IMenuService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IUserService _userService;

        public MenuService(MasterDBContext masterDbContext
            , ILogger<MenuService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IUserService userService

        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _userService = userService;

        }

        public async Task<ICollection<MenuOutputModel>> GetMeMenuList()
        {
            var lstMenu = new List<MenuOutputModel>();
            var lstPermissions = await _userService.GetMePermission();
            var moduleIds = lstPermissions.Select(p => p.ModuleId).ToList();

            var menus = _masterDbContext.Menu.ToList();

            var permissionMenus = menus.Where(m => m.ModuleId <= 0
                || lstPermissions.Any(p => p.ModuleId == m.ModuleId && (m.ObjectTypeId == 0 || m.ObjectTypeId == p.ObjectTypeId && m.ObjectId == p.ObjectId)));

            foreach (var item in permissionMenus)
            {
                if (lstMenu.Any(m => m.MenuId == item.MenuId) || menus.Any(m => m.ParentId == item.MenuId)) continue;

                var info = EntityToModel(item);
                lstMenu.Add(info);

                var parentId = info.ParentId;
                while (parentId > 0)
                {
                    var parent = menus.FirstOrDefault(m => m.MenuId == parentId);
                    parentId = parent?.ParentId ?? 0;
                    if (parent != null && !lstMenu.Any(m => m.MenuId == parent.MenuId))
                    {
                        lstMenu.Add(EntityToModel(parent));
                    }
                }
            }
            return lstMenu.OrderBy(m => m.SortOrder).ToList();
        }



        public async Task<ICollection<MenuOutputModel>> GetList()
        {
            var lstMenu = new List<MenuOutputModel>();
            foreach (var item in await _masterDbContext.Menu.OrderBy(m => m.SortOrder).ToListAsync())
            {               
                lstMenu.Add(EntityToModel(item));
            }
            return lstMenu;
        }

        public async Task<bool> Update(int menuId, MenuInputModel model)
        {

            var obj = await _masterDbContext.Menu.FirstOrDefaultAsync(m => m.MenuId == menuId);

            if (obj == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            obj.ParentId = model.ParentId;
            obj.IsDisabled = model.IsDisabled;
            obj.ModuleId = model.ModuleId;
            obj.ObjectTypeId = model.ObjectTypeId;
            obj.ObjectId = model.ObjectId;
            obj.MenuName = model.MenuName;
            obj.Url = model.Url;
            obj.Icon = model.Icon;
            obj.Param = model.Param;
            obj.UpdatedByUserId = _currentContextService.UserId;
            obj.SortOrder = model.SortOrder;
            obj.IsGroup = model.IsGroup;
            obj.IsAlwaysShowTopMenu = model.IsAlwaysShowTopMenu;
            obj.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _activityLogService.CreateLog(EnumObjectType.Menu, menuId, $"Cập nhật menu {obj.MenuName} ", model.JsonSerialize());

            await _masterDbContext.SaveChangesAsync();
            return true;

        }


        public async Task<bool> Delete(int menuId)
        {

            var obj = await _masterDbContext.Menu.FirstOrDefaultAsync(m => m.MenuId == menuId);
            if (obj == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            obj.IsDeleted = true;
            obj.UpdatedByUserId = _currentContextService.UserId;
            obj.DeletedDatetimeUtc = DateTime.UtcNow;
            await _masterDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Menu, menuId, $"Xoá menu {obj.MenuName} ", obj.JsonSerialize());
            return true;
        }

        public async Task<int> Create(MenuInputModel model)
        {

            var entity = new Menu()
            {
                ParentId = model.ParentId,
                IsDisabled = model.IsDisabled,
                ModuleId = model.ModuleId,
                ObjectTypeId = model.ObjectTypeId,
                ObjectId = model.ObjectId,
                MenuName = model.MenuName,
                Url = model.Url,
                Icon = model.Icon,
                Param = model.Param,
                CreatedByUserId = _currentContextService.UserId,
                UpdatedByUserId = _currentContextService.UserId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                SortOrder = model.SortOrder,
                IsGroup = model.IsGroup,
                IsAlwaysShowTopMenu = model.IsAlwaysShowTopMenu
            };
            _masterDbContext.Menu.Add(entity);
            await _masterDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Menu, entity.MenuId, $"Thêm mới menu {entity.MenuName} ", model.JsonSerialize());

            return entity.MenuId;
        }

        public async Task<MenuOutputModel> Get(int menuId)
        {
            var obj = await _masterDbContext.Menu.FirstOrDefaultAsync(m => m.MenuId == menuId);
            if (obj == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            return EntityToModel(obj);
        }

        private MenuOutputModel EntityToModel(Menu obj)
        {
            return new MenuOutputModel()
            {
                MenuId = obj.MenuId,
                ParentId = obj.ParentId,
                IsDisabled = obj.IsDisabled,
                ModuleId = obj.ModuleId,
                ObjectTypeId = obj.ObjectTypeId,
                ObjectId = obj.ObjectId,
                MenuName = obj.MenuName,
                Url = obj.Url,
                Icon = obj.Icon,
                Param = obj.Param,
                SortOrder = obj.SortOrder,
                IsGroup = obj.IsGroup,
                IsAlwaysShowTopMenu = obj.IsAlwaysShowTopMenu
            };
        }
    }
}
