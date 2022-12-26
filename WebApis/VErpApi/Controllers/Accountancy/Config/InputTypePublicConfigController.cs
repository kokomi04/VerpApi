using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/accountancy/config/public/inputType")]

    public class InputTypePublicConfigController : InputTypeConfigControllerAbstract
    {
        public InputTypePublicConfigController(IInputPublicConfigService inputConfigPublicService) : base(inputConfigPublicService)
        {

        }

    }
}
