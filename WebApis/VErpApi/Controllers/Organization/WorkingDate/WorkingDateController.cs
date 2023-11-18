using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.WorkingDate;
using VErp.Services.Organization.Service.WorkingDate;

namespace VErpApi.Controllers.Organization.WorkingDate
{
    [Route("api/organization/workingdate")]
    public class WorkingDateController : VErpBaseController
    {
        private readonly IWorkingDateService _workingDateService;
        public WorkingDateController(IWorkingDateService workingDateService) {
            _workingDateService = workingDateService;
        }

        [HttpGet("{userId}")]
        public async Task<WorkingDateModel> GetWorkingDateByUser([FromRoute]int userId)
        {
            return await _workingDateService.GetWorkingDateByUserId(userId);
        }

        [HttpPut("")]
        public async Task<bool> UpdateWorkingDateUser([FromBody] WorkingDateModel req)
        {
            return await _workingDateService.Update(req);
        }
        [HttpPost("")]
        public async Task<WorkingDateModel> Create([FromBody]WorkingDateModel model)
        {
            return await _workingDateService.Create(model);
        }
    }
}
