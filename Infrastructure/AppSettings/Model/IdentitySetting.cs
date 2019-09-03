using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class IdentitySetting
    {
        public string Endpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiName { get; set; }
        public string ApiSecret { get; set; }
    }
}
