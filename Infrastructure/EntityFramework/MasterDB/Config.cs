using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Config
    {
        public int ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string Description { get; set; }
    }
}
