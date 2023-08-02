using System;
using System.Collections.Generic;

namespace ActivityLogDB;

public partial class Subscription
{
    public long SubscriptionId { get; set; }

    public int ObjectTypeId { get; set; }

    public int? BillTypeId { get; set; }

    public int UserId { get; set; }

    public long ObjectId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }
}
