using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.Config;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Stocks
{
    [Route("api/inventoryConfig")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryConfigService _inventoryConfigService;

        public InventoryController(IInventoryConfigService inventoryConfigService)
        {
            _inventoryConfigService = inventoryConfigService;
        }


        [Route("")]
        [HttpPut]
        public Task<bool> UpdateConfig([FromBody] InventoryConfigModel req)
        {
            return _inventoryConfigService.UpdateConfig(req);
        }
    }

}
