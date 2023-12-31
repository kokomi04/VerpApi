﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Report;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.Report;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : VErpBaseController
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        [Route("steps")]
        public async Task<IList<StepModel>> GetSteps([FromQuery] int? monthPlanId, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetSteps(monthPlanId, fromDate, toDate);
        }

        [HttpPost]
        [Route("ProductionProgress")]
        public async Task<IList<StepProgressModel>> GetProductionProgressReport([FromQuery] int? monthPlanId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] int[] stepIds)
        {
            return await _reportService.GetProductionProgressReport(monthPlanId, fromDate, toDate, stepIds);
        }

        [HttpGet]
        [Route("ProductionOrderStepProgress")]
        public async Task<ProductionOrderStepModel> GetProductionOrderStepProgress([FromQuery] int? monthPlanId, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetProductionOrderStepProgress(monthPlanId, fromDate, toDate);
        }

        [HttpGet]
        [Route("ProductionOrder")]
        public async Task<IList<ProductionReportModel>> GetProductionOrderReport([FromQuery] int? monthPlanId, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetProductionOrderReport(monthPlanId, fromDate, toDate);
        }

        [HttpGet]
        [Route("ProcessingOrderList")]
        public async Task<IList<ProcessingOrderListModel>> GetProcessingScheduleList()
        {
            return await _reportService.GetProcessingOrderList();
        }

        [HttpPost]
        [Route("ProductionProgress/{productionOrderId}")]
        public async Task<IList<StepReportModel>> GetProcessingStepReport([FromRoute] long productionOrderId, [FromBody] int[] stepIds)
        {
            return await _reportService.GetProcessingStepReport(productionOrderId, stepIds);
        }

        [HttpGet]
        [Route("OutsourcePartRequest")]
        public async Task<IList<OutsourcePartRequestReportModel>> GetOursourcePartRequestReport([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] long? productionOrderId)
        {
            return await _reportService.GetOursourcePartRequestReport(fromDate, toDate, productionOrderId);
        }

        [HttpPost]
        [Route("OutsourceStepRequest")]
        public async Task<PageData<OutsourceStepRequestReportModel>> GetOursourceStepRequestReport([FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters)
        {
            return await _reportService.GetOursourceStepRequestReport(page, size, orderByFieldName, asc, filters);
        }


        [HttpGet]
        [Route("DailyImport/month/{monthPlanId}/step/{stepId}")]
        public async Task<IDictionary<long, DailyImportModel>> GetDailyImport([FromRoute] long monthPlanId, [FromRoute] int stepId)
        {
            return await _reportService.GetDailyImport(monthPlanId, stepId);
        }


    }
}
