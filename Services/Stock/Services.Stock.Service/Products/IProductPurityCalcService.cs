using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Product.Calc;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductPurityCalcService
    {
        Task<IList<ProductPurityCalcModel>> GetList();
        Task<ProductPurityCalcModel> GetInfo(int ProductPurityCalcId);
        Task<int> Create(ProductPurityCalcModel req);
        Task<bool> Update(int ProductPurityCalcId, ProductPurityCalcModel req);
        Task<bool> Delete(int ProductPurityCalcId);
    }
}
