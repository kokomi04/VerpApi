using System.Threading.Tasks;
using VErp.Services.Stock.Model.Product.Partial;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductPartialService
    {
        Task<ProductPartialGeneralModel> GeneralInfo(int productId);

        Task<bool> UpdateGeneralInfo(int productId, ProductPartialGeneralUpdateWithExtraModel model);

        Task<ProductPartialStockModel> StockInfo(int productId);

        Task<bool> UpdateStockInfo(int productId, ProductPartialStockModel model);

        Task<ProductPartialSellModel> SellInfo(int productId);

        Task<bool> UpdateSellInfo(int productId, ProductPartialSellModel model);


        Task<ProductProcessModel> ProcessInfo(int productId);

        Task<bool> UpdateProcessInfo(int productId, ProductProcessModel model);
    }
}
