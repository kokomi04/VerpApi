using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Config
{
    [Route("api/PurchasingOrder/config/voucherType")]

    public class VoucherTypeConfigController : VErpBaseController
    {
        private readonly IVoucherConfigService _voucherConfigService;
        public VoucherTypeConfigController(IVoucherConfigService voucherConfigService)
        {
            _voucherConfigService = voucherConfigService;
        }


        [HttpGet]
        [Route("groups")]
        public async Task<IList<VoucherTypeGroupList>> GetList()
        {
            return await _voucherConfigService.VoucherTypeGroupList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("groups")]
        public async Task<int> VoucherTypeGroupCreate([FromBody] VoucherTypeGroupModel model)
        {
            return await _voucherConfigService.VoucherTypeGroupCreate(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("groups/{voucherTypeGroupId}")]
        public async Task<bool> GetVoucherType([FromRoute] int voucherTypeGroupId, [FromBody] VoucherTypeGroupModel model)
        {
            return await _voucherConfigService.VoucherTypeGroupUpdate(voucherTypeGroupId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("groups/{voucherTypeGroupId}")]
        public async Task<bool> DeleteVoucherTypeGroup([FromRoute] int voucherTypeGroupId)
        {
            return await _voucherConfigService.VoucherTypeGroupDelete(voucherTypeGroupId).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("")]
        public async Task<PageData<VoucherTypeModel>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _voucherConfigService.GetVoucherTypes(keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("fields")]
        public async Task<PageData<VoucherFieldOutputModel>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _voucherConfigService.GetVoucherFields(keyword, page, size).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("fields")]
        public async Task<int> AddVoucherField([FromBody] VoucherFieldInputModel voucherAreaField)
        {
            return await _voucherConfigService.AddVoucherField(voucherAreaField).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("fields/{voucherFieldId}")]
        public async Task<bool> UpdateVoucherField([FromRoute] int voucherFieldId, [FromBody] VoucherFieldInputModel voucherField)
        {
            return await _voucherConfigService.UpdateVoucherField(voucherFieldId, voucherField).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("fields/{voucherFieldId}")]
        public async Task<bool> DeleteVoucherField([FromRoute] int voucherFieldId)
        {
            return await _voucherConfigService.DeleteVoucherField(voucherFieldId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddVoucherType([FromBody] VoucherTypeModel category)
        {
            return await _voucherConfigService.AddVoucherType(category).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/clone")]
        public async Task<int> CloneVoucherType([FromRoute] int voucherTypeId)
        {
            return await _voucherConfigService.CloneVoucherType(voucherTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}")]
        public async Task<VoucherTypeFullModel> GetVoucherTypeById([FromRoute] int voucherTypeId)
        {
            return await _voucherConfigService.GetVoucherType(voucherTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("getByCode")]
        public async Task<VoucherTypeFullModel> GetVoucherTypeByCode([FromQuery] string voucherTypeCode)
        {
            return await _voucherConfigService.GetVoucherType(voucherTypeCode).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}")]
        public async Task<bool> UpdateVoucherType([FromRoute] int voucherTypeId, [FromBody] VoucherTypeModel voucherType)
        {
            return await _voucherConfigService.UpdateVoucherType(voucherTypeId, voucherType).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}")]
        public async Task<bool> DeleteVoucherType([FromRoute] int voucherTypeId)
        {
            return await _voucherConfigService.DeleteVoucherType(voucherTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/voucherareas")]
        public async Task<PageData<VoucherAreaModel>> GetVoucherAreas([FromRoute] int voucherTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _voucherConfigService.GetVoucherAreas(voucherTypeId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/voucherareas/{voucherAreaId}")]
        public async Task<VoucherAreaModel> GetVoucherArea([FromRoute] int voucherTypeId, [FromRoute] int voucherAreaId)
        {
            return await _voucherConfigService.GetVoucherArea(voucherTypeId, voucherAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/basicInfo")]
        public async Task<VoucherTypeBasicOutput> GetVoucherTypeBasicInfo([FromRoute] int voucherTypeId)
        {
            return await _voucherConfigService.GetVoucherTypeBasicInfo(voucherTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
        public async Task<VoucherTypeViewModel> GetVoucherTypeBasicInfo([FromRoute] int voucherTypeId, [FromRoute] int voucherTypeViewId)
        {
            return await _voucherConfigService.GetVoucherTypeViewInfo(voucherTypeId, voucherTypeViewId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/views")]
        public async Task<int> VoucherTypeViewCreate([FromRoute] int voucherTypeId, [FromBody] VoucherTypeViewModel model)
        {
            return await _voucherConfigService.VoucherTypeViewCreate(voucherTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
        public async Task<bool> VoucherTypeViewUpdate([FromRoute] int voucherTypeId, [FromRoute] int voucherTypeViewId, [FromBody] VoucherTypeViewModel model)
        {
            if (voucherTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _voucherConfigService.VoucherTypeViewUpdate(voucherTypeViewId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
        public async Task<bool> VoucherTypeViewUpdate([FromRoute] int voucherTypeId, [FromRoute] int voucherTypeViewId)
        {
            if (voucherTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _voucherConfigService.VoucherTypeViewDelete(voucherTypeViewId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{voucherTypeId}/voucherareas")]
        public async Task<int> AddVoucherArea([FromRoute] int voucherTypeId, [FromBody] VoucherAreaInputModel voucherArea)
        {
            return await _voucherConfigService.AddVoucherArea(voucherTypeId, voucherArea).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/voucherareas/{voucherAreaId}")]
        public async Task<bool> UpdateVoucherArea([FromRoute] int voucherTypeId, [FromRoute] int voucherAreaId, [FromBody] VoucherAreaInputModel voucherArea)
        {
            return await _voucherConfigService.UpdateVoucherArea(voucherTypeId, voucherAreaId, voucherArea).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}/voucherareas/{voucherAreaId}")]
        public async Task<bool> DeleteVoucherArea([FromRoute] int voucherTypeId, [FromRoute] int voucherAreaId)
        {
            return await _voucherConfigService.DeleteVoucherArea(voucherTypeId, voucherAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/voucherareas/{voucherAreaId}/voucherareafields")]
        public async Task<PageData<VoucherAreaFieldOutputFullModel>> GetVoucherAreaFields([FromRoute] int voucherTypeId, [FromRoute] int voucherAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _voucherConfigService.GetVoucherAreaFields(voucherTypeId, voucherAreaId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/voucherareas/{voucherAreaId}/voucherareafields/{voucherAreaField}")]
        public async Task<VoucherAreaFieldOutputFullModel> GetVoucherAreaField([FromRoute] int voucherTypeId, [FromRoute] int voucherAreaId, [FromRoute] int voucherAreaField)
        {
            return await _voucherConfigService.GetVoucherAreaField(voucherTypeId, voucherAreaId, voucherAreaField).ConfigureAwait(true); ;
        }

        [HttpPost]
        [Route("{voucherTypeId}/multifields")]
        public async Task<bool> UpdateMultiField([FromRoute] int voucherTypeId, [FromBody] List<VoucherAreaFieldInputModel> fields)
        {
            return await _voucherConfigService.UpdateMultiField(voucherTypeId, fields).ConfigureAwait(true);
        }
    }
}
