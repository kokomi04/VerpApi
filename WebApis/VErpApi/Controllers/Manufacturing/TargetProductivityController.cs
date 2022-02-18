using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/TargetProductivity")]
    [ApiController]
    public class TargetProductivityController : VErpBaseController
    {
        private readonly ITargetProductivityService _targetProductivityService;

        public TargetProductivityController(ITargetProductivityService targetProductivityService)
        {
            _targetProductivityService = targetProductivityService;
        }


        [HttpPost]
        [Route("")]
        public async Task<int> AddTargetProductivity([FromBody] TargetProductivityModel model)
        {
            return await _targetProductivityService.AddTargetProductivity(model);
        }

        [HttpDelete]
        [Route("{targeProductivityId}")]
        public async Task<bool> DeleteTargetProductivity([FromRoute] int targetProductivityId)
        {
            return await _targetProductivityService.DeleteTargetProductivity(targetProductivityId);
        }

        [HttpGet]
        [Route("{targeProductivityId}")]
        public async Task<TargetProductivityModel> GetTargetProductivity([FromRoute] int targetProductivityId)
        {
            return await _targetProductivityService.GetTargetProductivity(targetProductivityId);
        }

        [HttpPost]
        [Route("search")]
        public async Task<IList<TargetProductivityModel>> Search([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _targetProductivityService.Search(keyword, page, size);
        }

        [HttpPut]
        [Route("{targeProductivityId}")]
        public async Task<bool> UpdateTargetProductivity([FromRoute] int targetProductivityId, [FromBody] TargetProductivityModel model)
        {
            return await _targetProductivityService.UpdateTargetProductivity(targetProductivityId, model);
        }
    }
}