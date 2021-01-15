using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IList<StepModel>> CreateStep([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetSteps(fromDate, toDate);
        }

        [HttpPost]
        [Route("ProductionProgress")]
        public async Task<IList<StepProgressModel>> GetProductionProgressReport([FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] int[] stepIds)
        {
            return await _reportService.GetProductionProgressReport(fromDate, toDate, stepIds);
        }

        [HttpGet]
        [Route("ProductionOrderStepProgress")]
        public async Task<ProductionOrderStepModel> GetProductionOrderStepProgress([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetProductionOrderStepProgress(fromDate, toDate);
        }

        [HttpGet]
        [Route("ProductionSchedule")]
        public async Task<IList<ProductionScheduleReportModel>> GetProductionScheduleReport([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _reportService.GetProductionScheduleReport(fromDate, toDate);
        }

        [HttpGet]
        [Route("ProcessingScheduleList")]
        public async Task<IList<ProcessingScheduleListModel>> GetProcessingScheduleList()
        {
            return await _reportService.GetProcessingScheduleList();
        }

        [HttpPost]
        [Route("ProductionProgress/{scheduleTurnId}")]
        public async Task<IList<StepReportModel>> GetProcessingStepReport([FromRoute] long scheduleTurnId, [FromBody] int[] stepIds)
        {
            return await _reportService.GetProcessingStepReport(scheduleTurnId, stepIds);
        }

        [HttpGet]
        [Route("OutsourcePartRequest")]
        public async Task<IList<OutsourcePartRequestReportModel>> GetOursourcePartRequestReport([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] long? productionOrderId)
        {
            return await _reportService.GetOursourcePartRequestReport(fromDate, toDate, productionOrderId);
        }
    }
}
