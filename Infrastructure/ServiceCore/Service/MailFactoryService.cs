using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using System.Linq;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IMailFactoryService
    {
        Task<bool> Dispatch<T>(string[] mailTo, string mailTemplateCode, InternalObjectDataMail<T> data) where T : class;
    }

    public class MailFactoryService : IMailFactoryService
    {
        private readonly IHttpCrossService _httpCrossService;

        public MailFactoryService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> Dispatch<T>(string[] mailTo, string mailTemplateCode, InternalObjectDataMail<T> data) where T : class
        {
            var config = await _httpCrossService.Get<EmailConfigSimpleModel>("api/internal/InternalEmailConfiguration");
            var mailTemplate = await _httpCrossService.Get<MailTemplateSimpleModel>($"api/internal/InternalEmailConfiguration/template?mailTemplateCode={mailTemplateCode}");

            var message = Regex.Replace(mailTemplate.Content, @"{{(?<PropertyName>[^}]+)}}", m =>
            {
                var propertyName = m.Groups["PropertyName"].Value;

                var internalType = typeof(InternalObjectDataMail<T>);

                if (internalType.GetProperties().Any(x => x.Name == propertyName) && propertyName != "Data")
                {
                    return internalType.GetProperty(propertyName).GetValue(data).ToString();
                }

                var dynamicType = typeof(T);
                if (dynamicType.GetProperties().Any(x => x.Name == propertyName))
                {
                    return dynamicType.GetProperty(propertyName).GetValue(data.Data).ToString();
                }

                return "";
            });

            var mailArguments = new MailArguments()
            {
                MailTo = mailTo,
                MailFrom = config.MailFrom,
                Message = message,
                Name = "",
                Password = config.Password,
                Port = config.Port,
                SmtpHost = config.SmtpHost,
                Subject = mailTemplate.Title
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
                    mailMsg.To.Add(mailTo.Trim());

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