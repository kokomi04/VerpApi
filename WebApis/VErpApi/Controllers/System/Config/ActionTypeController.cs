using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/actionType")]
    public class ActionTypeController : VErpBaseController
    {
        private readonly IActionTypeService _actionTypeService;
        public ActionTypeController(IActionTypeService actionTypeService)
        {
            _actionTypeService = actionTypeService;
        }

        [HttpGet]
        [GlobalApi]
        [Route("")]
        public async Task<IList<ActionType>> GetList()
        {
            return await _actionTypeService.GetList().ConfigureAwait(true);
        }      

        [HttpPost]
        [Route("")]
        public async Task<int> Post([FromBody] ActionType model)
        {
            return await _actionTypeService.Create(model);
        }

        [HttpPut]
        [Route("{actionTypeId}")]
        public async Task<bool> Update([FromRoute] int actionTypeId, [FromBody] ActionType model)
        {
            return await _actionTypeService.Update(actionTypeId, model);
        }


        [HttpDelete]
        [Route("{actionTypeId}")]
        public async Task<bool> Delete([FromRoute] int actionTypeId)
        {
            return await _actionTypeService.Delete(actionTypeId);
        }
    }
}