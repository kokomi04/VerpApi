﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class ConfigurationSetting
    {
        public string ConfigFileKey { get; set; }
        public string SigninCert { get; set; }
        public string SigninCertPassword { get; set; }
    }
}
