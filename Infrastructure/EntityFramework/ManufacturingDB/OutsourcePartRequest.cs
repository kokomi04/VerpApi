using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class OutsourcePartRequest
{
    public long OutsourcePartRequestId { get; set; }

    public string OutsourcePartRequestCode { get; set; }

    public long? ProductionOrderDetailId { get; set; }

    public DateTime? OutsourcePartRequestFinishDate { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public bool MarkInvalid { get; set; }

    public int OutsourcePartRequestStatusId { get; set; }

    public long? ProductionOrderId { get; set; }

    public virtual ICollection<OutsourcePartRequestDetail> OutsourcePartRequestDetail { get; set; } = new List<OutsourcePartRequestDetail>();

    public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
}
