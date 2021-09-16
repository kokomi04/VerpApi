using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
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
        public async Task<PageData<PropertyCalcListModel>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromBody] ArrayClause filter = null)
        {
            return await _propertyCalcService
                .GetList(keyword, filter, page, size)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetHistoryProductOrderList")]
        public IAsyncEnumerable<PropertyOrderProductHistory> GetHistoryProductOrderList([FromQuery] IList<int> productIds, [FromQuery] IList<string> orderCodes)
        {
            return _propertyCalcService
                .GetHistoryProductOrderList(productIds, orderCodes);
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
