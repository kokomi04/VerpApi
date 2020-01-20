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
    public interface IBillOfMaterialService
    {
        /// <summary>
        /// Lấy danh sách BOM của một sản phẩm
        /// </summary>
        /// <param name="productId">Id sản phẩm</param>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<BillOfMaterialOutput>> GetList(int productId, int page, int size);


        /// <summary>
        /// Lấy thông tin của BOM theo sản phẩm
        /// </summary>
        /// <param name="productId">Mã Id sản phẩm</param>
        /// <returns></returns>
        Task<PageData<BillOfMaterialOutput>> GetAll(int productId);

        /// <summary>
        /// Lấy thông tin của BOM
        /// </summary>
        /// <param name="billOfMaterialId">Mã Id BOM</param>        
        /// <returns></returns>
        Task<ServiceResult<BillOfMaterialOutput>> Get(long billOfMaterialId);

        /// <summary>
        /// Thêm mới thông tin BOM
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> Add(BillOfMaterialInput req);

        /// <summary>
        /// Cập nhật thông tin BOM
        /// </summary>
        /// <param name="billOfMaterialId">Mã Id BOM</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> Update(long billOfMaterialId, BillOfMaterialInput req);

        /// <summary>
        /// Xóa thông tin BOM (đánh dấu xóa)
        /// </summary>
        /// <param name="billOfMaterialId">Mã BOM</param>
        /// <param name="rootProductId">Mã id sản phẩm gốc</param>
        /// <returns></returns>
        Task<Enum> Delete(long billOfMaterialId, int rootProductId);
    }
}
