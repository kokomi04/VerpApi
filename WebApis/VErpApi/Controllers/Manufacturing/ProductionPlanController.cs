using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Model.WorkloadPlanModel;
using VErp.Services.Manafacturing.Service.ProductionPlan;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanController : VErpBaseController
    {
        private readonly IProductionPlanService _productionPlanService;

        public ProductionPlanController(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
        }

        [HttpGet]
        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan([FromQuery] int? monthPlanId, [FromQuery] int? factoryDepartmentId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionPlanService.GetProductionPlan(monthPlanId, factoryDepartmentId, startDate, endDate);
        }

        [HttpPost]
        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan([FromBody] IDictionary<long, List<ProductionWeekPlanModel>> data)
        {
            return await _productionPlanService.UpdateProductionPlan(data);
        }

        [HttpDelete]
        [Route("{productionOrderId}")]
        public async Task<bool> DeleteProductionPlan([FromBody] long productionOrderId)
        {
            return await _productionPlanService.DeleteProductionPlan(productionOrderId);
        }

        [HttpPost]
        [Route("export")]
        public async Task<FileStreamResult> ProductionPlanExport([FromQuery] int? monthPlanId, [FromQuery] int? factoryDepartmentId, [FromQuery] long startDate, [FromQuery] long endDate, [FromBody] ProductionPlanExportModel data)
        {
            var (stream, fileName, contentType) = await _productionPlanService.ProductionPlanExport(monthPlanId, factoryDepartmentId, startDate, endDate, data);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };

        }

        [HttpPost]
        [Route("workload")]
        public async Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlan([FromBody] IList<long> productionOrderIds)
        {
            return await _productionPlanService.GetWorkloadPlan(productionOrderIds);
        }

        [HttpGet]
        [Route("workloadByDate")]
        public async Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlanByDate([FromQuery] int? monthPlanId, [FromQuery] int? factoryDepartmentId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionPlanService.GetWorkloadPlanByDate(monthPlanId, factoryDepartmentId, startDate, endDate);
        }

        [HttpGet]
        [Route("monthlyImportStock")]
        public async Task<IDictionary<long, List<ImportProductModel>>> GetMonthlyImportStock([FromQuery] int monthPlanId)
        {
            return await _productionPlanService.GetMonthlyImportStock(monthPlanId);
        }

        [HttpGet]
        [Route("workload/export")]
        public async Task<FileStreamResult> ProductionWorkloadPlanExport([FromQuery] int monthPlanId, [FromQuery] int? factoryDepartmentId, [FromQuery] long startDate, [FromQuery] long endDate, [FromQuery] string monthPlanName)
        {
            var (stream, fileName, contentType) = await _productionPlanService.ProductionWorkloadPlanExport(monthPlanId, factoryDepartmentId, startDate, endDate, monthPlanName);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };

        }
    }
}
