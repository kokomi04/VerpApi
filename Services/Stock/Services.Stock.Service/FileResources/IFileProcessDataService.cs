using System.Threading.Tasks;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileProcessDataService
    {
        /// <summary>
        /// Nhập dữ liệu khách hàng đối tác
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> ImportCustomerData(int currentUserId, long fileId);

        ///// <summary>
        ///// Nhập dữ liệu tồn kho (nhập kho) đầu kỳ
        ///// </summary>
        ///// <param name="currentUserId"></param>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //Task<bool> ImportInventoryInputOpeningBalance(int currentUserId, InventoryOpeningBalanceModel model);

        ///// <summary>
        ///// Nhập dữ liệu xuất kho đầu kỳ
        ///// </summary>
        ///// <param name="currentUserId"></param>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //Task<bool> ImportInventoryOutput(int currentUserId, InventoryOpeningBalanceModel model);
    }
}
