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
        private readonly IPrintConfigHeaderCustomService _printConfigHeaderCustomService;

        public PrintConfigCustomService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<PrintConfigCustomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IDocOpenXmlService docOpenXmlService
            , IPrintConfigHeaderCustomService printConfigHeaderCustomService) :
            base(masterDBContext, appSetting, logger, mapper, docOpenXmlService, EnumObjectType.PrintConfigCustom, nameof(PrintConfigCustom.PrintConfigCustomId))
        {

            _printConfigCustomActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigCustom);
            _printConfigHeaderCustomService = printConfigHeaderCustomService;
        }

        protected override async Task LogAddPrintConfig(PrintConfigCustomModel model, PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigCustomId)
                .JsonData(model)
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfig(PrintConfigCustomModel model, PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.PrintConfigName)
                .ObjectId(entity.PrintConfigCustomId)
                .JsonData(model)
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfig(PrintConfigCustom entity)
        {
            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.PrintConfigName)
                 .ObjectId(entity.PrintConfigCustomId)
                 .JsonData(entity)
                 .CreateLog();
        }

        public override async Task<int> AddPrintConfig(PrintConfigCustomModel model, IFormFile template, IFormFile background)
        {
            if (!model.PrintConfigHeaderCustomId.HasValue && model.PrintConfigHeaderStandardId.HasValue)
            {
                var printConfigStandard = await _masterDBContext.PrintConfigStandard.FindAsync(model.PrintConfigStandardId);

                model.PrintConfigHeaderCustomId = await GetOrCreateHeaderCustom(printConfigStandard);
            }
               
            return await base.AddPrintConfig(model, template, background);
        }
       
        public async Task<bool> RollbackPrintConfigCustom(int printConfigCustomId)
        {
            var printConfigCustom = await _masterDBContext.PrintConfigCustom
                .Where(p => p.PrintConfigCustomId == printConfigCustomId)
                .FirstOrDefaultAsync();

            if (printConfigCustom == null)
                throw new BadRequestException(InputErrorCode.PrintConfigNotFound);
            if (!printConfigCustom.PrintConfigStandardId.HasValue || printConfigCustom.PrintConfigStandardId.Value <= 0)
                throw PrintConfigStandardEmpty.BadRequest(GeneralCode.InternalError);

            var printConfigStandard = await _masterDBContext.PrintConfigStandard
                .Where(x => x.PrintConfigStandardId == printConfigCustom.PrintConfigStandardId)
                .FirstOrDefaultAsync();

            var source = _mapper.Map<PrintConfigRollbackModel>(printConfigStandard);

            if (source == null)
                throw PrintConfigStandardNotFound.BadRequest(GeneralCode.InternalError);

            printConfigCustom.PrintConfigHeaderCustomId = await GetOrCreateHeaderCustom(printConfigStandard);

            var destProperties = printConfigCustom.GetType().GetProperties();
            foreach (var destProperty in destProperties)
            {
                var sourceProperty = source.GetType().GetProperty(destProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProperty != null && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    destProperty.SetValue(printConfigCustom, sourceProperty.GetValue(source, new object[] { }), new object[] { });
                }
            }

            var moduleTypeIds = await _masterDBContext.PrintConfigStandardModuleType
                .Where(m => m.PrintConfigStandardId == printConfigCustom.PrintConfigStandardId)
                .Select(m => m.ModuleTypeId)
                .ToListAsync();

            await UpdateMappingModuleTypes(moduleTypeIds, printConfigCustomId);

            await _masterDBContext.SaveChangesAsync();

            await _printConfigCustomActivityLog.LogBuilder(() => PrintConfigCustomActivityLogMessage.Delete)
            .MessageResourceFormatDatas(printConfigCustom.PrintConfigName)
            .ObjectId(printConfigCustom.PrintConfigCustomId)
            .JsonData(printConfigCustom)
            .CreateLog();

            return true;
        }

        private async Task<int?> GetOrCreateHeaderCustom(PrintConfigStandard printConfigStandard)
        {
            if (printConfigStandard != null && printConfigStandard.PrintConfigHeaderStandardId.HasValue)
            {
                var printConfigHeaderCustom = await _masterDBContext.PrintConfigHeaderCustom
                    .FirstOrDefaultAsync(c => c.PrintConfigHeaderStandardId == printConfigStandard.PrintConfigHeaderStandardId);

                if (printConfigHeaderCustom == null)
                {
                    var headerStandard = await _masterDBContext.PrintConfigHeaderStandard.FindAsync(printConfigStandard.PrintConfigHeaderStandardId);

                    var headerCustomModel = new PrintConfigHeaderCustomModel()
                    {
                        PrintConfigHeaderStandardId = headerStandard.PrintConfigHeaderStandardId,
                        PrintConfigHeaderCustomCode = headerStandard.PrintConfigHeaderStandardCode,
                        SortOrder = headerStandard.SortOrder,
                        IsShow = headerStandard.IsShow,
                        JsAction = headerStandard.JsAction,
                        Title = headerStandard.Title,
                    };

                    return await _printConfigHeaderCustomService.CreateHeader(headerCustomModel);
                }
                else
                {
                    return printConfigHeaderCustom.PrintConfigHeaderCustomId;
                }
            }

            return null;
        }

    }
}
