using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class ProductPurityCalc
{
    public int ProductPurityCalcId { get; set; }

    public int SubsidiaryId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string EvalSourceCodeJs { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }
}
