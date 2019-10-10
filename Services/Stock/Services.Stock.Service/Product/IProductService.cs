using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Product
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<int>> AddProduct(ProductInput req);
        Task<Enum> UpdateProductCate(int productCateId, ProductCateInput req);
        Task<Enum> DeleteProductCate(int productCateId);
        Task<ServiceResult<ProductCateOutput>> GetInfoProductCate(int productCateId);
    }
}
