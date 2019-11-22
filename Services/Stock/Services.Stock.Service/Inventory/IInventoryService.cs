﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Inventory    
{
    /// <summary>
    /// I - Nhap xuat kho
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockId">Id kho</param>
        /// <param name="type">Loại typeId: 1 nhập ; 2 : xuất kho theo MasterEnum.EnumInventory</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventory type = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10);

        /// <summary>
        /// Lấy thông tin của phiếu nhập xuất
        /// </summary>
        /// <param name="inventoryId">Mã phiếu</param>
        /// <returns></returns>
        Task<ServiceResult<InventoryOutput>> GetInventory(int inventoryId);

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> AddInventoryInput(int currentUserId, InventoryInput req);

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> AddInventoryOutput(int currentUserId, InventoryInput req);

        /// <summary>
        /// Cập nhật thông tin phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Enum> UpdateInventoryInput(int inventoryId, int currentUserId, InventoryInput model);

        /// <summary>
        /// Cập nhật thông tin phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Enum> UpdateInventoryOutput(int inventoryId, int currentUserId, InventoryInput model);

        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="currentUserId"></param>        
        /// <returns></returns>
        Task<Enum> ApproveInventoryInput(int inventoryId, int currentUserId);


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="currentUserId"></param>        
        /// <returns></returns>
        Task<Enum> ApproveInventoryOutput(int inventoryId, int currentUserId);

        /// <summary>
        /// Xóa thông tin phiếu nhập / xuất kho (đánh dấu xóa)
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        Task<Enum> DeleteInventory(int inventoryId, int currentUserId);

        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductListOutput>> GetProductListForImport(string keyword, int stockId, int page = 1, int size = 20);

        /// <summary>
        /// Lấy danh sách sản phẩm để xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockIdList">Id kho</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductListOutput>> GetProductListForExport(string keyword, IList<int> stockIdList, int page = 1, int size = 20);

        /// <summary>
        /// Lấy danh sách kiện để xuất kho
        /// </summary>
        /// <param name="productId">Id sản phẩm</param>
        /// <param name="stockIdList">List Id kho</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<PackageOutputModel>> GetPackageListForExport(int productId, IList<int> stockIdList, int page = 1, int size = 20);
    }
}
