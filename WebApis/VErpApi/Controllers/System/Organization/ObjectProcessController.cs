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

namespace VErpApi.Controllers.System
{
    [Route("api/organization/objectProcess")]
    public class ObjectProcessController : VErpBaseController
    {
        private readonly ObjectProcessService _objectProcessService;

        public ObjectProcessController(ObjectProcessService objectProcessService)
        {
            _objectProcessService = objectProcessService;
        }

        [HttpGet]
        [Route("")]
        public IList<ObjectProcessInfoModel> GetList()
        {
            return _objectProcessService.ObjectProcessList();
        }

        [HttpPut]
        [Route("{objectProcessTypeId}/Steps")]
        public async Task<bool> Update([FromRoute] EnumObjectProcessType objectProcessTypeId, [FromBody] IList<ObjectProcessInfoStepModel> data)
        {
            return await _objectProcessService.ObjectProcessUpdate(objectProcessTypeId, data).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{objectProcessTypeId}/Steps")]
        public async Task<IList<ObjectProcessInfoStepModel>> ObjectProcessSteps([FromRoute] EnumObjectProcessType objectProcessTypeId)
        {
            return await _objectProcessService.ObjectProcessSteps(objectProcessTypeId).ConfigureAwait(true);
        }
    }
}