using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Stock.Service.Dictionary
{
    public interface IProductCateService
    {
        Task<PageData<ProductCateOutput>> GetList(string keyword, int page, int size, string orderBy, bool asc, Clause filters = null);
        Task<int> AddProductCate(ProductCateInput req);
        Task<bool> UpdateProductCate(int productCateId, ProductCateInput req);
        Task<bool> DeleteProductCate(int productCateId);
        Task<ProductCateOutput> GetInfoProductCate(int productCateId);
    }
}
