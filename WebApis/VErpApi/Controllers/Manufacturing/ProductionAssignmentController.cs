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
        [Route("productionOrder/{productionOrderId}")]
        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetProductionAssignments(productionOrderId);
        }

        [HttpGet]
        [Route("productionOrder/{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}")]
        public async Task<ProductionAssignmentModel> GetProductionAssignment([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] int departmentId)
        {
            return await _productionAssignmentService.GetProductionAssignment(productionOrderId, productionStepId, departmentId);
        }

        [HttpPut]
        [Route("productionOrder/{productionOrderId}/productionStep/{productionStepId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromBody] ProductionAssignmentInputModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productionAssignmentService.UpdateProductionAssignment(productionOrderId, productionStepId, data.ProductionAssignments, data.ProductionStepWorkInfo, data.DepartmentTimeTable);
        }

        [HttpPut]
        [Route("productionOrder/{productionOrderId}")]
        public async Task<bool> UpdateProductionAssignment([FromRoute] long productionOrderId, [FromBody] GeneralAssignmentModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productionAssignmentService.UpdateProductionAssignment(productionOrderId, data);
        }

        [HttpPut]
        [Route("productionOrder/{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}/status/{status}")]
        public async Task<bool> ChangeAssignedProgressStatus([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] int departmentId, EnumAssignedProgressStatus status)
        {
            return await _productionAssignmentService.ChangeAssignedProgressStatus(productionOrderId, productionStepId, departmentId, status);
        }

        [HttpPost]
        [Route("DepartmentTimeTable")]
        public async Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable([FromBody] int[] departmentIds, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetDepartmentTimeTable(departmentIds, startDate, endDate);
        }

        [HttpGet]
        [Route("departments/{departmentId}")]
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
        [Route("productivity/general/{productionOrderId}")]
        public async Task<IDictionary<long, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetGeneralProductivityDepartments(productionOrderId);
        }

        [HttpGet]
        [Route("capacity/productionOrder/{productionOrderId}/productionStep/{productionStepId}")]
        public async Task<CapacityOutputModel> GetCapacityDepartments([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacityDepartments(productionOrderId, productionStepId, startDate, endDate);
        }

        [HttpGet]
        [Route("capacity/general/productionOrder/{productionOrderId}")]
        public async Task<CapacityOutputModel> GetGenaralCapacityDepartments([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetGeneralCapacityDepartments(productionOrderId);
        }

        [HttpGet]
        [Route("capacity")]
        public async Task<IList<CapacityDepartmentChartsModel>> GetCapacity([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionAssignmentService.GetCapacity(startDate, endDate);
        }

        [HttpGet]
        [Route("WorkInfo/productionOrder/{productionOrderId}")]
        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetListProductionStepWorkInfo(productionOrderId);
        }
    }
}
