using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Notification
{
    public class EmailConfigurationModel: IMapFrom<EmailConfiguration>
    {
        public string SmtpHost { get; set; }
        public int Port { get; set; }
        public string MailFrom { get; set; }
        public string Password { get; set; }
        public bool IsSsl { get; set; }
    }
}