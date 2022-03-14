using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service.Implement;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.Report
{
    [Route("api/DashboardConfig")]
    public class DashboardConfigController : VErpBaseController
    {
        private readonly IDashboardConfigService _dashboardConfigService;

        public DashboardConfigController(IDashboardConfigService dashboardConfigService)
        {
            _dashboardConfigService = dashboardConfigService;
        }

        [HttpGet]
        [Route("type/{dashboardTypeId}/view")]
        public async Task<DashboardTypeViewModel> DashboardTypeViewInfo([FromRoute] int dashboardTypeId,[FromQuery] bool isConfig)
        {
            return await _dashboardConfigService
                .DashboardTypeViewGetInfo(dashboardTypeId, isConfig)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("type/{dashboardTypeId}/view")]
        public async Task<bool> ViewInfoUpdate([FromRoute] int dashboardTypeId, [FromBody] DashboardTypeViewModel model)
        {
            return await _dashboardConfigService
                .DashboardTypeViewUpdate(dashboardTypeId, model)
                .ConfigureAwait(true);
        }

        // [HttpGet]
        // [Route("type/search")]
        // public async Task<PageData<DashboardTypeListModel>> GetDashboardTypes([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? moduleTypeId = null)
        // {
        //     return await _dashboardConfigService
        //         .DashboardTypes(keyword, page, size, moduleTypeId)
        //         .ConfigureAwait(true);
        // }
        
        [HttpPost]
        [Route("type")]
        public async Task<int> AddDashboardType(DashboardTypeModel data)
        {
            return await _dashboardConfigService.AddDashboardType(data);
        }
        
        [HttpPost]
        [Route("group")]
        public async Task<int> AddDashboardTypeGroup(DashboardTypeGroupModel model)
        {
            return await _dashboardConfigService.AddDashboardTypeGroup(model);
        }
        
        [HttpGet]
        [Route("group")]
        public async Task<IList<DashboardTypeGroupModel>> DashboardTypeGroupList()
        {
            return await _dashboardConfigService.DashboardTypeGroupList();
        }
        
        [HttpGet]
        [Route("type")]
        public async Task<IList<DashboardTypeModel>> DashboardTypeList()
        {
            return await _dashboardConfigService.DashboardTypeList();
        }
        
        [HttpDelete]
        [Route("type/{dashboardTypeId}")]
        public async Task<bool> DeleteDashboardType([FromRoute]int dashboardTypeId)
        {
            return await _dashboardConfigService.DeleteDashboardType(dashboardTypeId);
        }
        
        [HttpDelete]
        [Route("group/{dashboardTypeGroupId}")]
        public async Task<bool> DeleteDashboardTypeGroup([FromRoute]int dashboardTypeGroupId)
        {
            return await _dashboardConfigService.DeleteDashboardTypeGroup(dashboardTypeGroupId);
        }
        
        [HttpGet]
        [Route("type/{dashboardTypeId}")]
        public async Task<DashboardTypeModel> GetDashboardType(int dashboardTypeId)
        {
            return await _dashboardConfigService.GetDashboardType(dashboardTypeId);
        }
        
        [HttpGet]
        [Route("group/{dashboardTypeGroupId}")]
        public async Task<DashboardTypeGroupModel> GetDashboardTypeGroup(int dashboardTypeGroupId)
        {
            return await _dashboardConfigService.GetDashboardTypeGroup(dashboardTypeGroupId);
        }
        
        [HttpPut]
        [Route("type/{dashboardTypeId}")]
        public async Task<bool> UpdateDashboardType([FromRoute]int dashboardTypeId, [FromBody]DashboardTypeModel data)
        {
            return await _dashboardConfigService.UpdateDashboardType(dashboardTypeId, data);
        }
        
        [HttpPut]
        [Route("group/{dashboardTypeGroupId}")]
        public async Task<bool> UpdateDashboardTypeGroup([FromRoute]int dashboardTypeGroupId, [FromBody]DashboardTypeGroupModel model)
        {
            return await _dashboardConfigService.UpdateDashboardTypeGroup(dashboardTypeGroupId, model);
        }
    }
}