using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Service.Input.Implement.Facade;

namespace VErpApi.Controllers.Accountancy.Data
{

    [Route("api/accountancy/public/bills")]
    // [ObjectDataApi(EnumObjectType.InputType, "inputTypeId")]
    public class InputPublicController : InputControllerBaseAbstract
    {

        public InputPublicController(IInputDataPrivateService inputDataService, IInpuDataExportFacadeService inpuDataExportFacadeService)
            : base(inputDataService, inpuDataExportFacadeService)
        {

        }

    }
}
