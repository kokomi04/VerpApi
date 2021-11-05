using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/notification/email")]
    public class EmailNotificationController : VErpBaseController
    {
        private readonly IEmailNotificationService _emailNotificationService;

        public EmailNotificationController(IEmailNotificationService emailNotificationService)
        {
            _emailNotificationService = emailNotificationService;
        }

        [HttpPost]
        [Route("")]
        public async Task<bool> Dispatch([FromBody] MailRequest model)
        {
            return await _emailNotificationService.Dispatch(model.MailTo, model.Subject, model.Message);
        }
    }
}