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
using VErp.Services.Organization.Model.BusinessInfo;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/objectApprovalStep")]
    public class ObjectApprovalStepController : VErpBaseController
    {
        private readonly IObjectApprovalStepService _objectApprovalStepService ;

        public ObjectApprovalStepController(IObjectApprovalStepService objectApprovalStepService)
        {
            _objectApprovalStepService  = objectApprovalStepService;
        }


        
        [HttpGet]
        [Route("items")]
        public async Task<IList<ObjectApprovalStepItemModel>> GetAllObjectApprovalStepItem()
        {
            return await _objectApprovalStepService.GetAllObjectApprovalStepItem();
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<ObjectApprovalStepModel>> GetObjectApprovalStep([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _objectApprovalStepService.GetObjectApprovalStep(objectTypeId, objectId);
        }
        
        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateObjectApprovalStep(ObjectApprovalStepModel model)
        {
            return await _objectApprovalStepService.UpdateObjectApprovalStep(model);
        }
    }
}