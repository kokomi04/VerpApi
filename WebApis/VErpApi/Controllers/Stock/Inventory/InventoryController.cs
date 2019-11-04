using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.Invetory;

namespace VErpApi.Controllers.Stock.Stocks
{
    [Route("api/inventory")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;
        public InventoryController(IInventoryService iventoryService
            )
        {
            _inventoryService = iventoryService;
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
        public async Task<ApiResponse<PageData<InventoryOutput>>> Get([FromQuery] string keyword, [FromQuery] int stockId,[FromQuery] int type, [FromQuery] int page, [FromQuery] int size)
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

    }
}
