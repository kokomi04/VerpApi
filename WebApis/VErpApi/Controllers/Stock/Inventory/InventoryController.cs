using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.Invetory;
using VErp.Services.Stock.Service.FileResources;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;

namespace VErpApi.Controllers.Stock.Stocks
{
    [Route("api/inventory")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IFileService _fileService;
        public InventoryController(IInventoryService iventoryService,IFileService fileService
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
        public async Task<ApiResponse<PageData<InventoryOutput>>> Get([FromQuery] string keyword, [FromQuery] int stockId,[FromQuery] EnumInventory type, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetList(keyword: keyword, stockId: stockId,type: type, page: page, size: size);
        }


        /// <summary>
        /// Thêm mới phiếu nhập/xuất kho
        /// </summary>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> AddInventory([FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;
            return await _inventoryService.AddInventory(currentUserId, inventoryInput);
        }

        /// <summary>
        /// Cập nhật phiếu nhập/xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]        
        [Route("{inventoryId}")]
        public async Task<ApiResponse> UpdateInventory([FromRoute] int inventoryId, [FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;
            return await _inventoryService.UpdateInventory(inventoryId, currentUserId, inventoryInput);
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
    }
}
