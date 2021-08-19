using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model.ProductPrice;

namespace VErp.Services.PurchaseOrder.Service.ProductPrice.Implement
{
    public interface IProductPriceConfigService
    {
        Task<int> Create(ProductPriceConfigVersionModel model);
        Task<bool> Delete(int productPriceConfigId);
        Task<IList<ProductPriceConfigVersionModel>> GetList(bool? isActived);
        Task<int> Update(int productPriceConfigId, ProductPriceConfigVersionModel model);
        Task<ProductPriceConfigVersionModel> VersionInfo(int productPriceConfigVersionId);
        Task<ProductPriceConfigVersionModel> LastestVersionInfo(int productPriceConfigId);
    }
}