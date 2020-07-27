﻿using System;
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

namespace VErp.Services.Master.Service.Config.Implement
{
    public class MenuService : IMenuService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public MenuService(MasterDBContext masterDbContext
            , ILogger<MenuService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService

        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;

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
                    MenuName = item.MenuName,
                    Url = item.Url,
                    Icon = item.Icon,
                    Param = item.Param,
                    SortOrder = item.SortOrder,
                    IsGroup =item.IsGroup
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
                obj.Param = model.Param;
                obj.UpdatedByUserId = _currentContextService.UserId;
                obj.SortOrder = model.SortOrder;
                obj.IsGroup = model.IsGroup;
                obj.UpdatedDatetimeUtc = DateTime.UtcNow;
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
                obj.UpdatedByUserId = _currentContextService.UserId;
                obj.DeletedDatetimeUtc = DateTime.UtcNow;
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
                    Param = model.Param,
                    CreatedByUserId = _currentContextService.UserId,
                    UpdatedByUserId = _currentContextService.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    SortOrder = model.SortOrder,
                    IsGroup = model.IsGroup
                };
                _masterDbContext.Menu.Add(entity);
                await _masterDbContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.Menu, entity.MenuId, $"Thêm mới menu {entity.MenuName} ", model.JsonSerialize());
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