using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionAssignment;

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

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetByProductionOrders")]
        public async Task<IList<ProductionAssignmentModel>> GetByProductionOrders([FromBody] IList<long> productionOrderIds)
        {
            return await _productionAssignmentService.GetByProductionOrders(productionOrderIds);
        }


        [HttpGet]
        [Route("GetByDateRange")]
        public async Task<IList<ProductionAssignmentModel>> GetByDateRange([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _productionAssignmentService.GetByDateRange(fromDate, toDate);
        }


        [HttpGet]
        [Route("productionOrder/{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}")]
        public async Task<ProductionAssignmentModel> GetProductionAssignment([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] int departmentId)
        {
            return await _productionAssignmentService.GetProductionAssignment(productionOrderId, productionStepId, departmentId);
        }

        //[HttpPut]
        //[Route("productionOrder/{productionOrderId}/productionStep/{productionStepId}")]
        //public async Task<bool> UpdateProductionAssignment([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromBody] ProductionAssignmentInputModel data)
        //{
        //    if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
        //    return await _productionAssignmentService.UpdateProductionAssignment(productionOrderId, productionStepId, data.ProductionAssignments, data.ProductionStepWorkInfo);
        //}

        [HttpPut]
        [Route("productionOrder/{productionOrderId}/dismissWarning")]
        public async Task<bool> DismissUpdateWarning([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.DismissUpdateWarning(productionOrderId);
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

        //[HttpPost]
        //[Route("DepartmentTimeTable")]
        //public async Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable([FromBody] int[] departmentIds, [FromQuery] long startDate, [FromQuery] long endDate)
        //{
        //    return await _productionAssignmentService.GetDepartmentTimeTable(departmentIds, startDate, endDate);
        //}

        [HttpGet]
        [Route("departments/{departmentId}")]
        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment([FromRoute] int departmentId, [FromQuery] string keyword, [FromQuery] long? productionOrderId, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, long? fromDate, long? toDate)
        {
            return await _productionAssignmentService.DepartmentProductionAssignment(departmentId, keyword, productionOrderId, page, size, orderByFieldName, asc, fromDate, toDate);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("DepartmentsFreeDates")]
        public async Task<IList<DepartmentAssignFreeDate>> DepartmentsFreeDates([FromBody] DepartmentAssignFreeDateInput req)
        {
            return await _productionAssignmentService.DepartmentsFreeDates(req);
        }      


        [HttpPut]
        [Route("departments/{departmentId}/AssignDate")]
        public async Task<bool> UpdateDepartmentAssignmentDate([FromRoute] int departmentId, IList<DepartmentAssignUpdateDateModel> data)
        {
            return await _productionAssignmentService.UpdateDepartmentAssignmentDate(departmentId, data);
        }

        //Task<bool> UpdateDepartmentAssignmentDate(int departmentId, IList<DepartmentAssignUpdateDateModel> data)
        //[HttpGet]
        //[Route("productivity/productionStep/{productionStepId}")]
        //public async Task<IDictionary<int, ProductivityModel>> GetProductivityDepartments([FromRoute] long productionStepId)
        //{
        //    return await _productionAssignmentService.GetProductivityDepartments(productionStepId);
        //}

        [HttpGet]
        [Route("productivity/general")]
        public async Task<IDictionary<int, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments()
        {
            return await _productionAssignmentService.GetGeneralProductivityDepartments();
        }

        //[HttpGet]
        //[Route("capacity/productionOrder/{productionOrderId}/productionStep/{productionStepId}")]
        //public async Task<CapacityOutputModel> GetCapacityDepartments([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromQuery] long startDate, [FromQuery] long endDate)
        //{
        //    return await _productionAssignmentService.GetCapacityDepartments(productionOrderId, productionStepId, startDate, endDate);
        //}

        [HttpGet]
        [Route("capacity/general/productionOrder/{productionOrderId}")]
        public async Task<CapacityOutputModel> GetGenaralCapacityDepartments([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetGeneralCapacityDepartments(productionOrderId);
        }

        //[HttpGet]
        //[Route("capacity")]
        //public async Task<IList<CapacityDepartmentChartsModel>> GetCapacity([FromQuery] long startDate, [FromQuery] long endDate)
        //{
        //    return await _productionAssignmentService.GetCapacity(startDate, endDate);
        //}

        [HttpGet]
        [Route("WorkInfo/productionOrder/{productionOrderId}")]
        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo([FromRoute] long productionOrderId)
        {
            return await _productionAssignmentService.GetListProductionStepWorkInfo(productionOrderId);
        }
    }
}
