﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionPlan;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Model.WorkloadPlanModel;

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
        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionPlanService.GetProductionPlan(startDate, endDate);
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
        public async Task<FileStreamResult> ProductionPlanExport([FromQuery] long startDate, [FromQuery] long endDate, [FromBody] ProductionPlanExportModel data)
        {
            var (stream, fileName, contentType) = await _productionPlanService.ProductionPlanExport(startDate, endDate, data);

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
        public async Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlanByDate([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionPlanService.GetWorkloadPlanByDate(startDate, endDate);
        }

        [HttpGet]
        [Route("monthlyImportStock")]
        public async Task<IDictionary<long, List<ImportProductModel>>> GetMonthlyImportStock([FromQuery] int monthPlanId)
        {
            return await _productionPlanService.GetMonthlyImportStock(monthPlanId);
        }

    }
}
