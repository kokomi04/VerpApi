using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/InventoryRotation")]
    public class InventoryRotationController : VErpBaseController
    {
        private readonly IInventoryRotationService inventoryRotationService;


        public InventoryRotationController(IInventoryRotationService inventoryRotationService)
        {
            this.inventoryRotationService = inventoryRotationService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddInventoryInput([FromBody] InventoryOutRotationModel req)
        {
            return await inventoryRotationService.Create(req);

        }

        [HttpDelete]
        [Route("{inventoryId}/NotApprovedDelete")]
        public async Task<bool> NotApprovedDelete([FromRoute] long inventoryId)
        {
            return await inventoryRotationService.NotApprovedDelete(inventoryId);
        }

        [HttpDelete]
        [Route("{inventoryId}/ApprovedDelete")]
        public async Task<bool> ApprovedDelete([FromRoute] long inventoryId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] ApprovedInputDataSubmitModel req)
        {
            return await inventoryRotationService.ApprovedDelete(inventoryId, fromDate, toDate, req);
        }

        [HttpPut]
        [Route("{inventoryId}/Approve")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Approve([FromRoute] long inventoryId)
        {
            return await inventoryRotationService.Approve(inventoryId);
        }


        [HttpPut]
        [Route("{inventoryId}/SentToCensor")]
        public async Task<bool> SentToCensor([FromRoute] long inventoryId)
        {
            return await inventoryRotationService.SentToCensor(inventoryId);
        }


        [HttpPut]
        [Route("{inventoryId}/Reject")]
        public async Task<bool> Reject([FromRoute] long inventoryId)
        {
            return await inventoryRotationService.Reject(inventoryId);
        }
    }
}
