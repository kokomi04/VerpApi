using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class EmailConfiguration
    {
        public int EmailConfigurationId { get; set; }
        public string SmtpHost { get; set; }
        public int Port { get; set; }
        public string MailFrom { get; set; }
        public string Password { get; set; }
        public bool? IsSsl { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
    }
}
