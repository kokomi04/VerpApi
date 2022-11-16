using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        //Task<PageData<ProductListOutput>> GetList(string keyword, IList<int> productIds, string productName, int[] productTypeIds, IList<int> productCateIds, int page, int size, bool? isProductSemi, bool? isProduct, bool? isMaterials, Clause filters = null, IList<int> stockIds = null);
        Task<PageData<ProductListOutput>> GetList(ProductFilterRequestModel req, int page, int size);
        Task<(Stream stream, string fileName, string contentType)> ExportList(ProductExportRequestModel req);

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

        Task<bool> UpdateProductCoefficientManual(int productId, decimal coefficient);

        Task<int> CopyProduct(ProductModel req, int sourceProductId);
        Task<int> CopyProductBom(int sourceProductId, int destProductId);
        Task<int> CopyProductMaterialConsumption(int sourceProductId, int destProductId);

        Task<bool> UpdateProductionProcessVersion(int productId);
        Task<long> GetProductionProcessVersion(int productId);
        Task<(int? productId, string msg)> CheckProductIdsIsUsed(List<int> listProduct);

    }
}
