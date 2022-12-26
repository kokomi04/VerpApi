using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public abstract class InternalInputControllerAbstract : CrossServiceBaseController
    {
        private readonly IInputDataServiceBase _inputDataService;
        private readonly IInputConfigServiceBase _inputConfigService;
        public InternalInputControllerAbstract(IInputDataServiceBase inputDataService, IInputConfigServiceBase inputConfigService)
        {
            _inputDataService = inputDataService;
            _inputConfigService = inputConfigService;
        }

        [HttpPost]
        [Route("CheckReferFromCategory")]
        public async Task<bool> CheckReferFromCategory([FromBody] ReferFromCategoryModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _inputDataService.CheckReferFromCategory(data.CategoryCode, data.FieldNames, data.CategoryRow).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<InputTypeSimpleModel>> GetSimpleList()
        {
            return await _inputConfigService.GetInputTypeSimpleList().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/GetBillNotApprovedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet([FromRoute] int inputTypeId)
        {
            return await _inputDataService.GetBillNotApprovedYet(inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}/GetBillNotChekedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet([FromRoute] int inputTypeId)
        {
            return await _inputDataService.GetBillNotChekedYet(inputTypeId);
        }
    }
}