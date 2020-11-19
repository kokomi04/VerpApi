using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Commons.Enums.Manafacturing;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionAssignmentController : ControllerBase
    {
        private readonly IProductionAssignmentService _productionAssignmentService;

        public ProductionAssignmentController(IProductionAssignmentService productionAssignmentService)
        {
            _productionAssignmentService = productionAssignmentService;
        }

        [HttpGet]
        [Route("{scheduleTurnId}")]
        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments([FromRoute]long scheduleTurnId)
        {
            return await _productionAssignmentService.GetProductionAssignments(scheduleTurnId);
        }

        [HttpPut]
        [Route("{productionStepId}/{scheduleTurnId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute]long productionStepId, [FromRoute]long scheduleTurnId, [FromBody] ProductionAssignmentModel[] data)
        {
            return await _productionAssignmentService.UpdateProductionAssignment(productionStepId, scheduleTurnId, data);
        }
    }
}
