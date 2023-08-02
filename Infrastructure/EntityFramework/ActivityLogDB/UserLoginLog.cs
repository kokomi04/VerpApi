using System;
using System.Collections.Generic;

namespace ActivityLogDB;

public partial class UserLoginLog
{
    public long UserLoginLogId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; }

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public int Status { get; set; }

    public int MessageTypeId { get; set; }

    public string MessageResourceName { get; set; }

    public string MessageResourceFormatData { get; set; }

    public string Message { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public string StrSubId { get; set; }
}
