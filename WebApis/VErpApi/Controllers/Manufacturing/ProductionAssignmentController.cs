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
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.StandardEnum;

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
        [Route("{productionOrderId}")]
        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetProductionAssignments(productionOrderId);
        }

        [HttpPut]
        [Route("{productionStepId}/{productionOrderId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute] long productionStepId, [FromRoute] long productionOrderId, [FromBody] ProductionAssignmentInputModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productionAssignmentService.UpdateProductionAssignment(productionStepId, productionOrderId, data.ProductionAssignments, data.ProductionStepWorkInfo, data.DepartmentTimeTable);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute] long productionOrderId, [FromBody] GeneralAssignmentModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productionAssignmentService.UpdateProductionAssignment(productionOrderId, data);
        }

        [HttpPost]
        [Route("DepartmentTimeTable")]
        public async Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable([FromBody] int[] departmentIds, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetDepartmentTimeTable(departmentIds, startDate, endDate);
        }

        [HttpGet]
        [Route("Departments/{departmentId}")]
        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment([FromRoute] int departmentId, [FromQuery] long? productionOrderId, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc)
        {
            return await _productionAssignmentService.DepartmentProductionAssignment(departmentId, productionOrderId, page, size, orderByFieldName, asc);
        }

        [HttpGet]
        [Route("productivity/{productionStepId}")]
        public async Task<IDictionary<int, ProductivityModel>> GetProductivityDepartments([FromRoute] long productionStepId)
        {
            return await _productionAssignmentService.GetProductivityDepartments(productionStepId);
        }

        [HttpGet]
        [Route("{productionOrderId}/capacity/{productionStepId}")]
        public async Task<CapacityOutputModel> GetCapacityDepartments([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacityDepartments(productionOrderId, productionStepId, startDate, endDate);
        }

        [HttpGet]
        [Route("capacity")]
        public async Task<IList<CapacityDepartmentChartsModel>> GetCapacity([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacity(startDate, endDate);
        }

        [HttpGet]
        [Route("{productionOrderId}/WorkInfo")]
        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetListProductionStepWorkInfo(productionOrderId);
        }
    }
}
