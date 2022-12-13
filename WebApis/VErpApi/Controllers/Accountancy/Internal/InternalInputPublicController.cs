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
    public class InternalInputPublicController : InternalInputControllerAbstract
    {
        public InternalInputPublicController(IInputDataPublicService inputDataService, IInputPublicConfigService inputConfigService)
            : base(inputDataService, inputConfigService)
        {

        }

    }
}