using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class FileConfiguration
{
    public int FileConfigurationId { get; set; }

    public long FileMaxLength { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }
}
