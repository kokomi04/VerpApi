﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalStockController : CrossServiceBaseController
    {
        private readonly IStockService _stockService;
        public InternalStockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<StockOutput>> GetStocks([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetAll(keyword, page, size, filters);
        }

        [HttpGet]
        [Route("{stockId}")]
        public async Task<StockOutput> StockInfo([FromRoute] int stockId)
        {
            return await _stockService.StockInfo(stockId);
        }
    }
}