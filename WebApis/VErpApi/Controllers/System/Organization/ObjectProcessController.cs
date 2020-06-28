using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Organization.Model.Department;
using Services.Organization.Service.Department;
using Services.Organization.Model.Deparment;
using Services.Organization.Service.BusinessInfo.Implement;
using Services.Organization.Model.BusinessInfo;
using System.Collections.Generic;
using Services.Organization.Service.BusinessInfo;

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