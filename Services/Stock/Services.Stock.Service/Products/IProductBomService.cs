using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products    
{
    /// <summary>
    /// I - BOM 
    /// </summary>
    public interface IProductBomService
    {
        /// <summary>
        /// Lấy thông tin của BOM
        /// </summary>
        /// <param name="productBomId">Mã Id BOM</param>        
        /// <returns></returns>
        Task<IList<ProductBomOutput>> GetBOM(long productBomId);

        /// <summary>
        /// Thêm mới thông tin BOM
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<long> Add(ProductBomInput req);

        /// <summary>
        /// Cập nhật thông tin BOM
        /// </summary>
        /// <param name="productBomId">Mã Id BOM</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> Update(long productBomId, ProductBomInput req);

        /// <summary>
        /// Xóa thông tin BOM (đánh dấu xóa)
        /// </summary>
        /// <param name="productBomId">Mã BOM</param>
        /// <param name="rootProductId">Mã id sản phẩm gốc</param>
        /// <returns></returns>
        Task<bool> Delete(long productBomId, int rootProductId);
    }
}
