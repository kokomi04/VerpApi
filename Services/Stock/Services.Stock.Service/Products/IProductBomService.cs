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
        /// Lấy danh sách BOM của một sản phẩm
        /// </summary>
        /// <param name="productId">Id sản phẩm</param>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductBomOutput>> GetList(int productId, int page, int size);


        /// <summary>
        /// Lấy thông tin của BOM theo sản phẩm
        /// </summary>
        /// <param name="productId">Mã Id sản phẩm</param>
        /// <returns></returns>
        Task<PageData<ProductBomOutput>> GetAll(int productId);

        /// <summary>
        /// Lấy thông tin của BOM
        /// </summary>
        /// <param name="productBomId">Mã Id BOM</param>        
        /// <returns></returns>
        Task<ProductBomOutput> Get(long productBomId);

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
