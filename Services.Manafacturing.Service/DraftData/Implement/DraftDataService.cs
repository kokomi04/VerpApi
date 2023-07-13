using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.DraftData;
using DraftDataEntity = VErp.Infrastructure.EF.ManufacturingDB.DraftData;
using Verp.Resources.Manafacturing.Handover;
using Verp.Resources.Manafacturing.DraftData;

namespace VErp.Services.Manafacturing.Service.DraftData.Implement
{
    public class DraftDataService : IDraftDataService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public DraftDataService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<DraftDataService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.DraftData);
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<DraftDataModel> UpdateDraftData(DraftDataModel data)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var draftData = await _manufacturingDBContext.DraftData.Where(d => d.ObjectId == data.ObjectId && d.ObjectTypeId == data.ObjectTypeId).FirstOrDefaultAsync();
                if (draftData == null)
                {
                    draftData = _mapper.Map<DraftDataEntity>(data);
                    _manufacturingDBContext.DraftData.Add(draftData);
                }
                else
                {
                    _mapper.Map(data, draftData);
                }
                _manufacturingDBContext.SaveChanges();
                trans.Commit();
                await _objActivityLogFacade.LogBuilder(() => DraftDataActivityLogMessage.Update)
                   .MessageResourceFormatDatas(draftData.ObjectId)
                   .ObjectId(draftData.ObjectId)
                   .ObjectType(EnumObjectType.DraftData)
                   .JsonData(data)
                   .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateDraftData");
                throw;
            }
        }

        public async Task<DraftDataModel> GetDraftData(int objectTypeId, long objectId)
        {
            var draftData = await _manufacturingDBContext.DraftData.Where(d => d.ObjectId == objectId && d.ObjectTypeId == objectTypeId).FirstOrDefaultAsync();
            if (draftData == null) return new DraftDataModel
            {
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                Data = string.Empty
            };
            var model = _mapper.Map<DraftDataModel>(draftData);
            return model;
        }

        public async Task<bool> DeleteDraftData(int objectTypeId, long objectId)
        {
            try
            {
                var draftData = _manufacturingDBContext.DraftData.Where(d => d.ObjectId == objectId && d.ObjectTypeId == objectTypeId).FirstOrDefault();

                if (draftData == null) return true;

                _manufacturingDBContext.DraftData.Remove(draftData);
                _manufacturingDBContext.SaveChanges();
                await _objActivityLogFacade.LogBuilder(() => DraftDataActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(objectId)
                   .ObjectId(objectId)
                   .ObjectType(EnumObjectType.DraftData)
                   .JsonData(draftData)
                   .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDraftData");
                throw;
            }
        }
    }
}
