using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary
{
    public interface IProductTypeService
    {
        Task<PageData<ProductTypeOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<int>> AddProductType(ProductTypeInput req);
        Task<Enum> UpdateProductType(int productTypeId, ProductTypeInput req);
        Task<Enum> DeleteProductType(int productTypeId);
        Task<ServiceResult<ProductTypeOutput>> GetInfoProductType(int productTypeId);
    }
}
