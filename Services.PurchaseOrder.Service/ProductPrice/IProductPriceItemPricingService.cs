using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model.ProductPrice;

namespace VErp.Services.PurchaseOrder.Service.ProductPrice
{
    public interface IProductPriceItemPricingService
    {
        Task<ProductPriceConfigItemPricingUpdate> GetConfigItemPricing(int productPriceConfigId);
        Task<bool> UpdateConfigItemPricing(int productPriceConfigId, ProductPriceConfigItemPricingUpdate model);
    }
}
