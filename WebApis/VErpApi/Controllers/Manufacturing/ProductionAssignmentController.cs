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
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionAssignmentController : VErpBaseController
    {
        private readonly IProductionAssignmentService _productionAssignmentService;

        public ProductionAssignmentController(IProductionAssignmentService productionAssignmentService)
        {
            _productionAssignmentService = productionAssignmentService;
        }

        [HttpGet]
        [Route("{scheduleTurnId}")]
        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments([FromRoute] long scheduleTurnId)
        {
            return await _productionAssignmentService.GetProductionAssignments(scheduleTurnId);
        }

        [HttpPut]
        [Route("{productionStepId}/{scheduleTurnId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute] long productionStepId, [FromRoute] long scheduleTurnId, [FromBody] ProductionAssignmentInputModel data)
        {
            return await _productionAssignmentService.UpdateProductionAssignment(productionStepId, scheduleTurnId, data.ProductionAssignments, data.ProductionStepWorkInfo);
        }

        [HttpGet]
        [Route("Departments/{departmentId}")]
        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment([FromRoute] int departmentId, [FromQuery] long? scheduleTurnId, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc)
        {
            return await _productionAssignmentService.DepartmentProductionAssignment(departmentId, scheduleTurnId, page, size, orderByFieldName, asc);
        }

        [HttpGet]
        [Route("productivity/{productionStepId}")]
        public async Task<IDictionary<int, decimal>> GetProductivityDepartments([FromRoute] long productionStepId)
        {
            return await _productionAssignmentService.GetProductivityDepartments(productionStepId);
        }

        [HttpGet]
        [Route("{scheduleTurnId}/capacity/{productionStepId}")]
        public async Task<IDictionary<int, List<CapacityModel>>> GetCapacityDepartments([FromRoute] long scheduleTurnId, [FromRoute] long productionStepId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacityDepartments(scheduleTurnId, productionStepId, startDate, endDate);
        }

        [HttpGet]
        [Route("capacity")]
        public async Task<IList<CapacityDepartmentChartsModel>> GetCapacity([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacity(startDate, endDate);
        }

        [HttpGet]
        [Route("{scheduleTurnId}/WorkInfo")]
        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo([FromRoute] long scheduleTurnId)
        {
            return await _productionAssignmentService.GetListProductionStepWorkInfo(scheduleTurnId);
        }
    }
}
