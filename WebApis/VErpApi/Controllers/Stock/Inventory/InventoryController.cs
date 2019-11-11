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
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory;

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
        public async Task<ApiResponse<PageData<InventoryOutput>>> Get([FromQuery] string keyword, [FromQuery] int stockId, [FromQuery] EnumInventory type, [FromQuery] int page, [FromQuery] int size)
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
        public async Task<ApiResponse<InventoryOutput>> GetInventory([FromRoute] int inventoryId)
        {
            return await _inventoryService.GetInventory(inventoryId);
        }

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> AddInventoryInput([FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;

            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Input:
                    return await _inventoryService.AddInventoryInput(currentUserId, inventoryInput);
                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    }; 
                    return response;
                    
            }
        }

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> AddInventoryOutput([FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;

            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Output:
                    return await _inventoryService.AddInventoryOutput(currentUserId, inventoryInput);
                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    };
                    return response;

            }
        }


        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{inventoryId}")]
        public async Task<ApiResponse> UpdateInventoryInput([FromRoute] int inventoryId, [FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;
            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Input:
                    return await _inventoryService.UpdateInventoryInput(inventoryId, currentUserId, inventoryInput);

                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    };
                    return response;
            }
        }


        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{inventoryId}")]
        public async Task<ApiResponse> UpdateInventoryOutput([FromRoute] int inventoryId, [FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;
            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Output:
                    return await _inventoryService.UpdateInventoryOutput(inventoryId, currentUserId, inventoryInput);
                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    };
                    return response;
            }
        }


        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{inventoryId}")]
        public async Task<ApiResponse> ApproveInventoryInput([FromRoute] int inventoryId, [FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;

            if (!inventoryInput.IsApproved)
            {
                var response = new ApiResponse<long>()
                {
                    Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                    Data = 0,
                    Message = "Tham số trạng thái duyệt không hợp lệ"
                };
                return response;
            }

            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Input:
                    return await _inventoryService.UpdateInventoryInput(inventoryId, currentUserId, inventoryInput);

                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    };
                    return response;
            }
        }


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="inventoryInput">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{inventoryId}")]
        public async Task<ApiResponse> ApproveInventoryOutput([FromRoute] int inventoryId, [FromBody] InventoryInput inventoryInput)
        {
            var currentUserId = UserId;

            if (!inventoryInput.IsApproved)
            {
                var response = new ApiResponse<long>()
                {
                    Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                    Data = 0,
                    Message = "Tham số trạng thái duyệt không hợp lệ"
                };
                return response;
            }

            switch (inventoryInput.InventoryTypeId)
            {
                case (int)EnumInventory.Output:
                    return await _inventoryService.UpdateInventoryOutput(inventoryId, currentUserId, inventoryInput);
                default:
                    var response = new ApiResponse<long>()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Data = 0,
                        Message = "Tham số loại phiếu không hợp lệ"
                    };
                    return response;
            }
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
