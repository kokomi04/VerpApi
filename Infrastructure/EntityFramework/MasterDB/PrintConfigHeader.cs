using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class PrintConfigHeader
{
    public int PrintConfigHeaderId { get; set; }

    public string Title { get; set; }

    public string PrintConfigHeaderCode { get; set; }

    public string JsAction { get; set; }

    public bool? IsShow { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<PrintConfigCustom> PrintConfigCustoms { get; set; } = new List<PrintConfigCustom>();
}
