﻿namespace VErp.Infrastructure.AppSettings.Model
{
    public class AppSetting
    {
        public string ServiceName { get; set; }
        public int ServiceId { get; set; }
        public int Port { get; set; }
        public int HttpsPort { get; set; }
        public string PathBase { get; set; }
        public ConfigurationSetting Configuration { get; set; }
        public DatabaseConnectionSetting DatabaseConnections { get; set; }
        public string PasswordPepper { get; set; }
        public string FileUrlEncryptPepper { get; set; }
        public string ExtraFilterEncryptPepper { get; set; }
        public IdentitySetting Identity { get; set; }
        public ServiceUrlsModel ServiceUrls { get; set; }
        public LoggingSetting Logging { get; set; }

        public RedisSetting Redis { get; set; }

        public ElasticApmSetting ElasticApm { get; set; }
        public GrpcInternalSetting GrpcInternal { get; set; }
        public BackupStorageSetting BackupStorage { get; set; }
        public Developer Developer { get; set; }

        public PuppeteerPdfSetting PuppeteerPdf { get; set; }

        public WebPushSetting WebPush { get; set; }
    }
}
