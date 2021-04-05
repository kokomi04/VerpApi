using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductSemi
{
    public interface IProductSemiConversionService
    {
        Task<long> AddProductSemiConversion(ProductSemiConversionModel model);
        Task<bool> UpdateProductSemiConversion(long productSemiConversionId, ProductSemiConversionModel model);
        Task<bool> DeleteProductSemiConversion(long productSemiConversionId);

        Task<ICollection<ProductSemiConversionModel>> GetAllProductSemiConversionsByProductSemi(long productSemiId);
        Task<ProductSemiConversionModel> GetProductSemiConversion(long productSemiConversionId);
    }
}
