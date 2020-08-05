using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class ElasticApmSetting
    {
        public bool IsEnabled { get; set; }
        public string SecretToken { get; set; }
        public string ServerUrls { get; set; }
        public string ServiceName { get; set; }
    }
}
