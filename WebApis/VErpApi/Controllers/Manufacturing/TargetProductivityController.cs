using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
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
        [Route("{targetProductivityId}")]
        public async Task<bool> DeleteTargetProductivity([FromRoute] int targetProductivityId)
        {
            return await _targetProductivityService.DeleteTargetProductivity(targetProductivityId);
        }

        [HttpGet]
        [Route("{targetProductivityId}")]
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
        [Route("{targetProductivityId}")]
        public async Task<bool> UpdateTargetProductivity([FromRoute] int targetProductivityId, [FromBody] TargetProductivityModel model)
        {
            return await _targetProductivityService.UpdateTargetProductivity(targetProductivityId, model);
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetFieldDataForMapping()
        {
            return _targetProductivityService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("parseDetailsFromExcelMapping")]
        [VErpAction(EnumActionType.View)]
        public Task<IList<TargetProductivityDetailModel>> ParseDetails([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return _targetProductivityService.ParseDetails(mapping, file.OpenReadStream());
        }
    }
}