using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductMaterialsConsumptionGroupService
    {
        Task<int> AddProductMaterialsConsumptionGroup(ProductMaterialsConsumptionGroupModel model);
        Task<bool> UpdateProductMaterialsConsumptionGroup(int groupId, ProductMaterialsConsumptionGroupModel model);
        Task<bool> DeleteProductMaterialsConsumptionGroup(int groupId);
        Task<ProductMaterialsConsumptionGroupModel> GetProductMaterialsConsumptionGroup(int groupId);

        Task<PageData<ProductMaterialsConsumptionGroupModel>> SearchProductMaterialsConsumptionGroup(string keyword, int page, int size);
    }
}
