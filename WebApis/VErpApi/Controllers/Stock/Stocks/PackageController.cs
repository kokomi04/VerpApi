﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.package
{
    [Route("api/packages")]

    public class PackageController : VErpBaseController
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService
            )
        {
            _packageService = packageService;

        }

        /// <summary>
        /// Tìm kiếm kiện theo kho 
        /// </summary>
        /// <param name="stockId">Id kho</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns>List of PackageOutputModel</returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<PackageOutputModel>> Get([FromQuery] int stockId = 0, [FromQuery] string keyword = "", [FromQuery] int page = 0, [FromQuery] int size = 0)
        {
            return await _packageService.GetList(stockId, keyword, page, size);
        }


        /// <summary>
        /// Lấy thông tin kiện
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns>PackageOutputModel</returns>
        [HttpGet]
        [Route("{packageId}")]
        public async Task<PackageOutputModel> GetInfo([FromRoute] int packageId)
        {
            return await _packageService.GetInfo(packageId);
        }

        ///// <summary>
        ///// Thêm mới kiện vào kho 
        ///// </summary>
        ///// <param name="packageInputModel"></param>
        ///// <returns>new packageId</returns>
        //[HttpPost]
        //[Route("")]
        //public async Task<ServiceResult<long>> AddPackage([FromBody] PackageInputModel packageInputModel)
        //{
        //    return await _packageService.AddPackage(packageInputModel);
        //}

        /// <summary>
        /// Cập nhật thông tin kiện trong kho
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="packageInputModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{packageId}")]
        public async Task<bool> UpdatePackage([FromRoute] int packageId, [FromBody] PackageInputModel packageInputModel)
        {
            return await _packageService.UpdatePackage(packageId, packageInputModel);
        }

        ///// <summary>
        ///// Xóa thông tin kiện trong kho
        ///// </summary>
        ///// <param name="packageId"></param>
        ///// <returns></returns>
        //[HttpDelete]
        //[Route("{packageId}")]
        //public async Task<ServiceResult> Delete([FromRoute] int packageId)
        //{
        //    return await _packageService.DeletePackage(packageId);
        //}

        /// <summary>
        /// Tách kiện
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="req">Danh sách kiện cần tách</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{packageId}/Split")]
        public async Task<bool> Split([FromRoute] int packageId, [FromBody] PackageSplitInput req)
        {
            return await _packageService.SplitPackage(packageId, req);
        }


        /// <summary>
        /// Gộp kiện
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Join")]
        public async Task<long> Join([FromBody] PackageJoinInput req)
        {
            return await _packageService.JoinPackage(req);
        }


        [HttpGet]
        [Route("GetProductPackageListForExport")]
        public async Task<PageData<ProductPackageOutputModel>> GetPackageListForExport([FromQuery] string keyword, [FromQuery] bool? isTwoUnit, [FromQuery] IList<int> productCateIds, [FromQuery] IList<int> productIds, [FromQuery] IList<long> productUnitConversionIds, [FromQuery] IList<long> packageIds, [FromQuery] IList<int> stockIds, [FromQuery] int page, [FromQuery] int size)
        {
            return await _packageService.GetProductPackageListForExport(keyword, isTwoUnit, productCateIds, productIds, productUnitConversionIds, packageIds, stockIds, page, size);
        }
    }
}