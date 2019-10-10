using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<int>> AddProduct(ProductModel req);
        Task<ServiceResult<ProductModel>> ProductInfo(int productId);
        Task<Enum> UpdateProduct(int productId, ProductModel req);
        Task<Enum> DeleteProduct(int productId);
    }
}
