using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class PrintConfigHeaderCustomService : 
            PrintConfigHeaderServiceAbstract<PrintConfigHeaderCustom, PrintConfigHeaderCustomModel, PrintConfigHeaderCustomViewModel>, 
            IPrintConfigHeaderCustomService

    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _printConfigHeaderCustomActivityLog;
        public PrintConfigHeaderCustomService(MasterDBContext masterDBContext, 
            ILogger<PrintConfigHeaderServiceAbstract<PrintConfigHeaderCustom, PrintConfigHeaderCustomModel, PrintConfigHeaderCustomViewModel>> logger, 
            IMapper mapper,
            IActivityLogService activityLogService) 
                : base(masterDBContext, logger, mapper, nameof(PrintConfigHeaderCustom.PrintConfigHeaderCustomId))
        {
            _logger = logger;
            _mapper = mapper;
            _masterDBContext = masterDBContext;
            _printConfigHeaderCustomActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PrintConfigHeaderCustom);
        }

        public async Task<bool> RollbackPrintConfigHeaderCustom(int printConfigHeaderCustomId)
        {
            using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var pCHC = await _masterDBContext.PrintConfigHeaderCustom.AsNoTracking()
                            .FirstOrDefaultAsync(x=>x.PrintConfigHeaderCustomId == printConfigHeaderCustomId);

                if (pCHC == null)
                    throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

                var pCHS = new PrintConfigHeaderStandard();

                if (pCHC.PrintConfigHeaderStandardId.HasValue)
                {
                    pCHS = await _masterDBContext.PrintConfigHeaderStandard.FindAsync(pCHC.PrintConfigHeaderStandardId);

                    if (pCHC == null)
                        throw new BadRequestException("Không tìm thấy header phiếu in gốc");
                }
                else
                    throw new BadRequestException("Cấu hình phiếu in không được cấu hình từ phiếu in gốc nào");

                var rollbackModel = _mapper.Map<PrintConfigHeaderRollbackModel>(pCHS);

                pCHC = _mapper.Map<PrintConfigHeaderCustom>(rollbackModel);

                pCHC.PrintConfigHeaderCustomId = printConfigHeaderCustomId;

                _masterDBContext.Update(pCHC);

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RollbackPrintConfigHeaderCustom");
                throw;
            }

        }

        protected override async Task LogAddPrintConfigHeader(PrintConfigHeaderCustomModel model, PrintConfigHeaderCustom entity)
        {
            await _printConfigHeaderCustomActivityLog.LogBuilder(() => PrintConfigHeaderCustomActivityLogMessage.Create)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderCustomId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogUpdatePrintConfigHeader(PrintConfigHeaderCustomModel model, PrintConfigHeaderCustom entity)
        {
            await _printConfigHeaderCustomActivityLog.LogBuilder(() => PrintConfigHeaderCustomActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.Title)
                .ObjectId(entity.PrintConfigHeaderCustomId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
        }
        protected override async Task LogDeletePrintConfigHeader(PrintConfigHeaderCustom entity)
        {
            await _printConfigHeaderCustomActivityLog.LogBuilder(() => PrintConfigHeaderCustomActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(entity.Title)
                 .ObjectId(entity.PrintConfigHeaderCustomId)
                 .JsonData(entity.JsonSerialize())
                 .CreateLog();
        }
    }
}
