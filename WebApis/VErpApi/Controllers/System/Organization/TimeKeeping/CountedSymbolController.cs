using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/countedSymbol")]
    public class CountedSymbolController : VErpBaseController
    {
        private readonly ICountedSymbolService _countedSymbolService;

        public CountedSymbolController(ICountedSymbolService countedSymbolService)
        {
            _countedSymbolService = countedSymbolService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddCountedSymbol([FromBody] CountedSymbolModel model)
        {
            return await _countedSymbolService.AddCountedSymbol(model);
        }

        [HttpDelete]
        [Route("{countedSymbolId}")]
        public async Task<bool> DeleteCountedSymbol([FromRoute] int countedSymbolId)
        {
            return await _countedSymbolService.DeleteCountedSymbol(countedSymbolId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<CountedSymbolModel>> GetListCountedSymbol()
        {
            return await _countedSymbolService.GetListCountedSymbol();
        }

        [HttpGet]
        [Route("{countedSymbolId}")]
        public async Task<CountedSymbolModel> GetCountedSymbol([FromRoute] int countedSymbolId)
        {
            return await _countedSymbolService.GetCountedSymbol(countedSymbolId);
        }

        [HttpPut]
        [Route("{countedSymbolId}")]
        public async Task<bool> UpdateCountedSymbol([FromRoute] int countedSymbolId, [FromBody] CountedSymbolModel model)
        {
            return await _countedSymbolService.UpdateCountedSymbol(countedSymbolId, model);

        }
    }
}