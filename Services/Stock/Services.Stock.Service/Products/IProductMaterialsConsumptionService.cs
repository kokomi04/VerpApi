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
        Task<bool> UpdateProductMaterialsConsumption(int productId, ICollection<ProductMaterialsConsumptionInput> model);
        Task<bool> UpdateProductMaterialsConsumption(int productId, long ProductMaterialsConsumptionId,  ProductMaterialsConsumptionInput model);
        Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int productId);
        Task<long> AddProductMaterialsConsumption(int productId, ProductMaterialsConsumptionInput model);

        CategoryNameModel GetCustomerFieldDataForMapping();
        Task<bool> ImportMaterialsConsumptionFromMapping(int productId, ImportExcelMapping importExcelMapping, Stream stream, int materialsConsumptionGroupId);

        Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int[] productIds);
        Task<(Stream stream, string fileName, string contentType)> ExportProductMaterialsConsumption(int productId);
    }
}
