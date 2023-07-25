using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.OpenXml4Net.OPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.PrintConfig;
using static Verp.Resources.Master.Print.PrintConfigCustomValidationMessage;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class PrintConfigCustomService : PrintConfigServiceAbstract<PrintConfigCustom, PrintConfigCustomModel, PrintConfigCustomModuleType>, IPrintConfigCustomService
    {
        private readonly ObjectActivityLogFacade _printConfigCustomActivityLog;

        public PrintConfigCustomService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigCustomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService) :
            base(masterDBContext, appSetting, logger, mapper, docOpenXmlService, EnumObjectType.PrintConfigCustom, nameof(PrintConfigCustom.PrintConfigCustomId))
        {

            _printConfigCustomActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigCustom);
        }

        protected override async Task LogAddPrintConfig(PrintConfigCustomModel model, PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigCustomId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfig(PrintConfigCustomModel model, PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigCustomId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfig(PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.PrintConfigName)
                 .ObjectId(entity.PrintConfigCustomId)
                 .JsonData(entity.JsonSerialize())
                 .CreateLog();
        }


        public async Task<bool> RollbackPrintConfigCustom(int printConfigId)
        {
            var printConfig = await _masterDBContext.PrintConfigCustom
                .Where(p => p.PrintConfigCustomId == printConfigId)
                .FirstOrDefaultAsync();

            if (printConfig == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            if (!printConfig.PrintConfigStandardId.HasValue || printConfig.PrintConfigStandardId.Value <= 0)
                throw PrintConfigStandardEmpty.BadRequest(GeneralCode.InternalError);

            var printConfigStandard = await _masterDBContext.PrintConfigStandard
                .Where(x => x.PrintConfigStandardId == printConfig.PrintConfigStandardId)
                .FirstOrDefaultAsync();

            var source = _mapper.Map<PrintConfigRollbackModel>(printConfigStandard);

            if (source == null)
                throw PrintConfigStandardNotFound.BadRequest(GeneralCode.InternalError);

            var printConfigHeaderStandard = new PrintConfigHeaderStandard();

            if (printConfigStandard.PrintConfigHeaderStandardId.HasValue)
            {
                printConfigHeaderStandard = await _masterDBContext.PrintConfigHeaderStandard
                    .FindAsync(printConfigStandard.PrintConfigHeaderStandardId);
            }

            var printConfigHeaderCustom = await _masterDBContext.PrintConfigHeaderCustom
                .FirstOrDefaultAsync( c => c.PrintConfigHeaderStandardId == printConfigHeaderStandard.PrintConfigHeaderStandardId);

            if (printConfigHeaderCustom != null)
                printConfig.PrintConfigHeaderCustomId = printConfigHeaderCustom.PrintConfigHeaderCustomId;

            var destProperties = printConfig.GetType().GetProperties();
            foreach (var destProperty in destProperties)
            {
                var sourceProperty = source.GetType().GetProperty(destProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProperty != null && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    destProperty.SetValue(printConfig, sourceProperty.GetValue(source, new object[] { }), new object[] { });
                }
            }

            var moduleTypeIds = await _masterDBContext.PrintConfigStandardModuleType
                .Where(m => m.PrintConfigStandardId == printConfig.PrintConfigStandardId)
                .Select(m => m.ModuleTypeId)
                .ToListAsync();

            await UpdateMappingModuleTypes(moduleTypeIds, printConfigId);

            await _masterDBContext.SaveChangesAsync();

            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Delete)
            .MessageResourceFormatDatas(printConfig.PrintConfigName)
            .ObjectId(printConfig.PrintConfigCustomId)
            .JsonData(printConfig.JsonSerialize())
            .CreateLog();

            return true;
        }

    }


}
