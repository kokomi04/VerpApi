using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductMaterialsConsumptionService
    {
        Task<IDictionary<int, IEnumerable<ProductMaterialsConsumptionOutput>>> GetProductMaterialsConsumptionByProductIds(IList<int> productIds);

        Task<bool> UpdateProductMaterialsConsumption(int productId, ICollection<ProductMaterialsConsumptionInput> model);
        Task<bool> UpdateProductMaterialsConsumption(int productId, long ProductMaterialsConsumptionId,  ProductMaterialsConsumptionInput model);
        Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int productId);
        Task<long> AddProductMaterialsConsumption(int productId, ProductMaterialsConsumptionInput model);

        CategoryNameModel GetCustomerFieldDataForMapping();
        Task<bool> ImportMaterialsConsumptionFromMapping(int? productId, ImportExcelMapping importExcelMapping, Stream stream);
        Task<IList<MaterialsConsumptionByProduct>> ImportMaterialsConsumptionFromMappingAsPreviewData(int? productId, ImportExcelMapping mapping, Stream stream);

        Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int[] productIds);
        Task<(Stream stream, string fileName, string contentType)> ExportProductMaterialsConsumption(int productId);
    }
}
