﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalUnitController : CrossServiceBaseController
    {
        private readonly IUnitService _unitService;
        public InternalUnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<UnitOutput>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] EnumUnitStatus? unitStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _unitService.GetList(keyword, unitStatusId, page, size, filters);
        }

        [HttpGet]
        [Route("{unitId}")]
        public async Task<UnitOutput> GetUnitInfo([FromRoute] int unitId)
        {
            return await _unitService.GetUnitInfo(unitId);
        }
    }
}