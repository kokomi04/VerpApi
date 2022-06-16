using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.BusinessInfo;
using Services.Organization.Service.BusinessInfo;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/objectProcess")]
    public class ObjectProcessController : VErpBaseController
    {
        private readonly IObjectProcessService _objectProcessService;

        public ObjectProcessController(IObjectProcessService objectProcessService)
        {
            _objectProcessService = objectProcessService;
        }

        [HttpGet]
        [Route("")]
        public IList<ObjectProcessInfoModel> GetList()
        {
            return _objectProcessService.ObjectProcessList();
        }


        [HttpGet]
        [Route("{objectProcessTypeId}/Steps")]
        public async Task<IList<ObjectProcessInfoStepListModel>> ObjectProcessSteps([FromRoute] EnumObjectProcessType objectProcessTypeId)
        {
            return await _objectProcessService.ObjectProcessSteps(objectProcessTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{objectProcessTypeId}/Steps")]
        public async Task<int> ObjectProcessStepCreate([FromRoute] EnumObjectProcessType objectProcessTypeId, [FromBody] ObjectProcessInfoStepModel model)
        {
            return await _objectProcessService.ObjectProcessStepCreate(objectProcessTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{objectProcessTypeId}/Steps/{objectProcessStepId}")]
        public async Task<bool> ObjectProcessStepUpdate([FromRoute] EnumObjectProcessType objectProcessTypeId, [FromRoute] int objectProcessStepId, [FromBody] ObjectProcessInfoStepModel model)
        {
            return await _objectProcessService.ObjectProcessStepUpdate(objectProcessTypeId, objectProcessStepId, model).ConfigureAwait(true);
        }


        [HttpDelete]
        [Route("{objectProcessTypeId}/Steps/{objectProcessStepId}")]
        public async Task<bool> ObjectProcessStepDelete([FromRoute] EnumObjectProcessType objectProcessTypeId, [FromRoute] int objectProcessStepId)
        {
            return await _objectProcessService.ObjectProcessStepDelete(objectProcessTypeId, objectProcessStepId).ConfigureAwait(true);
        }
    }
}