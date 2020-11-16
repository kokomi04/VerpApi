using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductSemi
{
    public interface IProductSemiService
    {
        Task<IList<ProductSemiModel>> GetListProductSemi(long containerId, int containerTypeId);
        Task<long> CreateProductSemi(ProductSemiModel model);
        Task<bool> UpdateProductSemi(long productSemiId, ProductSemiModel model);
        Task<bool> DeleteProductSemi(long productSemiId);
    }
}
