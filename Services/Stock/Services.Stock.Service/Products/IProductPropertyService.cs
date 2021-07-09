using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductPropertyService
    {
        Task<IList<ProductPropertyModel>> GetProductProperties();
        Task<ProductPropertyModel> GetProductProperty(int productPropertyId);
        Task<int> CreateProductProperty(ProductPropertyModel req);
        Task<int> UpdateProductProperty(int productPropertyId, ProductPropertyModel req);
        Task<bool> DeleteProductProperty(int productPropertyId);
    }
}
