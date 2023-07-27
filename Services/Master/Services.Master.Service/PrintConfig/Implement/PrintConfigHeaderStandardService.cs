using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
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


        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _printConfigHeaderStandardActivityLog;

        public PrintConfigHeaderStandardService(MasterDBContext masterDBContext,
                ILogger<PrintConfigHeaderServiceAbstract<PrintConfigHeaderStandard,
                PrintConfigHeaderStandardModel, PrintConfigHeaderStandardViewModel>> logger,
                IMapper mapper,
                IActivityLogService activityLogService) :
                base(masterDBContext, logger, mapper, nameof(PrintConfigHeaderStandard.PrintConfigHeaderStandardId))
        {
            _printConfigHeaderStandardActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigHeaderStandard);
            _masterDBContext = masterDBContext;
        }

        public override async Task<bool> DeleteHeader(int headerId)
        {
            var printConfigStandardNames = await _masterDBContext.PrintConfigStandard
                .Where(s => s.PrintConfigHeaderStandardId == headerId)
                .Select(s => s.Title)
                .ToListAsync();

            string result = string.Join(", ", printConfigStandardNames.ConvertAll(x => $"\"{x}\""));

            if (await _masterDBContext.PrintConfigStandard.AnyAsync(s => s.PrintConfigHeaderStandardId == headerId))
                throw new BadRequestException($"Header đang được sử dụng tại phiếu in {result}");

            return await base.DeleteHeader(headerId);
        }

        protected override async Task LogAddPrintConfigHeader(PrintConfigHeaderStandardModel model, PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderStandardId)
                .JsonData(model)
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfigHeader(PrintConfigHeaderStandardModel model, PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderStandardId)
                .JsonData(model)
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfigHeader(PrintConfigHeaderStandard entity)
        {
            await _printConfigHeaderStandardActivityLog.LogBuilder(() => PrintConfigHeaderStandardActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.Title)
                 .ObjectId(entity.PrintConfigHeaderStandardId)
                 .JsonData(entity)
                 .CreateLog();
        }
    }
}
