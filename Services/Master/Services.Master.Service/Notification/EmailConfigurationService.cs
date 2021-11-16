using AutoMapper;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VErp.Services.Master.Service.Notification
{
    public interface IEmailConfigurationService
    {
        Task<EmailConfigurationModel> GetEmailConfiguration();
        Task<bool> UpdateEmailConfiguration(EmailConfigurationModel model);
    }

    public class EmailConfigurationService : IEmailConfigurationService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _guideActivityLog;
        private readonly IMapper _mapper;

        public EmailConfigurationService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
            _guideActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.EmailConfiguration);
        }

        public async Task<EmailConfigurationModel> GetEmailConfiguration()
        {
            var conf = await _masterDBContext.EmailConfiguration.AsNoTracking().FirstOrDefaultAsync();
            if (conf == null)
                return new EmailConfigurationModel();

            return _mapper.Map<EmailConfigurationModel>(conf);
        }

        public async Task<bool> UpdateEmailConfiguration(EmailConfigurationModel model)
        {
            var conf = await _masterDBContext.EmailConfiguration.FirstOrDefaultAsync();
            if (conf == null)
                _masterDBContext.EmailConfiguration.Add(_mapper.Map<EmailConfiguration>(model));
            else
                _mapper.Map(model, conf);

            await _masterDBContext.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}