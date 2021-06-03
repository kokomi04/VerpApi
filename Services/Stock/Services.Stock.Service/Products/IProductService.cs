using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using System.IO;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, int page, int size, bool? isProductSemi, bool? isProduct, Clause filters = null);
        Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListProductsByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListByCodeAndInternalNames(ProductQueryByProductCodeOrInternalNameRequest req);

        Task<int> AddProduct(ProductModel req);

        Task<ProductDefaultModel> ProductAddProductSemi(int parentProductId, ProductDefaultModel req);

        Task<int> AddProductToDb(ProductModel req);

        Task<ProductModel> ProductInfo(int productId);
        Task<bool> UpdateProduct(int productId, ProductModel req);
        Task<bool> DeleteProduct(int productId);

        Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct);
        CategoryNameModel GetFieldMappings();
        Task<bool> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream);

        Task<bool> UpdateProductCoefficientManual(int productId, int coefficient);

    }
}
