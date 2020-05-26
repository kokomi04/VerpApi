using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, int[] productTypeIds, int[] productCateIds, int page, int size, Dictionary<string, List<string>> filters = null);
        Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds);
        Task<ServiceResult<int>> AddProduct(ProductModel req);
        Task<ServiceResult<ProductModel>> ProductInfo(int productId);
        Task<Enum> UpdateProduct(int productId, ProductModel req);
        Task<Enum> DeleteProduct(int productId);
    }
}
