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
        Task<IList<ProductSemiModel>> GetListProductSemiListProductSemiId(IList<long> productSemiIds);
        Task<ProductSemiModel> GetListProductSemiById(long productSemiId);
        Task<IList<ProductSemiModel>> GetListProductSemiByListContainerId(IList<long> lsId);
        Task<long> CreateProductSemi(ProductSemiModel model);
        Task<bool> UpdateProductSemi(long productSemiId, ProductSemiModel model);
        Task<bool> DeleteProductSemi(long productSemiId);
    }
}
