using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalMenuController : CrossServiceBaseController
    {
        private readonly IMenuService _menuService;
        public InternalMenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [Route("")]
        [HttpPost]
        public async Task<int> Create([FromBody] MenuInputModel req)
        {
            return await _menuService.Create(req);
        }
    }
}