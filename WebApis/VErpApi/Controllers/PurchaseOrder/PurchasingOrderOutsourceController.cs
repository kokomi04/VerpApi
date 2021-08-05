using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder")]
    public class PurchasingOrderOutsourceController : VErpBaseController
    {
        private readonly IPurchaseOrderOutsourceStepService _purchaseOrderOutsourceStepService;
        private readonly IPurchaseOrderOutsourcePartService _purchaseOrderOutsourcePartService;

        public PurchasingOrderOutsourceController(
            IPurchaseOrderOutsourcePartService purchaseOrderOutsourcePartService,
            IPurchaseOrderOutsourceStepService purchaseOrderOutsourceStepService)
        {
            _purchaseOrderOutsourcePartService = purchaseOrderOutsourcePartService;
            _purchaseOrderOutsourceStepService = purchaseOrderOutsourceStepService;
        }

        #region Outsource-Step

        [HttpPost]
        [Route("outsourceStep")]
        public async Task<long> CreatePurchaseOrderOutsourceStep([FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourceStepService.CreatePurchaseOrderOutsourceStep(model);
        }

        [HttpDelete]
        [Route("outsourceStep/{purchaseOrderId}")]
        public async Task<bool> DeletePurchaseOrderOutsourceStep([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourceStepService.DeletePurchaseOrderOutsourceStep(purchaseOrderId);
        }

        /// <summary>
        /// Lấy thông tin chi tiết cần đi gia công
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("outsourceStep/request")]
        public async Task<IList<RefOutsourceStepRequestModel>> GetOutsourceStepRequest()
        {
            return await _purchaseOrderOutsourceStepService.GetOutsourceStepRequest();
        }

        /// <summary>
        /// Lấy thông tin các chi tiết cần gia công theo mã yêu cầu gia công
        /// </summary>
        /// <param name="arrOutsourceStepId"></param>
        /// <returns></returns>
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("outsourceStep/request")]
        public async Task<IList<RefOutsourceStepRequestModel>> GetOutsourceStepRequestByIds([FromBody] long[] arrOutsourceStepId)
        {
            return await _purchaseOrderOutsourceStepService.GetOutsourceStepRequest(arrOutsourceStepId);
        }

        [HttpPut]
        [Route("outsourceStep/{purchaseOrderId}")]
        public async Task<bool> UpdatePurchaseOrderOutsourceStep([FromRoute] long purchaseOrderId, [FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourceStepService.UpdatePurchaseOrderOutsourceStep(purchaseOrderId, model);
        }

        #endregion

        #region Outsource-Part

        [HttpPost]
        [Route("outsourcePart")]
        public async Task<long> CreatePurchaseOrderOutsourcePart([FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourcePartService.CreatePurchaseOrderOutsourcePart(model);
        }

        [HttpDelete]
        [Route("outsourcePart/{purchaseOrderId}")]
        public async Task<bool> DeletePurchaseOrderOutsourcePart([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourcePartService.DeletePurchaseOrderOutsourcePart(purchaseOrderId);
        }

        /// <summary>
        /// Lấy thông tin chi tiết cần đi gia công
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("outsourcePart/request")]
        public async Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequest()
        {
            return await _purchaseOrderOutsourcePartService.GetOutsourcePartRequest();
        }

        /// <summary>
        /// Lấy thông tin các chi tiết cần gia công theo mã yêu cầu gia công
        /// </summary>
        /// <param name="arrOutsourcePartId"></param>
        /// <returns></returns>
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("outsourcePart/request")]
        public async Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequestByIds([FromBody] long[] arrOutsourcePartId)
        {
            return await _purchaseOrderOutsourcePartService.GetOutsourcePartRequest(arrOutsourcePartId);
        }

        [HttpPut]
        [Route("outsourcePart/{purchaseOrderId}")]
        public async Task<bool> UpdatePurchaseOrderOutsourcePart([FromRoute] long purchaseOrderId, [FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourcePartService.UpdatePurchaseOrderOutsourcePart(purchaseOrderId, model);
        }

        #endregion

    }
}
