using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary
{
    public interface IProductCateService
    {
        Task<PageData<ProductCateOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<int>> AddProductCate(ProductCateInput req);
        Task<Enum> UpdateProductCate(int productCateId, ProductCateInput req);
        Task<Enum> DeleteProductCate(int productCateId);
        Task<ServiceResult<ProductCateOutput>> GetInfoProductCate(int productCateId);
    }
}
