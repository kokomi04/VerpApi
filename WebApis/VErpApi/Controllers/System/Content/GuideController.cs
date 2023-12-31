﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
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
        public async Task<PageData<GuideModelOutput>> GetList([FromQuery] string keyword, [FromQuery] int? guideCateId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _guideService.GetList(keyword, guideCateId, page, size);
        }

        [HttpGet]
        [Route("byCode/{guideCode}")]
        [GlobalApi]
        public async Task<IList<GuideModel>> GetListGuideByCode([FromRoute] string guideCode)
        {
            return await _guideService.GetGuidesByCode(guideCode);
        }

        [HttpGet]
        [Route("{guideId}")]
        public async Task<GuideModel> GetGuideById([FromRoute] int guideId)
        {
            return await _guideService.GetGuideById(guideId);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Create([FromBody] GuideModel model)
        {
            return await _guideService.Create(model);
        }

        [HttpPut]
        [Route("{guideId}")]
        public async Task<bool> Update([FromRoute] int guideId, [FromBody] GuideModel model)
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
