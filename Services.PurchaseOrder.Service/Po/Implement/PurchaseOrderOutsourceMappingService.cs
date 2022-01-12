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
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderOutsourceMappingService : IPurchaseOrderOutsourceMappingService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICurrentContextService _currentContext;
        private readonly ObjectActivityLogFacade _poActivityLog;
        private readonly IMapper _mapper;
        public PurchaseOrderOutsourceMappingService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , ILogger<PurchaseOrderOutsourceMappingService> logger
           , IActivityLogService activityLogService
           , ICurrentContextService currentContext
           , IPurchasingSuggestService purchasingSuggestService
           , IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
            _currentContext = currentContext;
            _mapper = mapper;
        }

        public async Task<long> AddPurchaseOrderOutsourceMapping(PurchaseOrderOutsourceMappingModel model)
        {
            var entity = _mapper.Map<PurchaseOrderOutsourceMapping>(model);
            await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();

            return entity.PurchaseOrderOutsourceMappingId;
        }

        public async Task<bool> UpdatePurchaseOrderOutsourceMapping(PurchaseOrderOutsourceMappingModel model, long purchaseOrderOutsourceMappingId)
        {
            var entity = await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.FirstOrDefaultAsync(x => x.PurchaseOrderOutsourceMappingId == purchaseOrderOutsourceMappingId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(model, entity);
            await _purchaseOrderDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePurchaseOrderOutsourceMapping(long purchaseOrderOutsourceMappingId)
        {
            var entity = await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.FirstOrDefaultAsync(x => x.PurchaseOrderOutsourceMappingId == purchaseOrderOutsourceMappingId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            entity.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByPurchaseOrderId(long purchaseOrderDetailId)
        {
            return await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.PurchaseOrderDetailId == purchaseOrderDetailId)
            .ProjectTo<PurchaseOrderOutsourceMappingModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }

        public async Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByProductionOrderCode(string productionOrderCode)
        {
            return await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.ProductionOrderCode == productionOrderCode)
            .ProjectTo<PurchaseOrderOutsourceMappingModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }

        public async Task<bool> ImplicitAddPurchaseOrderOutsourceMappingFromManufacturing(long outsourcePartRequestId, IList<PurchaseOrderOutsourceMappingModel> models)
        {
            var entities = await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.OutsourcePartRequestId == outsourcePartRequestId).ToListAsync();

            foreach (var item in entities)
            {
                var model = models.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (model != null)
                {
                    model.PurchaseOrderOutsourceMappingId = item.PurchaseOrderOutsourceMappingId;
                    _mapper.Map(model, item);

                }
                else item.IsDeleted = true;
            }

            var nEntities = models.Where(x => x.PurchaseOrderOutsourceMappingId <= 0)
                                    .Select(x => new PurchaseOrderOutsourceMapping()
                                    {
                                        ProductId = x.ProductId,
                                        Quantity = x.Quantity,
                                        OutsourcePartRequestId = x.OutsourcePartRequestId,
                                        ProductionOrderCode = x.ProductionOrderCode
                                    }).ToList();

            await _purchaseOrderDBContext.AddRangeAsync(nEntities);
            await _purchaseOrderDBContext.SaveChangesAsync();

            return true;
        }

        public Task<bool> UpdatePurchaseOrderExcess(long purchaseOrderExcessId, PurchaseOrderExcessModel model)
        {
            throw new System.NotImplementedException();
        }
    }
}