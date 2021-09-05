using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductBomController : CrossServiceBaseController
    {
        private readonly IProductBomService _productBomService;
        public InternalProductBomController(IProductBomService productBomService)
        {
            _productBomService = productBomService;
        }


        [HttpPost]
        [Route("products")]
        public async Task<IList<ProductElementModel>> GetElements([FromBody] int[] productIds)
        {
            return await _productBomService.GetProductElements(productIds);
        }
    }
}