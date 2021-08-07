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
        private readonly IPurchaseOrderOutsourcePropertyService _purchaseOrderOutsourcePropertyService;

        private readonly IPurchaseOrderTrackService _purchaseOrderTrackService;

        public PurchasingOrderOutsourceController(
            IPurchaseOrderOutsourcePartService purchaseOrderOutsourcePartService,
            IPurchaseOrderOutsourceStepService purchaseOrderOutsourceStepService,
            IPurchaseOrderOutsourcePropertyService purchaseOrderOutsourcePropertyService,
            IPurchaseOrderTrackService purchaseOrderTrackService)
        {
            _purchaseOrderOutsourcePartService = purchaseOrderOutsourcePartService;
            _purchaseOrderOutsourceStepService = purchaseOrderOutsourceStepService;
            _purchaseOrderOutsourcePropertyService = purchaseOrderOutsourcePropertyService;
            _purchaseOrderTrackService = purchaseOrderTrackService;
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

        [HttpGet]
        [Route("outsourceStep/{purchaseOrderId}")]
        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourceStep([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourceStepService.GetPurchaseOrderOutsourceStep(purchaseOrderId);
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

        [HttpGet]
        [Route("outsourcePart/{purchaseOrderId}")]
        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourcePart([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourcePartService.GetPurchaseOrderOutsourcePart(purchaseOrderId);
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

        #region Outsource-Property

        [HttpPost]
        [Route("outsourceProperty")]
        public async Task<long> CreatePurchaseOrderOutsourceProperty([FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourcePropertyService.CreatePurchaseOrderOutsourceProperty(model);
        }

        [HttpDelete]
        [Route("outsourceProperty/{purchaseOrderId}")]
        public async Task<bool> DeletePurchaseOrderOutsourceProperty([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourcePropertyService.DeletePurchaseOrderOutsourceProperty(purchaseOrderId);
        }

        [HttpGet]
        [Route("outsourceProperty/{purchaseOrderId}")]
        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourceProperty([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderOutsourcePropertyService.GetPurchaseOrderOutsourceProperty(purchaseOrderId);
        }

        [HttpGet]
        [Route("outsourceProperty/propertyCalc/{propertyCalcId}")]
        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourcePropertyByPropertyCalcId([FromRoute] long propertyCalcId)
        {
            return await _purchaseOrderOutsourcePropertyService.GetPurchaseOrderOutsourcePropertyByPropertyCalcId(propertyCalcId);
        }

        [HttpPut]
        [Route("outsourceProperty/{purchaseOrderId}")]
        public async Task<bool> UpdatePurchaseOrderOutsourceProperty([FromRoute] long purchaseOrderId, [FromBody] PurchaseOrderInput model)
        {
            return await _purchaseOrderOutsourcePropertyService.UpdatePurchaseOrderOutsourceProperty(purchaseOrderId, model);
        }

        #endregion

        #region Outsource-tracked

        [HttpPost()]
        [Route("outsourceTrack/{purchaseOrderId}")]
        public async Task<long> CreatePurchaseOrderTrack([FromRoute] long purchaseOrderId, [FromBody] purchaseOrderTrackedModel req)
        {
            return await _purchaseOrderTrackService.CreatePurchaseOrderTrack(purchaseOrderId, req);
        }

        [HttpDelete()]
        [Route("outsourceTrack/{purchaseOrderId}/{purchaseOrderTrackId}")]
        public async Task<bool> DeletePurchaseOrderTrack([FromRoute] long purchaseOrderId, [FromRoute] long purchaseOrderTrackId)
        {
            return await _purchaseOrderTrackService.DeletePurchaseOrderTrack(purchaseOrderId, purchaseOrderTrackId);
        }

        [HttpGet()]
        [Route("outsourceTrack/{purchaseOrderId}")]
        public async Task<IList<purchaseOrderTrackedModel>> SearchPurchaseOrderTrackByPurchaseOrder([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderTrackService.SearchPurchaseOrderTrackByPurchaseOrder(purchaseOrderId);
        }

        [HttpPut()]
        [Route("outsourceTrack/{purchaseOrderId}/{purchaseOrderTrackId}")]
        public async Task<bool> UpdatePurchaseOrderTrack([FromRoute] long purchaseOrderId, [FromRoute] long purchaseOrderTrackId, [FromBody] purchaseOrderTrackedModel req)
        {
            return await _purchaseOrderTrackService.UpdatePurchaseOrderTrack(purchaseOrderId, purchaseOrderTrackId, req);
        }

        [HttpPut()]
        [Route("outsourceTrack/{purchaseOrderId}")]
        public async Task<bool> UpdatePurchaseOrderTrackByPurchaseOrderId([FromRoute] long purchaseOrderId, [FromBody] IList<purchaseOrderTrackedModel> req)
        {
            return await _purchaseOrderTrackService.UpdatePurchaseOrderTrackByPurchaseOrderId(purchaseOrderId, req);
        }
        #endregion
    }
}
