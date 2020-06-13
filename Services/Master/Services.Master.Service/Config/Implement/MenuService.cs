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

namespace VErp.Services.Master.Service.Config.Implement
{
    public class MenuService : IMenuService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public MenuService(MasterDBContext masterDbContext
            , ILogger<MenuService> logger
            , IActivityLogService activityLogService

        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _activityLogService = activityLogService;

        }

        public async Task<ICollection<MenuOutputModel>> GetList()
        {
            var lstMenu = new List<MenuOutputModel>();
            foreach (var item in _masterDbContext.Menu)
            {
                var info = new MenuOutputModel()
                {
                    MenuId = item.MenuId,
                    ParentId = item.ParentId,
                    IsDisabled = item.IsDisabled,
                    ModuleId = item.ModuleId,
                    MenuName = item.MenuName,
                    Url = item.Url,
                    Icon = item.Icon,
                };
                lstMenu.Add(info);
            }
            return lstMenu;
        }

        public async Task<Enum> Update(int menuId, MenuInputModel model)
        {
            try
            {
                var obj = await _masterDbContext.Menu.FirstOrDefaultAsync(m => m.MenuId == menuId);

                if (obj == null)
                {
                    return GeneralCode.ItemNotFound;
                }
                obj.ParentId = model.ParentId;
                obj.IsDisabled = model.IsDisabled;
                obj.ModuleId = model.ModuleId;
                obj.MenuName = model.MenuName;
                obj.Url = model.Url;
                obj.Icon = model.Icon;

                await _activityLogService.CreateLog(EnumObjectType.Menu, menuId, $"Cập nhật menu {obj.MenuName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

       
        public async Task<Enum> Delete(int menuId)
        {
            try
            {
                var obj = await _masterDbContext.Menu.FirstOrDefaultAsync(m => m.MenuId == menuId);
                if (obj == null)
                {
                    return GeneralCode.ItemNotFound;
                }
                obj.IsDeleted = true;
                await _masterDbContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.Menu, menuId, $"Xoá menu {obj.MenuName} ", obj.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<int>> Create(MenuInputModel model)
        {
            var result = new ServiceResult<int>() { Data = 0 };
            try
            {
                var entity = new Menu()
                {
                    ParentId = model.ParentId,
                    IsDisabled = model.IsDisabled,
                    ModuleId = model.ModuleId,
                    MenuName = model.MenuName,
                    Url = model.Url,
                    Icon = model.Icon,
                };
                _masterDbContext.Menu.Add(entity);
                await _activityLogService.CreateLog(EnumObjectType.Menu, entity.MenuId, $"Thêm mới menu {entity.MenuName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();

                result.Code = GeneralCode.Success;
                result.Data = entity.MenuId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                result.Message = ex.Message;
                result.Code = GeneralCode.InternalError;
            }
            return result;
        }
    }
}
