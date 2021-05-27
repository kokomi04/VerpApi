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
            foreach (var item in await _masterDbContext.Menu.Where(m => m.ModuleId <= 0
                || lstPermissions.Any(p => p.ModuleId == m.ModuleId && (m.ObjectTypeId == 0 || m.ObjectTypeId == p.ObjectTypeId && m.ObjectId == p.ObjectId))
            ).OrderBy(m => m.SortOrder).ToListAsync())
            {
                var info = new MenuOutputModel()
                {
                    MenuId = item.MenuId,
                    ParentId = item.ParentId,
                    IsDisabled = item.IsDisabled,
                    ModuleId = item.ModuleId,
                    ObjectTypeId = item.ObjectTypeId,
                    ObjectId = item.ObjectId,
                    MenuName = item.MenuName,
                    Url = item.Url,
                    Icon = item.Icon,
                    Param = item.Param,
                    SortOrder = item.SortOrder,
                    IsGroup = item.IsGroup,
                    IsAlwaysShowTopMenu = item.IsAlwaysShowTopMenu
                };
                lstMenu.Add(info);
            }
            return lstMenu;
        }

        public async Task<ICollection<MenuOutputModel>> GetList()
        {
            var lstMenu = new List<MenuOutputModel>();
            foreach (var item in await _masterDbContext.Menu.OrderBy(m => m.SortOrder).ToListAsync())
            {
                var info = new MenuOutputModel()
                {
                    MenuId = item.MenuId,
                    ParentId = item.ParentId,
                    IsDisabled = item.IsDisabled,
                    ModuleId = item.ModuleId,
                    ObjectTypeId = item.ObjectTypeId,
                    ObjectId = item.ObjectId,
                    MenuName = item.MenuName,
                    Url = item.Url,
                    Icon = item.Icon,
                    Param = item.Param,
                    SortOrder = item.SortOrder,
                    IsGroup = item.IsGroup,
                    IsAlwaysShowTopMenu = item.IsAlwaysShowTopMenu
                };
                lstMenu.Add(info);
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
