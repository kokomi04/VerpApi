﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
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
        Task<PackageOutputModel> GetInfo(long packageId);

        ///// <summary>
        ///// Thêm mới thông tin kiện
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //Task<ServiceResult<long>> AddPackage(PackageInputModel req);

        /// <summary>
        /// Cập nhật thông tin kiện
        /// </summary>
        /// <param name="packageId">Mã kiện</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> UpdatePackage(long packageId, PackageInputModel req);

        ///// <summary>
        ///// Xóa thông tin kiện (đánh dấu xóa)
        ///// </summary>
        ///// <param name="packageId">Mã kiện</param>
        ///// <returns></returns>
        //Task<Enum> DeletePackage(long packageId);

        /// <summary>
        /// Tách kiện
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> SplitPackage(long packageId, PackageSplitInput req);

        /// <summary>
        /// Gộp kiện
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<long> JoinPackage(PackageJoinInput req);


        Task<PageData<ProductPackageOutputModel>> GetProductPackageListForExport(string keyword, bool? isTwoUnit, bool isIncludedEmptyPackage, IList<int> productCateIds, IList<int> productIds, IList<long> productUnitConversionIds, IList<long> packageIds, IList<int> stockIds, int page = 1, int size = 20, Clause filters = null);
    }
}
