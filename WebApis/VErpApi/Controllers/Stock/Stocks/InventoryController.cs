using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/inventory")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IFileService _fileService;
        public InventoryController(IInventoryService iventoryService, IFileService fileService
            )
        {
            _inventoryService = iventoryService;
            _fileService = fileService;
        }

        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockId">Id kho</param>
        /// <param name="type">Loại InventoryTypeId: 1 nhập ; 2 : xuất kho theo MasterEnum.EnumInventory</param>        
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<InventoryOutput>>> Get([FromQuery] string keyword, [FromQuery] int stockId, [FromQuery] EnumInventoryType type, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetList(keyword: keyword, stockId: stockId, type: type, page: page, size: size);
        }


        /// <summary>
        /// Lấy thông tin phiếu nhập / xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu</param>
        /// <returns>InventoryOutput</returns>
        [HttpGet]
        [Route("{inventoryId}")]
        public async Task<ApiResponse<InventoryOutput>> GetInventory([FromRoute] long inventoryId)
        {
            return await _inventoryService.GetInventory(inventoryId);
        }

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="req">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInventoryInput")]
        public async Task<ApiResponse<long>> AddInventoryInput([FromBody] InventoryInModel req)
        {
            return await _inventoryService.AddInventoryInput(UserId, req);

        }

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="req">Model InventoryOutput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInventoryOutput")]
        public async Task<ApiResponse<long>> AddInventoryOutput([FromBody] InventoryOutModel req)
        {
            return await _inventoryService.AddInventoryOutput(UserId, req);

        }


        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="req">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateInventoryInput/{inventoryId}")]
        public async Task<ApiResponse> UpdateInventoryInput([FromRoute] long inventoryId, [FromBody] InventoryInModel req)
        {
            return await _inventoryService.UpdateInventoryInput(inventoryId, UserId, req);
        }


        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="req">Model InventoryOutput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateInventoryOutput/{inventoryId}")]
        public async Task<ApiResponse> UpdateInventoryOutput([FromRoute] long inventoryId, [FromBody] InventoryOutModel req)
        {
            return await _inventoryService.UpdateInventoryOutput(inventoryId, UserId, req);
        }

        /// <summary>
        /// Xóa phiếu nhập/xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="type">EnumInventoryType: 1-phiếu nhập kho ; 2-phiếu xuất kho</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{inventoryId}")]
        public async Task<ApiResponse> Delete([FromRoute] long inventoryId, [FromQuery] EnumInventoryType type)
        {
            var currentUserId = UserId;
            switch (type)
            {
                case EnumInventoryType.Input:
                    return await _inventoryService.DeleteInventoryInput(inventoryId, currentUserId);
                    
                case EnumInventoryType.Output:
                    return await _inventoryService.DeleteInventoryOutput(inventoryId, currentUserId);                
            }
            var result = new ApiResponse { Code = GeneralCode.InvalidParams.ToString(), Message = GeneralCode.InvalidParams.GetEnumDescription() };
            return result;            
        }

        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <returns></returns>
        [HttpPut]
        [Route("ApproveInventoryInput/{inventoryId}")]
        public async Task<ApiResponse> ApproveInventoryInput([FromRoute] long inventoryId)
        {
            var currentUserId = UserId;
            return await _inventoryService.ApproveInventoryInput(inventoryId, currentUserId);
        }


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <returns></returns>
        [HttpPut]
        [Route("ApproveInventoryOutput/{inventoryId}")]
        public async Task<ApiResponse> ApproveInventoryOutput([FromRoute] long inventoryId)
        {
            var currentUserId = UserId;
            return await _inventoryService.ApproveInventoryOutput(inventoryId, currentUserId);
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<ApiResponse<long>> UploadImage([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.Inventory, fileTypeId, string.Empty, file);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductListForImport")]
        public async Task<ApiResponse<PageData<ProductListOutput>>> GetProductListForImport([FromQuery] string keyword, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetProductListForImport(keyword: keyword, stockIdList: stockIdList, page: page, size: size);
        }


        /// <summary>
        /// Lấy danh sách sản phẩm để xuất kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductListForExport")]
        public async Task<ApiResponse<PageData<ProductListOutput>>> GetProductListForExport([FromQuery] string keyword, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetProductListForExport(keyword: keyword, stockIdList: stockIdList, page: page, size: size);
        }

        /// <summary>
        /// Lấy danh sách kiện để xuất kho
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPackageListForExport")]
        public async Task<ApiResponse<PageData<PackageOutputModel>>> GetPackageListForExport([FromQuery] int productId, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetPackageListForExport(productId: productId, stockIdList: stockIdList, page: page, size: size);
        }


        /// <summary>
        /// Xử lý file - Đọc và tạo chứng từ tồn đầu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessOpeningBalance")]
        public async Task<ApiResponse> ProcessOpeningBalance([FromBody] InventoryOpeningBalanceInputModel model)
        {
            var currentUserId = UserId;
            return await _inventoryService.ProcessOpeningBalance(currentUserId, model);
        }
        
    }
}
