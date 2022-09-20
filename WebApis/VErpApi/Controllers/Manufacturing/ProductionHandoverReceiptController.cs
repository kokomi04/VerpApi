using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionHandoverReceiptController : VErpBaseController
    {
        private readonly IProductionHandoverReceiptService _productionHandoverReceiptService;

        public ProductionHandoverReceiptController(IProductionHandoverReceiptService productionHandoverReceiptService)
        {
            _productionHandoverReceiptService = productionHandoverReceiptService;
        }

        [HttpGet]
        [Route("DepartmentHandoverByDate")]
        public async Task<PageData<ProductionHandoverReceiptByDateModel>> GetDepartmentHandovers([FromQuery] IList<long> fromDepartmentIds, [FromQuery] IList<long> toDepartmentIds, [FromQuery] IList<long> fromStepIds, [FromQuery] IList<long> toStepIds, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] bool? isInFinish, [FromQuery] bool? isOutFinish, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productionHandoverReceiptService.GetDepartmentHandoversByDate(fromDepartmentIds, toDepartmentIds, fromStepIds, toStepIds, fromDate, toDate, isInFinish, isOutFinish, page, size);
        }


        [HttpGet]
        [Route("ProductionOrders/{productionOrderId}")]
        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers([FromRoute] long productionOrderId)
        {
            return await _productionHandoverReceiptService.GetProductionHandovers(productionOrderId);
        }

        [HttpGet]
        [Route("{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}")]
        public async Task<Dictionary<long, DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] int departmentId)
        {
            var lstDetail = await _productionHandoverReceiptService.GetDepartmentHandoverDetail(productionOrderId, productionStepId, departmentId);
            var group = lstDetail.GroupBy(d => d.ProductionStepId).ToDictionary(g => g.Key, g => g.First());
            return group;
        }

        [HttpPost]
        [Route("GetDetailByArrayProductionOrder/department/{departmentId}")]
        public async Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail([FromBody] IList<RequestObjectGetProductionOrderHandover> data, [FromRoute] int departmentId)
        {
            var lstDetail = await _productionHandoverReceiptService.GetDepartmentHandoverDetailByArrayProductionOrderId(data, departmentId);
            return lstDetail;
        }

        [HttpPost]
        [Route("DepartmentHandover/{departmentId}")]
        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers([FromRoute] long departmentId, [FromQuery] string keyword, [FromQuery] int? stepId, [FromQuery] int? productId, [FromQuery] int page, [FromQuery] int size, [FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] bool? isInFinish, [FromQuery] bool? isOutFinish, [FromQuery] EnumProductionStepLinkDataRoleType? productionStepLinkDataRoleTypeId)
        {
            return await _productionHandoverReceiptService.GetDepartmentHandovers(departmentId, keyword, page, size, fromDate, toDate, stepId, productId, isInFinish, isOutFinish, productionStepLinkDataRoleTypeId);
        }


        [HttpGet]
        [Route("productionInventoryRequirement/{productionOrderId}")]
        public async Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements([FromRoute] long productionOrderId)
        {
            return await _productionHandoverReceiptService.GetProductionInventoryRequirements(productionOrderId);
        }

        [HttpPost]
        [Route("{productionOrderId}")]
        public async Task<long> CreateProductionHandover([FromRoute] long productionOrderId, [FromBody] ProductionHandoverReceiptModel data)
        {
            return await _productionHandoverReceiptService.Create(productionOrderId, data);
        }

        [HttpPost]
        [Route("statictic/{productionOrderId}")]
        public async Task<long> CreateStatictic([FromRoute] long productionOrderId, [FromBody] ProductionHandoverReceiptModel data)
        {
            return await _productionHandoverReceiptService.CreateStatictic(productionOrderId, data);
        }

        [HttpPut]
        [Route("statictic/{receiptId}")]
        public async Task<bool> StaticticUpdate([FromRoute] long receiptId, [FromBody] ProductionHandoverReceiptModel data)
        {
            return await _productionHandoverReceiptService.Update(receiptId, data, EnumHandoverStatus.Accepted);
        }

        [HttpDelete]
        [Route("{receiptId}")]
        public async Task<bool> Delete([FromRoute] long receiptId)
        {
            return await _productionHandoverReceiptService.Delete(receiptId);
        }

        [HttpPut]
        [Route("{receiptId}")]
        public async Task<bool> Update([FromRoute] long receiptId, [FromBody] ProductionHandoverReceiptModel data)
        {
            return await _productionHandoverReceiptService.Update(receiptId, data, EnumHandoverStatus.Waiting);
        }

        [HttpPost]
        [Route("HandoverHistoryReceipts")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ProductionHandoverHistoryReceiptModel>> HandoverHistoryReceipts([FromQuery] string keyword, [FromQuery] long? fromDate, [FromQuery] long? toDate, 
            [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody]  Clause filters = null)
        {
            return await _productionHandoverReceiptService.GetList(keyword, fromDate, toDate, page, size, orderByFieldName, asc, filters);
        }

        [HttpGet]
        [Route("{receiptId}")]
        public async Task<ProductionHandoverReceiptModel> Info([FromRoute] long receiptId)
        {
            return await _productionHandoverReceiptService.Info(receiptId);
        }

        [HttpPut]
        [Route("AcceptBatch")]
        public async Task<bool> AcceptBatch([FromBody] IList<long> receiptIds)
        {
            return await _productionHandoverReceiptService.AcceptBatch(receiptIds);
        }

        [HttpPut]
        [Route("{receiptId}/accept")]
        public async Task<bool> Accept([FromRoute] long receiptId)
        {
            return await _productionHandoverReceiptService.Confirm(receiptId, EnumHandoverStatus.Accepted);
        }

        [HttpPut]
        [Route("{receiptId}/reject")]
        public async Task<bool> Reject([FromRoute] long receiptId)
        {
            return await _productionHandoverReceiptService.Confirm(receiptId, EnumHandoverStatus.Rejected);
        }

        [HttpPost]
        [Route("patch")]
        public async Task<bool> CreateProductionHandoverPatch([FromBody] IList<ProductionHandoverReceiptModel> data)
        {
            return await _productionHandoverReceiptService.CreateBatch(data);
        }
    }
}
