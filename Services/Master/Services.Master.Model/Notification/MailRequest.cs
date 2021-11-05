namespace VErp.Services.Master.Model.Notification
{
    public class MailRequest
    {
        public string[] MailTo { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}