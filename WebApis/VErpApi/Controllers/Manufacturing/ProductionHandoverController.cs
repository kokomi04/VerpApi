using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionHandoverController : VErpBaseController
    {
        private readonly IProductionHandoverService _productionHandoverService;

        public ProductionHandoverController(IProductionHandoverService productionHandoverService)
        {
            _productionHandoverService = productionHandoverService;
        }

        [HttpGet]
        [Route("{scheduleTurnId}")]
        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers([FromRoute] long scheduleTurnId)
        {
            return await _productionHandoverService.GetProductionHandovers(scheduleTurnId);
        }

        [HttpPost]
        [Route("{scheduleTurnId}")]
        public async Task<ProductionHandoverModel> CreateProductionHandover([FromRoute] long scheduleTurnId, [FromBody] ProductionHandoverInputModel data)
        {
            return await _productionHandoverService.CreateProductionHandover(scheduleTurnId, data);
        }

        [HttpPut]
        [Route("{scheduleTurnId}/{productionHandoverId}/accept")]
        public async Task<ProductionHandoverModel> AcceptProductionHandover([FromRoute] long scheduleTurnId, [FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.ConfirmProductionHandover(scheduleTurnId, productionHandoverId, EnumHandoverStatus.Accept);
        }

        [HttpPut]
        [Route("{scheduleTurnId}/{productionHandoverId}/reject")]
        public async Task<ProductionHandoverModel> RejectProductionHandover([FromRoute] long scheduleTurnId, [FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.ConfirmProductionHandover(scheduleTurnId, productionHandoverId, EnumHandoverStatus.Reject);
        }
    }
}
