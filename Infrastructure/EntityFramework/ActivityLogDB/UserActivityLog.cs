﻿using System;
using System.Collections.Generic;

namespace ActivityLogDB;

public partial class UserActivityLog
{
    public long UserActivityLogId { get; set; }

    public int UserId { get; set; }

    public int? BillTypeId { get; set; }

    public long ObjectId { get; set; }

    public int ObjectTypeId { get; set; }

    public int? ActionId { get; set; }

    public int MessageTypeId { get; set; }

    public string MessageResourceName { get; set; }

    public string MessageResourceFormatData { get; set; }

    public string Message { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public string IpAddress { get; set; }
}
