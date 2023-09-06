using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/inventoryInput")]
    public class InventoryInputController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryInputController(
            IInventoryService iventoryService,
            IInventoryBillInputService inventoryBillInputService,
            IInventoryBillOutputService inventoryBillOutputService,
            IFileService fileService
            )
        {
            _inventoryService = iventoryService;
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetListDetails")]
        public async Task<PageData<InventoryListProductOutput>> Get([FromQuery] string keyword,
            [FromQuery] int? customerId,
            [FromQuery] IList<int> productIds,
            [FromQuery] int stockId,
            [FromQuery] int? inventoryStatusId,
            [FromQuery] long? beginTime,
            [FromQuery] long? endTime,
            [FromQuery] bool? isInputBillCreated,
            [FromQuery] string sortBy,
            [FromQuery] bool asc,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] int? inventoryActionId,
            [FromBody] Clause filters = null)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "date";

            return await _inventoryService.GetListDetails(keyword, customerId, productIds, stockId, inventoryStatusId, EnumInventoryType.Input, beginTime, endTime, isInputBillCreated, sortBy, asc, page, size, inventoryActionId, filters).ConfigureAwait(true);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetInfosByIds")]
        public async Task<IList<InventoryOutput>> GetInfosByIds([FromBody] IList<long> inventoryIds)
        {
            return await _inventoryService.GetInfosByIds(inventoryIds, EnumInventoryType.Input);
        }
    }
}
