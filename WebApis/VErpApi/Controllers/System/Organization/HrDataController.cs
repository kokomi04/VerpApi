using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/data/bills")]
    [ObjectDataApi(EnumObjectType.HrType, "hrTypeId")]
    public class HrDataController : VErpBaseController
    {
        private readonly IHrDataService _hrDataService;

        public HrDataController(IHrDataService hrDataService)
        {
            _hrDataService = hrDataService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{hrTypeId}/Search")]
        public async Task<PageDataTable> GetBills([FromRoute] int hrTypeId, [FromBody] HrTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _hrDataService.SearchHr(hrTypeId, request.FromDate, request.ToDate, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/{fId}")]
        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetBillInfoRows([FromRoute] int hrTypeId, [FromRoute] long fId)
        {
            return await _hrDataService.GetHr(hrTypeId, fId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{hrTypeId}")]
        public async Task<long> CreateBill([FromRoute] int hrTypeId, [FromBody] NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _hrDataService.CreateHr(hrTypeId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{hrTypeId}/{fId}")]
        public async Task<bool> UpdateBill([FromRoute] int hrTypeId, [FromRoute] long fId, [FromBody] NonCamelCaseDictionary<IList<NonCamelCaseDictionary>> data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _hrDataService.UpdateHr(hrTypeId, fId, data).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{hrTypeId}/{fId}")]
        public async Task<bool> DeleteBill([FromRoute] int hrTypeId, [FromRoute] long fId)
        {
            return await _hrDataService.DeleteHr(hrTypeId, fId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping([FromRoute] int hrTypeId, [FromQuery] int? areaId)
        {
            return await _hrDataService.GetFieldDataForMapping(hrTypeId, areaId);
        }

        [HttpPost]
        [Route("{hrTypeId}/importFromMapping")]
        public async Task<bool> ImportFromMapping([FromRoute] int hrTypeId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _hrDataService.ImportHrBillFromMapping(hrTypeId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{hrTypeId}/{fId}/reference/{hrAreaId}")]
        public async Task<bool> UpdateHrBillReference([FromRoute] int hrTypeId, [FromRoute] long fId, [FromRoute] int hrAreaId, [FromQuery] long fReferenceId)
        {
            return await _hrDataService.UpdateHrBillReference(hrTypeId, hrAreaId, fId, fReferenceId).ConfigureAwait(true);
        }
    }
}