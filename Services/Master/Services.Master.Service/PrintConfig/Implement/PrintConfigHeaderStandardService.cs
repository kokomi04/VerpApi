using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class PrintConfigHeaderStandardService :
            PrintConfigHeaderServiceAbstract<PrintConfigHeaderStandard, PrintConfigHeaderStandardModel, PrintConfigHeaderStandardViewModel>,
            IPrintConfigHeaderStandardService

    {


        private readonly ObjectActivityLogFacade _printConfigHeaderStandardActivityLog;

        public PrintConfigHeaderStandardService(MasterDBContext masterDBContext,
                ILogger<PrintConfigHeaderServiceAbstract<PrintConfigHeaderStandard,
                PrintConfigHeaderStandardModel, PrintConfigHeaderStandardViewModel>> logger,
                IMapper mapper,
                IActivityLogService activityLogService) :
                base(masterDBContext, logger, mapper, nameof(PrintConfigHeaderStandard.PrintConfigHeaderStandardId))
        {
            _printConfigHeaderStandardActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigHeaderStandard); ;
        }

        protected override async Task LogAddPrintConfigHeader(PrintConfigHeaderStandardModel model, PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderStandardId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfigHeader(PrintConfigHeaderStandardModel model, PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderStandardId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfigHeader(PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.Title)
                 .ObjectId(entity.PrintConfigHeaderStandardId)
                 .JsonData(entity.JsonSerialize())
                 .CreateLog();
        }
    }
}
