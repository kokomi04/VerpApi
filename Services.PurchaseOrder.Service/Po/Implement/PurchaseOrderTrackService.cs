using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderTrackService : IPurchaseOrderTrackService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContext;
        private readonly IMapper _mapper;

        public PurchaseOrderTrackService(PurchaseOrderDBContext purchaseOrderDBContext, ILogger<PurchaseOrderTrackService> logger, IActivityLogService activityLogService, IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<long> CreatePurchaseOrderTrack(long purchaseOrderId, purchaseOrderTrackedModel req)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var entity = _mapper.Map<PurchaseOrderTracked>(req);
                await _purchaseOrderDBContext.PurchaseOrderTracked.AddAsync(entity);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();
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
                var track = await _purchaseOrderDBContext.PurchaseOrderTracked.FirstOrDefaultAsync(x => x.PurchaseOrderTrackedId == PurchaseOrderTrackId);
                if (track == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                _mapper.Map(req, track);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();
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
                var track = await _purchaseOrderDBContext.PurchaseOrderTracked.FirstOrDefaultAsync(x => x.PurchaseOrderTrackedId == purchaseOrderTrackId);
                if (track == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                track.IsDeleted = true;
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();
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
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdatePurchaseOrderTrackByPurchaseOrderId");
                throw;
            }
        }
    }
}