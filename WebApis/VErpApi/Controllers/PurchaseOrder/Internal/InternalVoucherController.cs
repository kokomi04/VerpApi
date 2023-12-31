﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalVoucherController : CrossServiceBaseController
    {
        private readonly IVoucherDataService _voucherDataService;
        private readonly IVoucherConfigService _voucherConfigService;
        public InternalVoucherController(IVoucherDataService voucherDataService, IVoucherConfigService voucherConfigService)
        {
            _voucherDataService = voucherDataService;
            _voucherConfigService = voucherConfigService;
        }

        [HttpPost]
        [Route("CheckReferFromCategory")]
        public async Task<bool> CheckReferFromCategory([FromBody] ReferFromCategoryModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _voucherDataService.CheckReferFromCategory(data.CategoryCode, data.FieldNames, data.CategoryRow).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<VoucherTypeSimpleModel>> GetSimpleList()
        {
            return await _voucherConfigService.GetVoucherTypeSimpleList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("OrderByCodes")]
        public async Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes([FromBody] IList<string> orderCodes)
        {
            return await _voucherDataService.OrderByCodes(orderCodes);
        }

        [HttpGet]
        [Route("{voucherTypeId}/GetBillNotApprovedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet([FromRoute] int voucherTypeId)
        {
            return await _voucherDataService.GetBillNotApprovedYet(voucherTypeId);
        }

        [HttpGet]
        [Route("{voucherTypeId}/GetBillNotChekedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet([FromRoute] int voucherTypeId)
        {
            return await _voucherDataService.GetBillNotChekedYet(voucherTypeId);
        }
    }
}