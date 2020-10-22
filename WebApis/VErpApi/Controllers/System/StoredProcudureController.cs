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
        [Route("{subSystemId}")]
        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList([FromRoute] EnumModuleType subSystemId)
        {
            return await _storedProcedureService.GetList(subSystemId);
        }

        [HttpPost]
        [Route("{subSystemId}/{type}")]
        public async Task<bool> Create([FromRoute] EnumModuleType subSystemId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Create(subSystemId, type, model);
        }

        [HttpPut]
        [Route("{subSystemId}/{type}")]
        public async Task<bool> Update([FromRoute] EnumModuleType subSystemId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Update(subSystemId, type, model);
        }

        [HttpDelete]
        [Route("{subSystemId}/{type}")]
        [AllowAnonymous]
        public async Task<bool> Drop([FromRoute] EnumModuleType subSystemId, [FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Drop(subSystemId, type, model);
        }
    }
}
