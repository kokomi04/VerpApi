using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{

    public class PrintConfigStandardService : PrintConfigServiceAbstract<PrintConfigStandard, PrintConfigStandardModel, PrintConfigStandardModuleType>, IPrintConfigStandardService
    {
        private readonly ObjectActivityLogFacade _printConfigStandardActivityLog;

        public PrintConfigStandardService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigStandardService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService) : 
            base(masterDBContext, appSetting, logger, mapper, docOpenXmlService, EnumObjectType.PrintConfigStandard, nameof(PrintConfigStandard.PrintConfigStandardId))
        {

            _printConfigStandardActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigStandard);
        }

        protected override async Task LogAddPrintConfig(PrintConfigStandardModel model, PrintConfigStandard entity)
        {
            await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigStandardId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfig(PrintConfigStandardModel model, PrintConfigStandard entity)
        {
            await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigStandardId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfig(PrintConfigStandard entity)
        {
            await _printConfigStandardActivityLog.LogBuilder(() => PrintConfigStandardActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.PrintConfigName)
                 .ObjectId(entity.PrintConfigStandardId)
                 .JsonData(entity.JsonSerialize())
                 .CreateLog();
        }      
    }
}
