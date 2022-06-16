﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErpApi.Controllers.System
{
    [Route("api/subsystems")]
    public class SubSystemsController : VErpBaseController
    {
        private readonly ISubSystemService _subSystemService;

        public SubSystemsController(ISubSystemService subSystemService)
        {
            _subSystemService = subSystemService;
        }

        [HttpGet]
        [Route("")]
        public IList<SubSystemInfo> GetSubSystems()
        {
            return _subSystemService.GetSubSystems();
        }
    }
}
