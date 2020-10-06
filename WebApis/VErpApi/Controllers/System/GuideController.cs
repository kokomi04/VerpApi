using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Guide;
using VErp.Services.Master.Service.Guide;

namespace VErpApi.Controllers.System
{
    [Route("api/guides")]
    public class GuideController : VErpBaseController
    {
        private readonly IGuideService _guideService;

        public GuideController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        [HttpGet]
        [Route("")]
        public async Task<List<GuideModel>> GetList()
        {
            return await _guideService.GetList();
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Create([FromBody] GuideModel model)
        {
            return await _guideService.Create(model);
        }

        [HttpPut]
        [Route("{guideId}")]
        public async Task<bool> Update([FromRoute] int guideId, GuideModel model)
        {
            return await _guideService.Update(guideId, model);
        }

        [HttpDelete]
        [Route("{guideId}")]
        public async Task<bool> Deleted([FromRoute] int guideId)
        {
            return await _guideService.Deleted(guideId);
        }
    }
}
