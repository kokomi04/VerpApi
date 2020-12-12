using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.System
{

    [Route("api/GenCodeConfigs")]
    public class GenCodeConfigsController : VErpBaseController
    {
        private readonly IGenCodeConfigService _genCodeConfigService;
        public GenCodeConfigsController(IGenCodeConfigService genCodeConfigService)
        {
            _genCodeConfigService = genCodeConfigService;
        }

        [HttpGet("")]
        public async Task<PageData<CustomGenCodeOutputModel>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _genCodeConfigService.GetList(keyword, page, size);
        }

        [HttpGet("{customGenCodeId}")]
        public async Task<CustomGenCodeOutputModel> GetInfo([FromRoute] int customGenCodeId, [FromQuery] long? fId, [FromQuery] string code, [FromQuery] long? date)
        {
            return await _genCodeConfigService.GetInfo(customGenCodeId, fId, code, date);
        }

        [HttpPut("{customGenCodeId}")]
        public async Task<bool> Update([FromRoute] int customGenCodeId, [FromBody] CustomGenCodeInputModel model)
        {
            return await _genCodeConfigService.Update(customGenCodeId, model);
        }

        [HttpPut("{customGenCodeId}/SetLastValue")]
        public async Task<bool> SetLastValue([FromRoute] int customGenCodeId, [FromBody] CustomGenCodeBaseValueModel model)
        {
            return await _genCodeConfigService.SetLastValue(customGenCodeId, model);
        }

        [HttpDelete("{customGenCodeId}/DeleteLastValue")]
        public async Task<bool> DeleteLastValue([FromRoute] int customGenCodeId, [FromBody] CustomGenCodeBaseValueModel model)
        {
            return await _genCodeConfigService.DeleteLastValue(customGenCodeId, model?.BaseValue);
        }

        [HttpDelete("{customGenCodeId}")]
        public async Task<bool> Delete([FromRoute] int customGenCodeId)
        {
            return await _genCodeConfigService.Delete(customGenCodeId);
        }

        [HttpPost("")]
        public async Task<int> Create([FromBody] CustomGenCodeInputModel model)
        {
            return await _genCodeConfigService.Create(model);
        }

        [HttpPost("{customGenCodeId}/GenerateCode")]
        public async Task<CustomCodeGeneratedModel> GenerateCode([FromRoute] int customGenCodeId, [FromQuery] int lastValue, [FromQuery] long? fId, [FromQuery] string code, [FromQuery] long? date)
        {
            return await _genCodeConfigService.GenerateCode(customGenCodeId, lastValue, fId, code, date);
        }

        [HttpPut]
        [Route("{customGenCodeId}/ConfirmCode")]
        public async Task<bool> ConfirmCode([FromRoute] int customGenCodeId, [FromQuery] string baseValue)
        {
            return await _genCodeConfigService.ConfirmCode(customGenCodeId, baseValue);
        }
    }
}