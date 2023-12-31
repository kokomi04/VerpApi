﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using VErp.Commons.Enums.Manafacturing;
//using VErp.Infrastructure.ApiCore;
//using VErp.Infrastructure.EF.EFExtensions;
//using VErp.Infrastructure.ServiceCore.Model;
//using VErp.Services.Manafacturing.Model.Outsource.Order;
//using VErp.Services.Manafacturing.Model.Outsource.Order.Part;
//using VErp.Services.Manafacturing.Service.Outsource;
//using VErp.Commons.GlobalObject;

//namespace VErpApi.Controllers.Manufacturing.Outsource
//{

//    [Route("api/manufacturing/outsourcePartOrder")]
//    [ApiController]
//    public class OutsourcePartOrderController : VErpBaseController
//    {
//        private readonly IOutsourcePartOrderService _outsourceOrderService;

//        public OutsourcePartOrderController(IOutsourcePartOrderService outsourceOrderService)
//        {
//            _outsourceOrderService = outsourceOrderService;
//        }

//        [HttpPost]
//        [Route("")]
//        public async Task<long> CreateOutsourceOrderPart([FromBody] OutsourcePartOrderInput req)
//        {
//            return await _outsourceOrderService.CreateOutsourceOrderPart(req);
//        }

//        [HttpPost]
//        [Route("search")]
//        public async Task<PageData<OutsourcePartOrderDetailInfo>> GetListOutsourceOrderPart(
//            [FromQuery] string keyword,
//            [FromQuery] int page,
//            [FromQuery] int size,
//            [FromQuery] long fromDate,
//            [FromQuery] long toDate,
//            [FromBody] Clause filters)
//        {
//            return await _outsourceOrderService.GetListOutsourceOrderPart(keyword, page, size,fromDate, toDate, filters);
//        }

//        [HttpDelete]
//        [Route("{outsourceOrderId}")]
//        public async Task<bool> DeleteOutsourceOrderPart([FromRoute] long outsourceOrderId)
//        {
//            return await _outsourceOrderService.DeleteOutsourceOrderPart(outsourceOrderId);
//        }

//        [HttpGet]
//        [Route("{outsourceOrderId}")]
//        public async Task<OutsourcePartOrderOutput> GetOutsourceOrderPart([FromRoute] long outsourceOrderId)
//        {
//            return await _outsourceOrderService.GetOutsourceOrderPart(outsourceOrderId);
//        }

//        [HttpPut]
//        [Route("{outsourceOrderId}")]
//        public async Task<bool> UpdateOutsourceOrderPart([FromRoute] long outsourceOrderId, [FromBody] OutsourcePartOrderInput req)
//        {
//            return await _outsourceOrderService.UpdateOutsourceOrderPart(outsourceOrderId, req);
//        }

//        [HttpGet]
//        [Route("{outsourceOrderId}/materials")]
//        public async Task<IList<OutsourceOrderMaterialsLSX>> GetMaterials([FromRoute] long outsourceOrderId)
//        {
//            return await _outsourceOrderService.GetMaterials(outsourceOrderId);
//        }
//    }
//}
