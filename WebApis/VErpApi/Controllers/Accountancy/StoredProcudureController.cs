using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Model.StoredProcedure;
using VErp.Services.Accountancy.Service.StoredProcedure;

namespace VErpApi.Controllers.Accountancy
{
    [Route("api/Accountancy/[controller]")]
    public class StoredProcuduresController : VErpBaseController
    {
        private readonly IStoredProcedureService _storedProcedureService;

        public StoredProcuduresController(IStoredProcedureService storedProcedureService)
        {
            _storedProcedureService = storedProcedureService;
        }

        [HttpGet()]
        [Route("")]
        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList()
        {
            return await _storedProcedureService.GetList();
        }

        [HttpPost]
        [Route("{type}")]
        public async Task<bool> Create([FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Create(type, model);
        }

        [HttpPut]
        [Route("{type}")]
        public async Task<bool> Update([FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Update(type, model);
        }

        [HttpDelete]
        [Route("{type}")]
        [AllowAnonymous]
        public async Task<bool> Drop([FromRoute] int type, [FromBody] StoredProcedureModel model)
        {
            return await _storedProcedureService.Drop(type, model);
        }
    }
}
