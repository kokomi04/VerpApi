using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.DraftData;
using DraftDataEntity = VErp.Infrastructure.EF.ManufacturingDB.DraftData;
namespace VErp.Services.Manafacturing.Service.DraftData.Implement
{
    public class DraftDataService : IDraftDataService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public DraftDataService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<DraftDataService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
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
                await _activityLogService.CreateLog(EnumObjectType.DraftData, draftData.ObjectId, $"Cập nhật kế dữ liệu nháp", data);

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
                await _activityLogService.CreateLog(EnumObjectType.DraftData, objectId, $"Xóa dữ liệu nháp", draftData);
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
