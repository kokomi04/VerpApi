using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.FileConfig;

namespace VErp.Services.Master.Service.Notification
{
    public interface IFileConfigurationService
    {
        Task<FileConfigurationModel> GetFileConfiguration();
        Task<bool> UpdateFileConfiguration(FileConfigurationModel model);
    }

    public class FileConfigurationService : IFileConfigurationService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _guideActivityLog;
        private readonly IMapper _mapper;

        public FileConfigurationService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
            _guideActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.FileConfiguration);
        }

        public async Task<FileConfigurationModel> GetFileConfiguration()
        {
            var conf = await _masterDBContext.FileConfiguration.AsNoTracking().FirstOrDefaultAsync();
            if (conf == null)
                return new FileConfigurationModel();

            return _mapper.Map<FileConfigurationModel>(conf);
        }

        public async Task<bool> UpdateFileConfiguration(FileConfigurationModel model)
        {
            var conf = await _masterDBContext.FileConfiguration.FirstOrDefaultAsync();
            if (conf == null)
                _masterDBContext.FileConfiguration.Add(_mapper.Map<FileConfiguration>(model));
            else
                _mapper.Map(model, conf);

            await _masterDBContext.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}