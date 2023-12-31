﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchaseOrder/MaterialCalc")]
    public class MaterialCalcController : VErpBaseController
    {
        private readonly IMaterialCalcService _materialCalcService;

        public MaterialCalcController(IMaterialCalcService materialCalcService)
        {
            _materialCalcService = materialCalcService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("List")]
        public async Task<PageData<MaterialCalcListModel>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromBody] ArrayClause filter = null)
        {
            return await _materialCalcService
                .GetList(keyword, filter, page, size)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("GetHistoryProductOrderList")]
        [VErpAction(EnumActionType.View)]
        public IAsyncEnumerable<MaterialOrderProductHistory> GetHistoryProductOrderList([FromBody] OrderProductMaterialHistoryInput req)
        {
            return _materialCalcService
                .GetHistoryProductOrderList(req?.ProductIds, req?.OrderCodes);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> Create([FromBody] MaterialCalcModel req)
        {
            return await _materialCalcService
                .Create(req)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{materialCalcId}")]
        public async Task<MaterialCalcModel> GetInfo([FromRoute] long materialCalcId)
        {
            return await _materialCalcService
                .Info(materialCalcId)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{materialCalcId}")]
        public async Task<bool> Update([FromRoute] long materialCalcId, [FromBody] MaterialCalcModel req)
        {
            return await _materialCalcService
                .Update(materialCalcId, req)
                .ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{materialCalcId}")]
        public async Task<bool> Delete([FromRoute] long materialCalcId)
        {
            return await _materialCalcService
                .Delete(materialCalcId)
                .ConfigureAwait(true);
        }
    }
}
