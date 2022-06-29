using Microsoft.AspNetCore.Mvc;
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
    [Route("api/PurchaseOrder/PropertyCalc")]
    public class PropertyCalcController : VErpBaseController
    {
        private readonly IPropertyCalcService _propertyCalcService;

        public PropertyCalcController(IPropertyCalcService propertyCalcService)
        {
            _propertyCalcService = propertyCalcService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("List")]
        public async Task<PageData<PropertyCalcListModel>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string sortBy, [FromQuery] bool asc, [FromBody] ArrayClause filter = null)
        {
            return await _propertyCalcService
                .GetList(keyword, filter, page, size, sortBy, asc)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("GetHistoryProductOrderList")]
        [VErpAction(EnumActionType.View)]
        public IAsyncEnumerable<PropertyOrderProductHistory> GetHistoryProductOrderList([FromBody] OrderProductMaterialHistoryInput req)
        {
            return _propertyCalcService
                .GetHistoryProductOrderList(req?.ProductIds, req?.OrderCodes);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> Create([FromBody] PropertyCalcModel req)
        {
            return await _propertyCalcService
                .Create(req)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{propertyCalcId}")]
        public async Task<PropertyCalcModel> GetInfo([FromRoute] long propertyCalcId)
        {
            return await _propertyCalcService
                .Info(propertyCalcId)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{PropertyCalcId}")]
        public async Task<bool> Update([FromRoute] long propertyCalcId, [FromBody] PropertyCalcModel req)
        {
            return await _propertyCalcService
                .Update(propertyCalcId, req)
                .ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{propertyCalcId}")]
        public async Task<bool> Delete([FromRoute] long propertyCalcId)
        {
            return await _propertyCalcService
                .Delete(propertyCalcId)
                .ConfigureAwait(true);
        }
    }
}
