using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Product.Partial;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductPartialService
    {
        Task<ProductPartialGeneralModel> GeneralInfo(int productId);

        Task<bool> UpdateGeneralInfo(int productId, ProductPartialGeneralModel model);

        Task<ProductPartialStockModel> StockInfo(int productId);

        Task<bool> UpdateStockInfo(int productId, ProductPartialStockModel model);

        Task<ProductPartialSellModel> SellInfo(int productId);

        Task<bool> UpdateSellInfo(int productId, ProductPartialSellModel model);
    }
}
