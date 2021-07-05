using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Model.Input;
using Microsoft.AspNetCore.Authorization;
using VErp.Services.Master.Model.Config;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Accountancy.Model;
using System.Reflection;
using Verp.Services.PurchaseOrder.Model;
using VErp.Services.Stock.Model;
using VErp.Services.Manafacturing.Model;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore.ModelBinders;

namespace VErpApi.Controllers.System
{
    [Route("api/printConfig")]
    public class PrintConfigController : VErpBaseController
    {
        private readonly IPrintConfigService _printConfigService;

        private readonly Dictionary<EnumModuleType, Assembly> AssemblyModuleModel = new Dictionary<EnumModuleType, Assembly>()
        {
            { EnumModuleType.Accountant,    AccountancyModelAssembly.Assembly },
            { EnumModuleType.PurchaseOrder,PurchaseOrderModelAssembly.Assembly },
            { EnumModuleType.Stock, StockModelAssembly.Assembly },
            { EnumModuleType.Manufacturing, ManufacturingModelAssembly.Assembly }
        };

        public PrintConfigController(IPrintConfigService printConfigService)
        {
            _printConfigService = printConfigService;
        }

        [HttpGet]
        [Route("suggestionField")]
        public async Task<IList<EntityField>> GetSuggestionField([FromQuery] int moduleTypeId)
        {
            var fields = await _printConfigService.GetSuggestionField(moduleTypeId);

            if (fields.Count == 0 && AssemblyModuleModel.ContainsKey((EnumModuleType)moduleTypeId))
                fields = await _printConfigService.GetSuggestionField(AssemblyModuleModel[(EnumModuleType)moduleTypeId]);

            return fields;
        }
    }
}