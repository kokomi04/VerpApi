﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Master.Config.ActionButton;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ActionButtonService : IActionButtonService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _actionButtonActivityLog;


        public ActionButtonService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActionButtonService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
            _actionButtonActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ActionButton);
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType objectTypeId, int? objectId)
        {
            var query = _masterDBContext.ActionButton
                .Where(a => a.ObjectTypeId == (int)objectTypeId);
            if (objectId.HasValue)
            {
                query = query.Where(q => q.ObjectId == objectId);
            }
            var lst = await query.ToListAsync();
            return _mapper.Map<IList<ActionButtonModel>>(lst);
        }

        public async Task<IList<ActionButtonSimpleModel>> GetActionButtons(EnumObjectType objectTypeId, int objectId)
        {
            var lst = await _masterDBContext.ActionButton
                 .Where(a => a.ObjectTypeId == (int)objectTypeId && a.ObjectId == objectId)
                 .ToListAsync();
            return _mapper.Map<IList<ActionButtonSimpleModel>>(lst);
        }

        public async Task<ActionButtonModel> AddActionButton(ActionButtonModel data)
        {
            if (_masterDBContext.ActionButton.Any(v => v.ObjectTypeId == (int)data.ObjectTypeId && v.ObjectId == data.ObjectId && v.ActionButtonCode == data.ActionButtonCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var action = _mapper.Map<ActionButton>(data);
            try
            {
                await _masterDBContext.ActionButton.AddAsync(action);
                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Create)
                  .MessageResourceFormatDatas($"{data.Title} {data.ObjectTitle}")
                  .ObjectId(action.ActionButtonId)
                  .JsonData(data.JsonSerialize())
                  .CreateLog();

                return _mapper.Map<ActionButtonModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data)
        {
            data.ActionButtonId = actionButtonId;
            if (_masterDBContext.ActionButton.Any(v => v.ActionButtonId != actionButtonId && v.ObjectTypeId == (int)data.ObjectTypeId && v.ObjectId == data.ObjectId && v.ActionButtonCode == data.ActionButtonCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.ObjectTypeId == (int)data.ObjectTypeId && v.ObjectId == data.ObjectId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }

            _mapper.Map(data, info);


            await _masterDBContext.SaveChangesAsync();

            await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Create)
            .MessageResourceFormatDatas($"{data.Title} {data.ObjectTitle}")
            .ObjectId(info.ActionButtonId)
            .JsonData(data.JsonSerialize())
            .CreateLog();


            return _mapper.Map<ActionButtonModel>(info);

        }


        public async Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType objectTypeId, int objectId)
        {
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.ObjectTypeId == (int)objectTypeId && v.ObjectId == objectId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            return _mapper.Map<ActionButtonModel>(info);
        }

        public async Task<bool> DeleteActionButtonsByType(ActionButtonIdentity data)
        {
            var lst = await _masterDBContext.ActionButton.Where(v => v.ObjectTypeId == (int)data.ObjectTypeId && v.ObjectId == data.ObjectId).ToListAsync();

            try
            {
                using (var batch = _actionButtonActivityLog.BeginBatchLog())
                {
                    foreach (var info in lst)
                    {
                        info.IsDeleted = true;

                        await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                         .MessageResourceFormatDatas($"{info.Title} {data.ObjectTitle}")
                         .ObjectId(info.ActionButtonId)
                         .JsonData(data.JsonSerialize())
                         .CreateLog();
                    }

                    await _masterDBContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                throw;
            }
        }

        public async Task<bool> DeleteActionButton(int actionButtonId, ActionButtonIdentity data)
        {
            data.ActionButtonId = actionButtonId;
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.ObjectTypeId == (int)data.ObjectTypeId && v.ObjectId == data.ObjectId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            try
            {
                info.IsDeleted = true;
                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                       .MessageResourceFormatDatas($"{info.Title} {data.ObjectTitle}")
                       .ObjectId(info.ActionButtonId)
                       .JsonData(data.JsonSerialize())
                       .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                throw;
            }
        }
    }
}

