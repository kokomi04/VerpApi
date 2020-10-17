using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
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
        public InternalVoucherController(IVoucherDataService voucherDataService)
        {
            _voucherDataService = voucherDataService;
        }

        [HttpPost]
        [Route("CheckReferFromCategory")]
        public async Task<bool> CheckReferFromCategory([FromBody] ReferFromCategoryModel data)
        {
            return await _voucherDataService.CheckReferFromCategory(data.CategoryCode, data.FieldNames, data.CategoryRow).ConfigureAwait(true);
        }
    }
}