using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Commons.GlobalObject;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary
{
    public interface IProductTypeService
    {
        Task<PageData<ProductTypeOutput>> GetList(string keyword, int page, int size, Clause filters = null);
        Task<int> AddProductType(ProductTypeInput req);
        Task<bool> UpdateProductType(int productTypeId, ProductTypeInput req);
        Task<bool> DeleteProductType(int productTypeId);
        Task<ProductTypeOutput> GetInfoProductType(int productTypeId);
    }
}
