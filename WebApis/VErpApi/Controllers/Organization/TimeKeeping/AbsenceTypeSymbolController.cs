using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.Organization.TimeKeeping
{
    [Route("api/organization/timekeeping/absenceTypeSymbol")]
    public class AbsenceTypeSymbolController : VErpBaseController
    {
        private readonly IAbsenceTypeSymbolService _absenceTypeSymbolService;

        public AbsenceTypeSymbolController(IAbsenceTypeSymbolService absenceTypeSymbolService)
        {
            _absenceTypeSymbolService = absenceTypeSymbolService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddCountedSymbol([FromBody] AbsenceTypeSymbolModel model)
        {
            return await _absenceTypeSymbolService.AddAbsenceTypeSymbol(model);
        }

        [HttpDelete]
        [Route("{absenceTypeSymbolId}")]
        public async Task<bool> DeleteCountedSymbol([FromRoute] int absenceTypeSymbolId)
        {
            return await _absenceTypeSymbolService.DeleteAbsenceTypeSymbol(absenceTypeSymbolId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<AbsenceTypeSymbolModel>> GetListCountedSymbol()
        {
            return await _absenceTypeSymbolService.GetListAbsenceTypeSymbol();
        }

        [HttpGet]
        [Route("{absenceTypeSymbolId}")]
        public async Task<AbsenceTypeSymbolModel> GetCountedSymbol([FromRoute] int absenceTypeSymbolId)
        {
            return await _absenceTypeSymbolService.GetAbsenceTypeSymbol(absenceTypeSymbolId);
        }

        [HttpPut]
        [Route("{absenceTypeSymbolId}")]
        public async Task<bool> UpdateCountedSymbol([FromRoute] int absenceTypeSymbolId, [FromBody] AbsenceTypeSymbolModel model)
        {
            return await _absenceTypeSymbolService.UpdateAbsenceTypeSymbol(absenceTypeSymbolId, model);

        }
    }
}