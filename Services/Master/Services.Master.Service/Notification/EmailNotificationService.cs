using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Notification;

namespace VErp.Services.Master.Service.Notification
{
    public interface IEmailNotificationService
    {
        Task<bool> Dispatch(string[] mailTo, string subject, string message);
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailConfigurationService _emailConfigurationService;

        public EmailNotificationService(IEmailConfigurationService emailConfigurationService)
        {
            _emailConfigurationService = emailConfigurationService;
        }

        public async Task<bool> Dispatch(string[] mailTo, string subject, string message)
        {
            var config = await _emailConfigurationService.GetEmailConfiguration();
            var mailArguments = new MailArguments()
            {
                MailTo = mailTo,
                MailFrom = config.MailFrom,
                Message = message,
                Name = "VERP",
                Password = config.Password,
                Port = config.Port,
                SmtpHost = config.SmtpHost,
                Subject = subject
            };
            return await Dispatch(mailArguments, config.IsSsl, true);
        }

        private async Task<bool> Dispatch(MailArguments mailArgs, bool isSsl, bool isBodyHtml)
        {
            try
            {
                var networkCredential = new NetworkCredential
                {
                    Password = mailArgs.Password,
                    UserName = mailArgs.MailFrom
                };

                var mailMsg = new MailMessage
                {
                    Body = mailArgs.Message,
                    Subject = mailArgs.Subject,
                    IsBodyHtml = isBodyHtml
                };
                foreach (var mailTo in mailArgs.MailTo)
                    mailMsg.To.Add(mailTo);

                mailMsg.From = new MailAddress(mailArgs.MailFrom, mailArgs.Name);

                var smtpClient = new SmtpClient(mailArgs.SmtpHost)
                {
                    Port = Convert.ToInt32(mailArgs.Port),
                    EnableSsl = isSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,

                    UseDefaultCredentials = true,

                    Credentials = networkCredential
                };

                smtpClient.Send(mailMsg);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return await Task.FromResult(false);
            }
        }
    }
}