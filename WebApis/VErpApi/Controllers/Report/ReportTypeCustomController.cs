using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.Report
{
    [Route("api/reportTypeCustom")]
    public class ReportTypeCustomController : VErpBaseController
    {
        private readonly IReportTypeCustomService _reportTypeCustomService;
        public ReportTypeCustomController(IReportTypeCustomService reportTypeCustomService)
        {
            _reportTypeCustomService = reportTypeCustomService;
        }

        [HttpGet("")]
        public async Task<ReportTypeCustomModel> GetInfo(int reportTypeId)
        {
            return await _reportTypeCustomService.InfoReportTypeCustom(reportTypeId);
        }
        [HttpPost("create")]
        public async Task<int> CreateReportTypeCustom([FromBody] ReportTypeCustomModel data)
        {
            return await _reportTypeCustomService.AddReportTypeCustom(data);
        }
        [HttpPut("{reportTypeId}")]
        public async Task<int> UpdateReportTypeCustom([FromRoute] int reportTypeId, [FromBody] ReportTypeCustomModel data)
        {
            return await _reportTypeCustomService.UpdateReportTypeCustom(reportTypeId, data);
        }
        [HttpPost("{reportTypeId}")]
        public async Task<bool> DeleteReportTypeCustom([FromRoute] int reportTypeId)
        {
            return await _reportTypeCustomService.DeleteReportTypeCustom(reportTypeId);
        }
    }
}
