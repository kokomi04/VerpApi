using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Po;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderExcessService : IPurchaseOrderExcessService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _poActivityLog;
        private readonly IProductHelperService _productHelperService;
        public PurchaseOrderExcessService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IMapper mapper,
            IProductHelperService productHelperService,
            IActivityLogService activityLogService
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _productHelperService = productHelperService;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
        }

        public async Task<bool> UpdatePurchaseOrderExcess(long purchaseOrderExcessId, PurchaseOrderExcessModel model)
        {
            var product = model.Title;
            if (model.ProductId > 0)
            {
                var productInfo = await _productHelperService.GetProduct(Convert.ToInt32(model.ProductId.Value));
                product = productInfo.ProductCode;
            }
            var excess = await _purchaseOrderDBContext.PurchaseOrderExcess.FirstOrDefaultAsync(x => x.PurchaseOrderExcessId == purchaseOrderExcessId);
            if (excess == null)
                throw new BadRequestException(PurchaseOrderErrorCode.ExcessNotFound);

            var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(p => p.PurchaseOrderId == excess.PurchaseOrderId);
            if (poInfo == null)
                throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            _mapper.Map(model, excess);
            _purchaseOrderDBContext.SaveChanges();

            await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.UpdatePurchaseOrderExcess)
             .MessageResourceFormatDatas(product, poInfo.PurchaseOrderCode)
             .ObjectId(poInfo.PurchaseOrderId)
             .JsonData((new { purchaseOrderExcessId, model }))
             .CreateLog();

            return true;
        }
    }
}