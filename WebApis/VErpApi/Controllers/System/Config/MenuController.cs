using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/Menus")]
    public class MenuController : VErpBaseController
    {
        private readonly IMenuService _menuService;
        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        [GlobalApi]
        [Route("Me")]
        public async Task<ICollection<MenuOutputModel>> GetMeMenuList()
        {
            return await _menuService.GetMeMenuList().ConfigureAwait(true);
        }

        [HttpGet]
        [GlobalApi]
        [Route("")]
        public async Task<ICollection<MenuOutputModel>> Get()
        {
            return await _menuService.GetList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Post([FromBody] MenuInputModel req)
        {
            return await _menuService.Create(req);
        }

        [HttpGet]
        [GlobalApi]
        [Route("{menuId}")]
        public async Task<MenuOutputModel> Get([FromRoute] int menuId)
        {
            return await _menuService.Get(menuId);
        }

        [HttpPut]
        [Route("{menuId}")]
        public async Task<bool> Update([FromRoute] int menuId, [FromBody] MenuInputModel req)
        {
            return await _menuService.Update(menuId, req);
        }

        [HttpDelete]
        [Route("{menuId}")]
        public async Task<bool> Delete([FromRoute] int menuId)
        {
            return await _menuService.Delete(menuId);
        }
    }
}