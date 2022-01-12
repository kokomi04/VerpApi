using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
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
