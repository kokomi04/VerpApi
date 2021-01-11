using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ActionTypeService : IActionTypeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public ActionTypeService(MasterDBContext masterDbContext
            , ILogger<ActionTypeService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;

        }
        public async Task<int> Create(ActionType model)
        {
            var lstActionTypes = await GetList();
            if (lstActionTypes.Any(t => t.ActionTypeName.NormalizeAsInternalName().Equals(model.ActionTypeName.NormalizeAsInternalName(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Tên Action đã tồn tại, vui lòng chọn tên khác");
            }
            var existedActionTypeIds = lstActionTypes.Select(a => a.ActionTypeId).ToHashSet();
            var newActionTypeId = 0;
            for (var i = 0; i < 32; i++)
            {
                var actionTypeId = (int)Math.Pow(2, i);
                if (newActionTypeId == 0 && !existedActionTypeIds.Contains(actionTypeId))
                {
                    newActionTypeId = actionTypeId;
                    break;
                }
            }
            if (newActionTypeId == 0)
            {
                throw new BadRequestException(GeneralCode.NotYetSupported, $"Không thể thêm action");
            }
            model.ActionTypeId = newActionTypeId;
            _masterDbContext.ActionType.Add(model);
            await _masterDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ActionType, newActionTypeId, $"Thêm action {model.ActionTypeName}", model.JsonSerialize());

            return newActionTypeId;
        }

        public async Task<bool> Delete(int actionTypeId)
        {
            var info = await _masterDbContext.ActionType.FirstOrDefaultAsync(a => a.ActionTypeId == actionTypeId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy action");
            }

            if (!info.IsEditable)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Action mặc định không thể xóa");
            }

            _masterDbContext.ActionType.Remove(info);
            await _masterDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ActionType, actionTypeId, $"Xóa action {info.ActionTypeName}", info.JsonSerialize());
            return true;
        }

        public async Task<IList<ActionType>> GetList()
        {
            return await _masterDbContext.ActionType.ToListAsync();
        }

        public async Task<bool> Update(int actionTypeId, ActionType model)
        {
            var lstActionTypes = await GetList();
            if (lstActionTypes.Any(t => t.ActionTypeId != actionTypeId && t.ActionTypeName.NormalizeAsInternalName().Equals(model.ActionTypeName.NormalizeAsInternalName(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Tên Action đã tồn tại, vui lòng chọn tên khác");
            }

            model.ActionTypeId = actionTypeId;
            var info = await _masterDbContext.ActionType.FirstOrDefaultAsync(a => a.ActionTypeId == actionTypeId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy action");
            }

            if (!info.IsEditable)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Action mặc định không thể xóa");
            }
            _masterDbContext.ActionType.Attach(model);
            model.IsEditable = true;
            await _masterDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ActionType, actionTypeId, $"Cập nhật action {info.ActionTypeName}", info.JsonSerialize());
            return true;
        }
    }
}
