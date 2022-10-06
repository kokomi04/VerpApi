using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.CategoryConfig;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/dataConfig")]
    public class SystemConfigController : VErpBaseController
    {
        [HttpGet]
        [Route("objectTypes")]
        public IList<ValueTitleModel> GetObjectTypes()
        {

            return EnumExtensions.GetEnumMembers<EnumObjectType>()
                .Select(m => new ValueTitleModel
                {
                    Value = (int)m.Enum,
                    Title = m.Description
                }).ToList();
        }
    }
}