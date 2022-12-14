using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.Menu;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Users;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class MenuService : IMenuService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly IUserService _userService;
        private readonly ObjectActivityLogFacade _menuActivityLog;

        public MenuService(MasterDBContext masterDbContext
            , ILogger<MenuService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IUserService userService

        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _currentContextService = currentContextService;
            _userService = userService;
            _menuActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Menu);
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
                    else
                    {
                        parentId = 0;
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

            await ValidateParent(obj);

            await _masterDbContext.SaveChangesAsync();

            await _menuActivityLog.LogBuilder(() => MenuActivityLogMessage.Update)
              .MessageResourceFormatDatas(obj.MenuName)
              .ObjectId(menuId)
              .JsonData(model.JsonSerialize())
              .CreateLog();

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

            await _menuActivityLog.LogBuilder(() => MenuActivityLogMessage.Update)
            .MessageResourceFormatDatas(obj.MenuName)
            .ObjectId(menuId)
            .JsonData(obj.JsonSerialize())
            .CreateLog();

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

            await ValidateParent(entity);

            _masterDbContext.Menu.Add(entity);
            await _masterDbContext.SaveChangesAsync();

            await _menuActivityLog.LogBuilder(() => MenuActivityLogMessage.Create)
            .MessageResourceFormatDatas(entity.MenuName)
            .ObjectId(entity.MenuId)
            .JsonData(model.JsonSerialize())
            .CreateLog();


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

        private async Task ValidateParent(Menu info)
        {
            var menus = await _masterDbContext.Menu.ToListAsync();
            var travledItems = new List<Menu>(){
                info
            };

            var parentId = info.ParentId;
            while (parentId > 0)
            {
                var parent = menus.FirstOrDefault(m => m.MenuId == parentId);
                parentId = parent?.ParentId ?? 0;
                if (parent != null)
                {
                    var existedParent = travledItems.FirstOrDefault(m => m.MenuId == parent.MenuId);
                    if (existedParent == null)
                    {
                        travledItems.Add(parent);
                    }
                    else
                    {
                        throw GeneralCode.ItemNotFound.BadRequest($"Lỗi lặp lại menu cha {existedParent.MenuName}");
                    }

                }
            }
        }
    }
}
