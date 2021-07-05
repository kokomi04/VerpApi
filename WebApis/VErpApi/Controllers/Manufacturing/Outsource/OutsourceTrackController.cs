using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.Track;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceOrders")]
    [ApiController]
    public class OutsourceTrackController : VErpBaseController
    {
        private readonly IOutsourceTrackService _outsourceTrackService;

        public OutsourceTrackController(IOutsourceTrackService outsourceTrackService)
        {
            _outsourceTrackService = outsourceTrackService;
        }

        [HttpGet]
        [Route("{outsourceOrderId}/tracks")]
        public async Task<IList<OutsourceTrackModel>> SearchOutsourceTrackByOutsourceOrder([FromRoute] long outsourceOrderId)
        {
            return await _outsourceTrackService.SearchOutsourceTrackByOutsourceOrder(outsourceOrderId);
        }

        [HttpPut]
        [Route("{outsourceOrderId}/tracks")]
        public async Task<bool> UpdateOutsourceTrackByOutsourceOrder([FromRoute] long outsourceOrderId, [FromBody] IList<OutsourceTrackModel> req)
        {
            return await _outsourceTrackService.UpdateOutsourceTrackByOutsourceOrder(outsourceOrderId, req);
        }

        [HttpPost]
        [Route("{outsourceOrderId}/tracks")]
        public async Task<long> CreateOutsourceTrack([FromRoute] long outsourceOrderId, [FromBody] OutsourceTrackModel req)
        {
            return await _outsourceTrackService.CreateOutsourceTrack(outsourceOrderId, req);
        }

        [HttpPut]
        [Route("{outsourceOrderId}/tracks/{outsourceTrackId}")]
        public async Task<bool> UpdateOutsourceTrack([FromRoute] long outsourceOrderId, [FromRoute] long outsourceTrackId, [FromBody] OutsourceTrackModel req)
        {
            return await _outsourceTrackService.UpdateOutsourceTrack(outsourceOrderId, outsourceTrackId, req);
        }

        [HttpDelete]
        [Route("{outsourceOrderId}/tracks/{outsourceTrackId}")]
        public async Task<bool> DeleteOutsourceTrack([FromRoute] long outsourceOrderId, [FromRoute] long outsourceTrackId)
        {
            return await _outsourceTrackService.DeleteOutsourceTrack(outsourceOrderId, outsourceTrackId);
        }
    }
}
