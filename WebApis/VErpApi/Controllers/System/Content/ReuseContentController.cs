using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Model.Guide;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Master.Service.Guide;

namespace VErpApi.Controllers.System
{
    [Route("api/guides")]
    public class ReuseContentController : VErpBaseController
    {
        private readonly IReuseContentService _reuseContentService;

        public ReuseContentController(IReuseContentService reuseContentService)
        {
            _reuseContentService = reuseContentService;
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<IList<ReuseContentModel>> GetList([FromQuery] string key)
        {
            return await _reuseContentService.GetList(key);
        }


        [HttpPost]
        [Route("")]
        [GlobalApi]
        public async Task<long> Create([FromBody] ReuseContentModel model)
        {
            return await _reuseContentService.Create(model);
        }


        [HttpGet]
        [Route("{reuseContentId}")]
        [GlobalApi]
        public async Task<ReuseContentModel> Update([FromRoute] long reuseContentId)
        {
            return await _reuseContentService.Info(reuseContentId);
        }

        [HttpPut]
        [Route("{reuseContentId}")]
        [GlobalApi]
        public async Task<bool> Update([FromRoute] long reuseContentId, ReuseContentModel model)
        {
            return await _reuseContentService.Update(reuseContentId, model);
        }

        [HttpDelete]
        [Route("{reuseContentId}")]
        [GlobalApi]
        public async Task<bool> Deleted([FromRoute] long reuseContentId)
        {
            return await _reuseContentService.Delete(reuseContentId);
        }
    }
}
