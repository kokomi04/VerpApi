using AutoMapper;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Services.Master.Service.Notification
{
    public interface IEmailConfigurationService
    {
        Task<EmailConfigurationModel> GetEmailConfiguration();
        Task<bool> UpdateEmailConfiguration(EmailConfigurationModel model);
        Task<bool> IsEnableEmailConfiguration();
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

            if (model.IsEnable && string.IsNullOrWhiteSpace(model.MailFrom))
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin địa chỉ mail là bắt buộc");

            if (model.IsEnable && model.Port == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin port là bắt buộc");

            if (model.IsEnable && string.IsNullOrWhiteSpace(model.SmtpHost))
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin host smtp là bắt buộc");

            if (model.IsEnable && string.IsNullOrWhiteSpace(model.Password))
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin mật khẩu là bắt buộc");

            if (conf == null)
            {
                _masterDBContext.EmailConfiguration.Add(_mapper.Map<EmailConfiguration>(model));
            }
            else
                _mapper.Map(model, conf);

            await _masterDBContext.SaveChangesAsync();

            return await Task.FromResult(true);
        }

        public async Task<bool> IsEnableEmailConfiguration()
        {
            var conf = await _masterDBContext.EmailConfiguration.AsNoTracking().FirstOrDefaultAsync();
            if (conf == null)
                return false;

            return conf.IsEnable;
        }
    }
}