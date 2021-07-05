using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
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
    }
}