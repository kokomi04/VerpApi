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
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalInputController : CrossServiceBaseController
    {
        private readonly IInputDataService _inputDataService;
        private readonly IInputConfigService _inputConfigService;
        public InternalInputController(IInputDataService inputDataService, IInputConfigService inputConfigService)
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
    }
}