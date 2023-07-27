using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Po;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderTrackService : IPurchaseOrderTrackService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IManufacturingHelperService _manufacturingHelperService;
        private readonly ObjectActivityLogFacade _poActivityLog;

        public PurchaseOrderTrackService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            ILogger<PurchaseOrderTrackService> logger,
            IActivityLogService activityLogService,
            IMapper mapper,
            IManufacturingHelperService manufacturingHelperService)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _mapper = mapper;
            _manufacturingHelperService = manufacturingHelperService;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
        }

        public async Task<long> CreatePurchaseOrderTrack(long purchaseOrderId, purchaseOrderTrackedModel req)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);
                if (poInfo == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var entity = _mapper.Map<PurchaseOrderTracked>(req);
                await _purchaseOrderDBContext.PurchaseOrderTracked.AddAsync(entity);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId);

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.CreatePoTrack)
                   .MessageResourceFormatDatas(poInfo.PurchaseOrderCode, req.Description)
                   .ObjectId(purchaseOrderId)
                   .JsonData(req)
                   .CreateLog();

                return entity.PurchaseOrderTrackedId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("CreatePurchaseOrderTrack", ex);
                throw;
            }
        }

        public async Task<bool> UpdatePurchaseOrderTrack(long purchaseOrderId, long PurchaseOrderTrackId, purchaseOrderTrackedModel req)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {

                var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

                var track = await _purchaseOrderDBContext.PurchaseOrderTracked.FirstOrDefaultAsync(x => x.PurchaseOrderTrackedId == PurchaseOrderTrackId);
                if (poInfo == null || track == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                _mapper.Map(req, track);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId);

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.UpdatePoTrack)
                  .MessageResourceFormatDatas(poInfo.PurchaseOrderCode, req.Description)
                  .ObjectId(purchaseOrderId)
                  .JsonData(req)
                  .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("UpdatePurchaseOrderTrack", ex);
                throw;
            }

        }

        public async Task<bool> DeletePurchaseOrderTrack(long purchaseOrderId, long purchaseOrderTrackId)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

                var track = await _purchaseOrderDBContext.PurchaseOrderTracked.FirstOrDefaultAsync(x => x.PurchaseOrderTrackedId == purchaseOrderTrackId);
                if (poInfo == null || track == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                track.IsDeleted = true;
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId);

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.UpdatePoTrack)
                .MessageResourceFormatDatas(poInfo.PurchaseOrderCode, track.Description)
                .ObjectId(purchaseOrderId)
                .JsonData(track)
                .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("DeletePurchaseOrderTrack", ex);
                throw;
            }
        }

        public async Task<IList<purchaseOrderTrackedModel>> SearchPurchaseOrderTrackByPurchaseOrder(long purchaseOrderId)
        {
            var lst = await _purchaseOrderDBContext.PurchaseOrderTracked
                            .AsNoTracking()
                            .Where(x => x.PurchaseOrderId == purchaseOrderId)
                            .ProjectTo<purchaseOrderTrackedModel>(_mapper.ConfigurationProvider)
                            .ToListAsync();
            return lst;
        }

        public async Task<bool> UpdatePurchaseOrderTrackByPurchaseOrderId(long purchaseOrderId, IList<purchaseOrderTrackedModel> req)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

                if (poInfo == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);


                var purchaseOrderTracks = await _purchaseOrderDBContext.PurchaseOrderTracked
                                            .Where(x => x.PurchaseOrderId == purchaseOrderId)
                                            .ToListAsync();
                foreach (var track in purchaseOrderTracks)
                {
                    var rTrack = req.FirstOrDefault(x => x.PurchaseOrderTrackedId == track.PurchaseOrderTrackedId);
                    if (rTrack != null)
                        _mapper.Map(rTrack, track);
                    else track.IsDeleted = true;
                }

                var newPurchaseOrderTracks = req.AsQueryable()
                                            .ProjectTo<PurchaseOrderTracked>(_mapper.ConfigurationProvider)
                                            .Where(t => !purchaseOrderTracks.Select(x => x.PurchaseOrderTrackedId).Contains(t.PurchaseOrderTrackedId));
                await _purchaseOrderDBContext.PurchaseOrderTracked.AddRangeAsync(newPurchaseOrderTracks);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId);


                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.UpdatePoTrackMulti)
                  .MessageResourceFormatDatas(poInfo.PurchaseOrderCode, string.Join(",", req?.Select(t => t.Description)?.ToArray()))
                  .ObjectId(purchaseOrderId)
                  .JsonData(req)
                  .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePurchaseOrderTrackByPurchaseOrderId");
                throw;
            }
        }

        private async Task<(long[], EnumPurchasingOrderType)> GetAllOutsourceRequestIdInPurchaseOrder(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var outsourceRequestId = _purchaseOrderDBContext.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == purchaseOrderId)
                .Select(x => x.OutsourceRequestId.GetValueOrDefault())
                .Distinct()
                .ToArray();
            return (outsourceRequestId, (EnumPurchasingOrderType)info.PurchaseOrderType);
        }

        private async Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId)
        {
            var (outsourceRequestId, purchaseOrderType) = await GetAllOutsourceRequestIdInPurchaseOrder(purchaseOrderId);

            if (purchaseOrderType == EnumPurchasingOrderType.OutsourcePart)
                return await _manufacturingHelperService.UpdateOutsourcePartRequestStatus(outsourceRequestId);

            if (purchaseOrderType == EnumPurchasingOrderType.OutsourceStep)
                return await _manufacturingHelperService.UpdateOutsourceStepRequestStatus(outsourceRequestId);

            return true;
        }
    }
}