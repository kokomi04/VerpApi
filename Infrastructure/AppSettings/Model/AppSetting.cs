using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class AppSetting
    {
        public string PathBase { get; set; }
        public ConfigurationSetting Configuration { get; set; }
        public DatabaseConnectionSetting DatabaseConnections { get; set; }
        public string PasswordPepper { get; set; }
        public IdentitySetting Identity { get; set; }        
    }
}
