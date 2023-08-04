using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderOrderMappingService : IPurchaseOrderOrderMappingService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICurrentContextService _currentContext;
        private readonly ObjectActivityLogFacade _poActivityLog;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public PurchaseOrderOrderMappingService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , ILogger<PurchaseOrderOrderMappingService> logger
           , IActivityLogService activityLogService
           , ICurrentContextService currentContext
           , IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
            _currentContext = currentContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<long> AddPurchaseOrderOrderMapping(PurchaseOrderOrderMappingModel model)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var entity = _mapper.Map<PurchaseOrderOrderMapping>(model);
                await _purchaseOrderDBContext.PurchaseOrderOrderMapping.AddAsync(entity);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return entity.PurchaseOrderOrderMappingId;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("PurchaseOrderOrderMappingService.AddPurchaseOrderOrderMapping", ex);
                throw;
            }
        }

        public async Task<bool> UpdatePurchaseOrderOrderMapping(PurchaseOrderOrderMappingModel model, long purchaseOrderOrderMappingId)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var entity = await _purchaseOrderDBContext.PurchaseOrderOrderMapping.FirstOrDefaultAsync(x => x.PurchaseOrderOrderMappingId == purchaseOrderOrderMappingId);
                if (entity == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                _mapper.Map(model, entity);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("PurchaseOrderOrderMappingService.UpdatePurchaseOrderOrderMapping", ex);
                throw;
            }
        }

        public async Task<bool> DeletePurchaseOrderOrderMapping(long purchaseOrderOrderMappingId)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var entity = await _purchaseOrderDBContext.PurchaseOrderOrderMapping.FirstOrDefaultAsync(x => x.PurchaseOrderOrderMappingId == purchaseOrderOrderMappingId);
                if (entity == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                entity.IsDeleted = true;
                await _purchaseOrderDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("PurchaseOrderOrderMappingService.DeletePurchaseOrderOrderMapping", ex);
                throw;
            }
        }

        public async Task<IList<PurchaseOrderOrderMappingModel>> GetAllByPurchaseOrderDetailId(long purchaseOrderDetailId)
        {
            return await _purchaseOrderDBContext.PurchaseOrderOrderMapping.Where(x => x.PurchaseOrderDetailId == purchaseOrderDetailId)
            .ProjectTo<PurchaseOrderOrderMappingModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }
    }
}