using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.HrConfig;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/config/hrType")]
    public class HrTypeController: VErpBaseController
    {
        public readonly IHrTypeService _hrTypeService;
        public readonly IHrAreaService _hrAreaService;
        public readonly IHrTypeGroupService _hrTypeGroupService;

        public HrTypeController(IHrTypeService hrTypeService, IHrAreaService hrAreaService, IHrTypeGroupService hrTypeGroupService)
        {
            _hrTypeService = hrTypeService;
            _hrAreaService = hrAreaService;
            _hrTypeGroupService = hrTypeGroupService;
        }

        [HttpGet]
        [Route("groups")]
        public async Task<IList<HrTypeGroupList>> GetList()
        {
            return await _hrTypeGroupService.HrTypeGroupList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("groups")]
        public async Task<int> HrTypeGroupCreate([FromBody] HrTypeGroupModel model)
        {
            return await _hrTypeGroupService.HrTypeGroupCreate(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("groups/{hrTypeGroupId}")]
        public async Task<bool> GetHrType([FromRoute] int hrTypeGroupId, [FromBody] HrTypeGroupModel model)
        {
            return await _hrTypeGroupService.HrTypeGroupUpdate(hrTypeGroupId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("groups/{hrTypeGroupId}")]
        public async Task<bool> DeleteHrTypeGroup([FromRoute] int hrTypeGroupId)
        {
            return await _hrTypeGroupService.HrTypeGroupDelete(hrTypeGroupId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<HrTypeModel>> SearchHrType([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _hrTypeService.GetHrTypes(keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetAllConfig")]
        public async Task<IList<HrTypeFullModel>> GetAllConfig()
        {
            return await _hrTypeService.GetAllHrTypes().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<HrTypeSimpleModel>> GetSimpleList()
        {
            return await _hrTypeService.GetHrTypeSimpleList().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("areas/{hrAreaId}/fields")]
        public async Task<PageData<HrFieldOutputModel>> GetAllFields([FromRoute] int hrAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _hrAreaService.GetHrFields(hrAreaId, keyword, page, size).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("areas/{hrAreaId}/fields")]
        public async Task<HrFieldInputModel> AddHrField([FromRoute] int hrAreaId, [FromBody] HrFieldInputModel hrField)
        {
            return await _hrAreaService.AddHrField(hrAreaId, hrField).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("areas/{hrAreaId}/fields/{hrFieldId}")]
        public async Task<HrFieldInputModel> UpdateHrField([FromRoute] int hrAreaId, [FromRoute] int hrFieldId, [FromBody] HrFieldInputModel hrField)
        {
            return await _hrAreaService.UpdateHrField(hrAreaId, hrFieldId, hrField).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("areas/{hrAreaId}/fields/{hrFieldId}")]
        public async Task<bool> DeleteHrField([FromRoute] int hrAreaId, [FromRoute] int hrFieldId)
        {
            return await _hrAreaService.DeleteHrField(hrAreaId, hrFieldId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GlobalSetting")]
        public async Task<HrTypeGlobalSettingModel> GetHrGlobalSetting()
        {
            return await _hrTypeService.GetHrGlobalSetting().ConfigureAwait(true);
        }

        [HttpPut]
        [Route("GlobalSetting")]
        public async Task<bool> UpdateHrGlobalSetting([FromBody] HrTypeGlobalSettingModel setting)
        {
            return await _hrTypeService.UpdateHrGlobalSetting(setting).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddHrType([FromBody] HrTypeModel category)
        {
            return await _hrTypeService.AddHrType(category).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{hrTypeId}/clone")]
        public async Task<int> CloneHrType([FromRoute] int hrTypeId)
        {
            return await _hrTypeService.CloneHrType(hrTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}")]
        public async Task<HrTypeFullModel> GetHrTypeById([FromRoute] int hrTypeId)
        {
            return await _hrTypeService.GetHrType(hrTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("getByCode")]
        public async Task<HrTypeFullModel> GetHrTypeByCode([FromQuery] string hrTypeCode)
        {
            return await _hrTypeService.GetHrType(hrTypeCode).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{hrTypeId}")]
        public async Task<bool> UpdateHrType([FromRoute] int hrTypeId, [FromBody] HrTypeModel hrType)
        {
            return await _hrTypeService.UpdateHrType(hrTypeId, hrType).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{hrTypeId}")]
        public async Task<bool> DeleteHrType([FromRoute] int hrTypeId)
        {
            return await _hrTypeService.DeleteHrType(hrTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/areas")]
        public async Task<PageData<HrAreaModel>> GetHrAreas([FromRoute] int hrTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _hrAreaService.GetHrAreas(hrTypeId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/areas/{hrAreaId}")]
        public async Task<HrAreaModel> GetHrArea([FromRoute] int hrTypeId, [FromRoute] int hrAreaId)
        {
            return await _hrAreaService.GetHrArea(hrTypeId, hrAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/basicInfo")]
        public async Task<HrTypeBasicOutput> GetHrTypeBasicInfo([FromRoute] int hrTypeId)
        {
            return await _hrTypeService.GetHrTypeBasicInfo(hrTypeId).ConfigureAwait(true);
        }

        // [HttpGet]
        // [Route("{hrTypeId}/views/{hrTypeViewId}")]
        // public async Task<HrTypeViewModel> GetHrTypeBasicInfo([FromRoute] int hrTypeId, [FromRoute] int hrTypeViewId)
        // {
        //     return await _hrConfigService.GetHrTypeViewInfo(hrTypeId, hrTypeViewId).ConfigureAwait(true);
        // }

        // [HttpPost]
        // [Route("{hrTypeId}/views")]
        // public async Task<int> HrTypeViewCreate([FromRoute] int hrTypeId, [FromBody] HrTypeViewModel model)
        // {
        //     return await _hrConfigService.HrTypeViewCreate(hrTypeId, model).ConfigureAwait(true);
        // }

        // [HttpPut]
        // [Route("{hrTypeId}/views/{hrTypeViewId}")]
        // public async Task<bool> HrTypeViewUpdate([FromRoute] int hrTypeId, [FromRoute] int hrTypeViewId, [FromBody] HrTypeViewModel model)
        // {
        //     if (hrTypeId <= 0)
        //     {
        //         throw new BadRequestException(GeneralCode.InvalidParams);
        //     }
        //     return await _hrConfigService.HrTypeViewUpdate(hrTypeViewId, model).ConfigureAwait(true);
        // }

        // [HttpDelete]
        // [Route("{hrTypeId}/views/{hrTypeViewId}")]
        // public async Task<bool> HrTypeViewUpdate([FromRoute] int hrTypeId, [FromRoute] int hrTypeViewId)
        // {
        //     if (hrTypeId <= 0)
        //     {
        //         throw new BadRequestException(GeneralCode.InvalidParams);
        //     }
        //     return await _hrConfigService.HrTypeViewDelete(hrTypeViewId).ConfigureAwait(true);
        // }


        [HttpPost]
        [Route("{hrTypeId}/areas")]
        public async Task<int> AddHrArea([FromRoute] int hrTypeId, [FromBody] HrAreaInputModel hrArea)
        {
            return await _hrAreaService.AddHrArea(hrTypeId, hrArea).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{hrTypeId}/areas/{hrAreaId}")]
        public async Task<bool> UpdateHrArea([FromRoute] int hrTypeId, [FromRoute] int hrAreaId, [FromBody] HrAreaInputModel hrArea)
        {
            return await _hrAreaService.UpdateHrArea(hrTypeId, hrAreaId, hrArea).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{hrTypeId}/areas/{hrAreaId}")]
        public async Task<bool> DeleteHrArea([FromRoute] int hrTypeId, [FromRoute] int hrAreaId)
        {
            return await _hrAreaService.DeleteHrArea(hrTypeId, hrAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/areas/{hrAreaId}/areafields")]
        public async Task<PageData<HrAreaFieldOutputFullModel>> GetHrAreaFields([FromRoute] int hrTypeId, [FromRoute] int hrAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _hrAreaService.GetHrAreaFields(hrTypeId, hrAreaId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/areas/{hrAreaId}/areaFields/{hrAreaField}")]
        public async Task<HrAreaFieldOutputFullModel> GetHrAreaField([FromRoute] int hrTypeId, [FromRoute] int hrAreaId, [FromRoute] int hrAreaField)
        {
            return await _hrAreaService.GetHrAreaField(hrTypeId, hrAreaId, hrAreaField).ConfigureAwait(true); ;
        }

        [HttpPost]
        [Route("{hrTypeId}/multifields")]
        public async Task<bool> UpdateMultiField([FromRoute] int hrTypeId, [FromBody] List<HrAreaFieldInputModel> fields)
        {
            return await _hrAreaService.UpdateMultiField(hrTypeId, fields).ConfigureAwait(true);
        }
    }
}