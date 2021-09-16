using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.ProductPriceConfig;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.ProductPrice;
using VErp.Commons.Library;

namespace VErp.Services.PurchaseOrder.Service.ProductPrice.Implement {


    public class ProductPriceItemPricingService : IProductPriceItemPricingService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;

        public ProductPriceItemPricingService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IActivityLogService activityLogService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductPriceConfig);
        }

        public async Task<ProductPriceConfigItemPricingUpdate> GetConfigItemPricing(int productPriceConfigId)
        {
            var info = await _purchaseOrderDBContext.ProductPriceConfig
               .FirstOrDefaultAsync(c => c.ProductPriceConfigId == productPriceConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var lst = await _purchaseOrderDBContext.ProductPriceConfigItemPrice.Where(p => p.ProductPriceConfigId == productPriceConfigId).ToListAsync();
             
            var items = _mapper.Map<List<ProductPriceConfigItemPriceModel>>(lst);
            return new ProductPriceConfigItemPricingUpdate()
            {
                Currency = info.Currency,
                Items = items
            };
        }

        public async Task<bool> UpdateConfigItemPricing(int productPriceConfigId, ProductPriceConfigItemPricingUpdate model)
        {
            var info = await _purchaseOrderDBContext.ProductPriceConfig
                .FirstOrDefaultAsync(c => c.ProductPriceConfigId == productPriceConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.Currency = model.Currency;

            var versionInfo = await _purchaseOrderDBContext.ProductPriceConfigVersion.Where(v => v.ProductPriceConfigVersionId == info.LastestProductPriceConfigVersionId).FirstOrDefaultAsync();

            var lst = await _purchaseOrderDBContext.ProductPriceConfigItemPrice.Where(p => p.ProductPriceConfigId == productPriceConfigId).ToListAsync();

            var deleteItems = new List<ProductPriceConfigItemPrice>();

            foreach (var item in lst)
            {
                var updateItem = model.Items.FirstOrDefault(p => p.ProductPriceConfigItemPriceId == item.ProductPriceConfigItemPriceId);
                if (updateItem != null)
                {
                    _mapper.Map(updateItem, item);
                }
                else
                {
                    deleteItems.Add(item);
                }
            }

            var newItems = model.Items.Where(p => p.ProductPriceConfigItemPriceId <= 0)
                .Select(p =>
                {
                    var entity = _mapper.Map<ProductPriceConfigItemPrice>(p);
                    entity.ProductPriceConfigId = productPriceConfigId;
                    return entity;
                }).ToList();

            foreach (var item in deleteItems)
            {
                item.IsDeleted = true;
            }

            if (newItems.Count > 0)
                await _purchaseOrderDBContext.ProductPriceConfigItemPrice.AddRangeAsync(newItems);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objectActivityLog.LogBuilder(() => ProductPriceConfigActivityLogMessage.UpdatedItemsPricing)
                .MessageResourceFormatDatas(versionInfo.Title)
                .ObjectId(productPriceConfigId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return true;

        }
    }
}
