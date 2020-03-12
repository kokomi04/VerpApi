using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class RedisSetting
    {
        public string Endpoint { get; set; }
        public int? Port { get; set; }
        public bool Ssl { get; set; }
        public string AuthKey { get; set; }
    }
}
