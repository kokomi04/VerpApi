using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
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

        [Route("stocks")]
        [HttpGet]
        public async Task<ServiceResult<PageData<StockOutput>>> GetStocks([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetList(keyword, page, size);
        }

    }
}