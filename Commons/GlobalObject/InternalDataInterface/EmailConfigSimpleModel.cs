using VErp.Commons.GlobalObject;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class EmailConfigSimpleModel
    {
        public string SmtpHost { get; set; }
        public int Port { get; set; }
        public string MailFrom { get; set; }
        public string Password { get; set; }
        public bool IsSsl { get; set; }
    }
}