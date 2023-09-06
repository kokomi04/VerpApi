using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ActionType;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using static Verp.Resources.Master.Config.ActionType.ActionTypeValidationMessage;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ActionTypeService : IActionTypeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _actionTypeActivityLog;

        public ActionTypeService(MasterDBContext masterDbContext
            , ILogger<ActionTypeService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
        )
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
            _currentContextService = currentContextService;
            _actionTypeActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ActionType);

        }
        public async Task<int> Create(ActionType model)
        {
            var lstActionTypes = await GetList();
            if (lstActionTypes.Any(t => t.ActionTypeName.NormalizeAsInternalName().Equals(model.ActionTypeName.NormalizeAsInternalName(), StringComparison.OrdinalIgnoreCase)))
            {
                throw ActionTypeNameAlreadyExisted.BadRequest();
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
                throw ActionTypeIdOverflow.BadRequest();
            }
            model.ActionTypeId = newActionTypeId;
            _masterDbContext.ActionType.Add(model);
            await _masterDbContext.SaveChangesAsync();


            await _actionTypeActivityLog.LogBuilder(() => ActionTypeActivityLogMessage.Create)
            .MessageResourceFormatDatas(model.ActionTypeName)
            .ObjectId(newActionTypeId)
            .JsonData(model)
            .CreateLog();

            return newActionTypeId;
        }

        public async Task<bool> Delete(int actionTypeId)
        {
            var info = await _masterDbContext.ActionType.FirstOrDefaultAsync(a => a.ActionTypeId == actionTypeId);
            if (info == null)
            {
                throw ActionTypeNotFound.BadRequest();
            }

            if (!info.IsEditable)
            {
                throw CannotDeleteDefaultActionType.BadRequest();
            }

            _masterDbContext.ActionType.Remove(info);
            await _masterDbContext.SaveChangesAsync();

            await _actionTypeActivityLog.LogBuilder(() => ActionTypeActivityLogMessage.Delete)
             .MessageResourceFormatDatas(info.ActionTypeName)
             .ObjectId(actionTypeId)
             .JsonData(info)
             .CreateLog();

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
                throw ActionTypeNameAlreadyExisted.BadRequest();
            }

            model.ActionTypeId = actionTypeId;
            var info = await _masterDbContext.ActionType.FirstOrDefaultAsync(a => a.ActionTypeId == actionTypeId);
            if (info == null)
            {
                throw ActionTypeNotFound.BadRequest();
            }

            if (!info.IsEditable)
            {
                throw CannotUpdateDefaultActionType.BadRequest();
            }
            _masterDbContext.ActionType.Attach(model);
            model.IsEditable = true;
            await _masterDbContext.SaveChangesAsync();

            await _actionTypeActivityLog.LogBuilder(() => ActionTypeActivityLogMessage.Update)
               .MessageResourceFormatDatas(info.ActionTypeName)
               .ObjectId(actionTypeId)
               .JsonData(info)
               .CreateLog();

            return true;
        }
    }
}
