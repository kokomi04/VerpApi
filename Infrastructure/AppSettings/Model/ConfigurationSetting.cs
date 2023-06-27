namespace VErp.Infrastructure.AppSettings.Model
{
    public class ConfigurationSetting
    {
        public string SigninCert { get; set; }
        public string SigninCertPassword { get; set; }
        public string FileUploadFolder { get; set; }
        public long FileUploadMaxLength { get; set; }
        public long BackupStorageFolder { get; set; }
        public string InternalCrossServiceKey { get; set; }
        public string ExternalHelpApiKey { get; set;}
    }
}
