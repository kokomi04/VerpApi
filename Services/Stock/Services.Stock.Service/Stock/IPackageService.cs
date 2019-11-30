using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IPackageService
    {
        /// <summary>
        /// Lấy danh sách kiện
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<PackageOutputModel>> GetList(int stockId, string keyword, int page, int size);

        /// <summary>
        /// Lấy thông tin của kiện
        /// </summary>
        /// <param name="packageId">Mã kiện</param>
        /// <returns></returns>
        Task<ServiceResult<PackageOutputModel>> GetInfo(long packageId);

        /// <summary>
        /// Thêm mới thông tin kiện
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> AddPackage(PackageInputModel req);

        /// <summary>
        /// Cập nhật thông tin kiện
        /// </summary>
        /// <param name="packageId">Mã kiện</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> UpdatePackage(long packageId, PackageInputModel req);

        /// <summary>
        /// Xóa thông tin kiện (đánh dấu xóa)
        /// </summary>
        /// <param name="packageId">Mã kiện</param>
        /// <returns></returns>
        Task<Enum> DeletePackage(long packageId);

        /// <summary>
        /// Tách kiện
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> SplitPackage(long packageId, PackageSplitInput req);
    }
}
