using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Verp.Services.PurchaseOrder.Model;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Model;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Model;

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