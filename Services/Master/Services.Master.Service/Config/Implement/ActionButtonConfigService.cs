using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ActionButtonConfigService : IActionButtonConfigService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _actionButtonActivityLog;


        public ActionButtonConfigService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActionButtonConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
            _actionButtonActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ActionButton);
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType billTypeObjectTypeId)
        {
            var query = _masterDBContext.ActionButton
                .Where(a => a.BillTypeObjectTypeId == (int)billTypeObjectTypeId);

            var lst = await query.ToListAsync();
            return _mapper.Map<IList<ActionButtonModel>>(lst);
        }


        public async Task<ActionButtonModel> AddActionButton(ActionButtonModel data, string typeTitle)
        {
            if (_masterDBContext.ActionButton.Any(v => v.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId && v.ActionButtonCode == data.ActionButtonCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var action = _mapper.Map<ActionButton>(data);
            try
            {
                await _masterDBContext.ActionButton.AddAsync(action);
                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Create)
                  .MessageResourceFormatDatas($"{data.Title} {typeTitle}")
                  .ObjectId(action.ActionButtonId)
                  .JsonData(data)
                  .CreateLog();

                return _mapper.Map<ActionButtonModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data, string typeTitle)
        {
            data.ActionButtonId = actionButtonId;
            if (_masterDBContext.ActionButton.Any(v => v.ActionButtonId != actionButtonId && v.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId && v.ActionButtonCode == data.ActionButtonCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }

            _mapper.Map(data, info);


            await _masterDBContext.SaveChangesAsync();

            await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Create)
            .MessageResourceFormatDatas($"{data.Title} {typeTitle}")
            .ObjectId(info.ActionButtonId)
            .JsonData(data)
            .CreateLog();


            return _mapper.Map<ActionButtonModel>(info);

        }


        public async Task<bool> DeleteActionButton(int actionButtonId, ActionButtonIdentity data, string typeTitle)
        {
            data.ActionButtonId = actionButtonId;
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            try
            {
                info.IsDeleted = true;
                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                       .MessageResourceFormatDatas($"{info.Title} {typeTitle}")
                       .ObjectId(info.ActionButtonId)
                       .JsonData(data)
                       .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                throw;
            }
        }

        public async Task<int> AddActionButtonBillType(ActionButtonBillTypeMapping data, string objectTitle)
        {
            var actionButtonId = data.ActionButtonId;
            var info = await _masterDBContext.ActionButton.FirstOrDefaultAsync(v => v.ActionButtonId == actionButtonId && v.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId);
            if (info == null)
            {
                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            }


            try
            {
                var mapping = await _masterDBContext.ActionButtonBillType.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ActionButtonId == data.ActionButtonId && m.BillTypeObjectId == data.BillTypeObjectId);
                if (mapping != null && !mapping.IsDeleted)
                {
                    return mapping.ActionButtonId;
                }

                if (mapping == null)
                {
                    await _masterDBContext.ActionButtonBillType.AddAsync(new ActionButtonBillType()
                    {
                        ActionButtonId = data.ActionButtonId,
                        BillTypeObjectId = data.BillTypeObjectId
                    });
                }
                else
                {
                    mapping.IsDeleted = false;
                }

                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.AddMappingBillTypeObject)
                       .MessageResourceFormatDatas(info.Title, objectTitle)
                       .ObjectId(info.ActionButtonId)
                       .JsonData(data)
                       .CreateLog();

                return actionButtonId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddActionButtonBillType");
                throw;
            }
        }

        public async Task<bool> RemoveActionButtonBillType(ActionButtonBillTypeMapping data, string objectTitle)
        {
            try
            {
                var mapping = (await MappingInfoByBillType(data)).FirstOrDefault();
                if (mapping == null)
                {
                    if (mapping == null) throw new BadRequestException(GeneralCode.ItemNotFound);
                }

                mapping.Mapping.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();

                await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.RemoveMappingBillTypeObject)
                         .MessageResourceFormatDatas(mapping.Button.Title, objectTitle)
                         .ObjectId(mapping.Button.ActionButtonId)
                         .JsonData(data)
                         .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveActionButtonBillType");
                throw;
            }
        }


        public async Task<bool> RemoveAllByBillType(ActionButtonBillTypeMapping data, string objectTitle)
        {
            var lst = await (from m in _masterDBContext.ActionButtonBillType
                             join b in _masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                             where b.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId
                             && m.BillTypeObjectId == data.BillTypeObjectId
                             select new
                             {
                                 Mapping = m,
                                 Button = b
                             }
                             ).ToListAsync();

            try
            {
                using (var batch = _actionButtonActivityLog.BeginBatchLog())
                {
                    foreach (var item in lst)
                    {
                        item.Mapping.IsDeleted = true;

                        await _actionButtonActivityLog.LogBuilder(() => ActionButtonActivityLogMessage.RemoveMappingBillTypeObject)
                         .MessageResourceFormatDatas(item.Button.Title, objectTitle)
                         .ObjectId(item.Mapping.ActionButtonId)
                         .JsonData(data)
                         .CreateLog();
                    }

                    await _masterDBContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveActionButtonsByBillType");
                throw;
            }
        }


        public async Task<IList<ActionButtonBillTypeMapping>> GetMappings(EnumObjectType billTypeObjectTypeId, long? billTypeObjectId)
        {
            return await (from m in _masterDBContext.ActionButtonBillType
                          join b in _masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                          where b.BillTypeObjectTypeId == (int)billTypeObjectTypeId
                          && (!billTypeObjectId.HasValue || m.BillTypeObjectId == billTypeObjectId)
                          select new ActionButtonBillTypeMapping
                          {
                              ActionButtonId = m.ActionButtonId,
                              BillTypeObjectTypeId = billTypeObjectTypeId,
                              BillTypeObjectId = m.BillTypeObjectId

                          }
                             ).ToListAsync();
        }



        private async Task<IList<MappingInfo>> MappingInfoByBillType(ActionButtonBillTypeMapping data)
        {
            return await (from m in _masterDBContext.ActionButtonBillType
                          join b in _masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                          where b.BillTypeObjectTypeId == (int)data.BillTypeObjectTypeId
                          && m.BillTypeObjectId == data.BillTypeObjectId
                          select new MappingInfo
                          {
                              Mapping = m,
                              Button = b
                          }
                           ).ToListAsync();

        }

        private class MappingInfo
        {
            public ActionButtonBillType Mapping { get; set; }
            public ActionButton Button { get; set; }
        }

    }
}

