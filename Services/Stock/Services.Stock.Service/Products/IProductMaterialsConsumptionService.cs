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
        Task<bool> UpdateProductMaterialsConsumptionService(int productId, ICollection<ProductMaterialsConsumptionInput> model);
        Task<bool> UpdateProductMaterialsConsumptionService(int productId, long ProductMaterialsConsumptionId,  ProductMaterialsConsumptionInput model);
        Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumptionService(int productId);
        Task<long> AddProductMaterialsConsumptionService(int productId, ProductMaterialsConsumptionInput model);

        CategoryNameModel GetCustomerFieldDataForMapping();
        Task<bool> ImportMaterialsConsumptionFromMapping(int productId, ImportExcelMapping importExcelMapping, Stream stream, int materialsConsumptionGroupId);
    }
}
