using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/EmailConfiguration")]
    public class EmailConfigurationController : VErpBaseController
    {
        private readonly IEmailConfigurationService _emailConfigurationService;
        private readonly IMailTemplateService _mailTemplateService;

        public EmailConfigurationController(IEmailConfigurationService emailConfigurationService, IMailTemplateService mailTemplateService)
        {
            _emailConfigurationService = emailConfigurationService;
            _mailTemplateService = mailTemplateService;
        }

        [HttpGet]
        [Route("")]
        public async Task<EmailConfigurationModel> GetEmailConfiguration()
        {
            return await _emailConfigurationService.GetEmailConfiguration();
        }

        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateEmailConfiguration([FromBody] EmailConfigurationModel model)
        {
            return await _emailConfigurationService.UpdateEmailConfiguration(model);
        }

        [HttpGet]
        [Route("ready")]
        public async Task<bool> IsEnableEmailConfiguration()
        {
            return await _emailConfigurationService.IsEnableEmailConfiguration();
        }


        [HttpPost]
        [Route("template")]
        public async Task<int> AddMailTemplate([FromBody] MailTemplateModel model)
        {
            return await _mailTemplateService.AddMailTemplate(model);
        }

        [HttpDelete]
        [Route("template/{mailTemplateId}")]
        public async Task<bool> DelteMailTemplate([FromRoute] int mailTemplateId)
        {
            return await _mailTemplateService.DelteMailTemplate(mailTemplateId);
        }

        [HttpGet]
        [Route("template/list")]
        public async Task<IList<MailTemplateModel>> GetListMailTemplate()
        {
            return await _mailTemplateService.GetListMailTemplate();
        }

        [HttpGet]
        [Route("template/{mailTemplateId}")]
        public async Task<MailTemplateModel> GetMailTemplate([FromRoute] int mailTemplateId)
        {
            return await _mailTemplateService.GetMailTemplate(mailTemplateId);
        }

        [HttpPut]
        [Route("template/{mailTemplateId}")]
        public async Task<bool> UpdateMailTemplate([FromRoute] int mailTemplateId, [FromBody] MailTemplateModel model)
        {
            return await _mailTemplateService.UpdateMailTemplate(mailTemplateId, model);
        }

        [HttpGet]
        [Route("template/fields")]
        public async Task<IList<TemplateMailField>> GetTemplateMailFields()
        {
            return await _mailTemplateService.GetTemplateMailFields();
        }
    }
}