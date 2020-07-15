using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Accountant.Model.Input;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using VErp.Services.Accountant.Service.Input;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/inputTypeGroups")]

    public class InputTypeGroupController : VErpBaseController
    {
        private readonly IInputConfigService _inputConfigService;
        public InputTypeGroupController(IInputConfigService inputConfigService)
        {
            _inputConfigService = inputConfigService;
        }


        [HttpGet]
        [Route("")]
        public async Task<IList<InputTypeGroupList>> GetList()
        {
            return await _inputConfigService.InputTypeGroupList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> InputTypeGroupCreate([FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigService.InputTypeGroupCreate(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeGroupId}")]
        public async Task<bool> GetInputType([FromRoute] int inputTypeGroupId, [FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigService.InputTypeGroupUpdate(inputTypeGroupId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeGroupId}")]
        public async Task<bool> DeleteInputType([FromRoute] int inputTypeGroupId)
        {
            return await _inputConfigService.InputTypeGroupDelete(inputTypeGroupId).ConfigureAwait(true);
        }        
    }
}