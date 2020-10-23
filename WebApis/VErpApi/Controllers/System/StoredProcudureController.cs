using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.StoredProcedure;
using VErp.Services.Master.Service.StoredProcedure;

namespace VErpApi.Controllers.System
{
    [Route("api/[controller]")]
    public class StoredProcuduresController : VErpBaseController
    {
        private readonly IStoredProcedureService _storedProcedureService;

        public StoredProcuduresController(IStoredProcedureService storedProcedureService)
        {
            _storedProcedureService = storedProcedureService;
        }

        [HttpGet()]
        [Route("{moduleTypeId}")]
        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList([FromRoute] EnumModuleType moduleTypeId)
        {
            return await _storedProcedureService.GetList(moduleTypeId);
        }

        [HttpPost]
        [Route("{moduleTypeId}/{type}")]
        public async Task<bool> Create([FromRoute] EnumModuleType moduleTypeId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Create(moduleTypeId, type, model);
        }

        [HttpPut]
        [Route("{moduleTypeId}/{type}")]
        public async Task<bool> Update([FromRoute] EnumModuleType moduleTypeId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Update(moduleTypeId, type, model);
        }

        [HttpDelete]
        [Route("{moduleTypeId}/{type}")]
        [AllowAnonymous]
        public async Task<bool> Drop([FromRoute] EnumModuleType moduleTypeId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Drop(moduleTypeId, type, model);
        }
    }
}
