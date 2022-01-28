using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.DataConfig;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class DataConfigService : IDataConfigService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _dataConfigActivityLog;


        public DataConfigService(MasterDBContext masterDbContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDbContext = masterDbContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _dataConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.DataConfig);
        }

        public async Task<DataConfigModel> GetConfig()
        {
            var info = await _masterDbContext.DataConfig.Where(c => c.SubsidiaryId == _currentContextService.SubsidiaryId).FirstOrDefaultAsync();
            if (info == null)
            {
                info = new DataConfig();
            }
            return _mapper.Map<DataConfigModel>(info);
        }

        public async Task<bool> UpdateConfig(DataConfigModel req)
        {
            var info = await _masterDbContext.DataConfig.FirstOrDefaultAsync();
            if (info == null)
            {
                info = _mapper.Map<DataConfig>(req);
                info.SubsidiaryId = _currentContextService.SubsidiaryId;
                await _masterDbContext.DataConfig.AddAsync(info);
            }
            else
            {
                _mapper.Map(req, info);
            }

            await _masterDbContext.SaveChangesAsync();

            await _dataConfigActivityLog.LogBuilder(() => DataConfigActivityLogMessage.Update)
               .ObjectId(info.SubsidiaryId)
               .JsonData(req.JsonSerialize())
               .CreateLog();

            return true;
        }
    }
}
