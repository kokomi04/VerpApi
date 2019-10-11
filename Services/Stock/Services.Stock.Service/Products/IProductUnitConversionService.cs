using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductUnitConversionService
    {
        Task<Enum> AddProductUnitConversion(ProductUnitConversionModel req);
        Task<ServiceResult<List<ProductUnitConversionOutput>>> ProductUnitConversionList(int productId);
        Task<Enum> UpdateProductUnitConversion(ProductUnitConversionModel req);
        Task<Enum> DeleteProductUnitConversion(int productId, int secondaryUnitId);
    }
}
