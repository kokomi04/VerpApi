namespace VErp.Infrastructure.AppSettings.Model
{
    public class WebPushSetting
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string ActionUrl { get; set; }
        public bool EnableWorker {get; set; }
    }
}