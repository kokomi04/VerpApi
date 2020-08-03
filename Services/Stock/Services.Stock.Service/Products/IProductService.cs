using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, int[] productTypeIds, int[] productCateIds, int page, int size, Clause filters = null);
        Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListProductsByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListByCodeAndInternalNames(ProductQueryByProductCodeOrInternalNameRequest req);

        Task<ServiceResult<int>> AddProduct(ProductModel req);
        Task<int> AddProductToDb(ProductModel req);

        Task<ServiceResult<ProductModel>> ProductInfo(int productId);
        Task<Enum> UpdateProduct(int productId, ProductModel req);
        Task<Enum> DeleteProduct(int productId);

        Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct);
    }
}
