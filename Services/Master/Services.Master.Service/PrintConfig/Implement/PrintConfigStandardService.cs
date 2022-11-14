using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static Org.BouncyCastle.Math.EC.ECCurve;
using static Verp.Resources.Master.Print.PrintConfigStandardValidationMessage;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
   
    public class PrintConfigStandardService : PrintConfigServiceAbstract<PrintConfigStandard, PrintConfigStandardModel>, IPrintConfigStandardService
    {
        private readonly ObjectActivityLogFacade _printConfigStandardActivityLog;

        public PrintConfigStandardService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigStandardService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService) : base(masterDBContext, appSetting, logger, mapper, docOpenXmlService, EnumObjectType.PrintConfigStandard)
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

        protected override int GetId(PrintConfigStandard entity)
        {
            return entity.PrintConfigStandardId;
        }
    }
}
